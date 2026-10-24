using Aetherforge.Domain;
using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

/// <summary>Opt-in isolated presentation regression. Normal run advances once per engine
/// frame at 1x; fast mode is for correctness only and does not certify wall timing.</summary>
public partial class ExpeditionPresentationCheck : Node
{
    public string EvidencePath { get; set; } = "";
    public string CaptureDirectory { get; set; } = "";
    public bool Fast { get; set; }
    private ExpeditionView _view = null!;
    private ExpeditionSimulation Sim => _view.Simulation;
    private ExpeditionStage Stage => _view.Stage;
    private readonly Stopwatch _wall = Stopwatch.StartNew();
    private readonly List<object> _samples = new();
    private readonly List<string> _checks = new();
    private int _assertions;
    private static object Field(object source, string name) => source.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.GetValue(source)!;
    private static T Field<T>(object source, string name) => (T)Field(source, name);
    private ExpeditionActor[] Enemies => Field<List<ExpeditionActor>>(Stage, "_enemies").ToArray();
    private object[] Numbers => Field<Array>(Stage.Hud, "_numbers").Cast<object>().ToArray();
    private ulong[] EffectIds => Field<IEnumerable>(Stage, "_effects").Cast<object>().Select(f => Field<Node>(f, "Node").GetInstanceId()).ToArray();
    private object? Number(int actor, double at) => Numbers.FirstOrDefault(n => Field<bool>(n, "Active") && Field<int>(n, "Actor") == actor && Field<double>(n, "At") == at);
    public void Bind(ExpeditionView view) => _view = view;
    private void Require(bool ok, string message)
    {
        _assertions++;
        if (!ok) throw new InvalidOperationException(message);
    }
    private async Task Frame(bool advance = true)
    {
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        if (advance) Sim.AdvanceDetailed(Fast ? 1.0 / 30 : GetProcessDeltaTime());
        typeof(ExpeditionView).GetMethod("RefreshState", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(_view, new object?[] { null });
        Stage._Process(0);
    }
    private void Sample(string label)
    {
        _samples.Add(new {
            label, wallSeconds = _wall.Elapsed.TotalSeconds, simulationSeconds = Sim.Elapsed,
            Sim.SessionId, Sim.AttemptId, Sim.StageIndex, Sim.EncounterAt, phase = Sim.Phase.ToString(), Sim.Paused,
            enemies = Enemies.Select((a, i) => new {
                slot = i, id = a.GetInstanceId(), Sim.Enemies[i].Health, a.CurrentPose,
                hurtAt = Field<double>(a, "_hurtAt"), deathAt = Field<double>(a, "_deathAt"),
                visualRotationZ = a.GetNode<Node3D>("Visual").Rotation.Z,
                visual = a.GetNode<Node3D>("Visual").Transform.ToString()
            }).ToArray(),
            numbers = Numbers.Where(n => Field<bool>(n, "Active")).Select(n => new {
                actor = Field<int>(n, "Actor"), identity = Field<ulong>(n, "Identity"), at = Field<double>(n, "At"),
                visible = Field<bool>(n, "Visible"), text = Field<string>(n, "Text"), rect = Field<Rect2>(n, "Rect").ToString()
            }).ToArray(), effects = EffectIds
        });
    }
    private async Task Capture(string name)
    {
        if (CaptureDirectory.Length == 0 || DisplayServer.GetName() == "headless") return;
        Directory.CreateDirectory(CaptureDirectory);
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        Stage.GetViewport().GetTexture().GetImage().SavePng(Path.Combine(CaptureDirectory, name + "-root.png"));
        Field<SubViewport>(Stage, "_viewport").GetTexture().GetImage().SavePng(Path.Combine(CaptureDirectory, name + "-stage.png"));
    }
    public override async void _Ready()
    {
        bool passed = false;
        string error = "";
        try
        {
            Engine.TimeScale = 1;
            _view.SetProcess(false); // Only this harness advances its isolated in-memory simulation.
            for (int i = 0; i < 4; i++) await Frame(false);
            var ids = Enemies.Select(a => a.GetInstanceId()).ToArray();
            var kills = new Dictionary<int, double>();
            int lastEvent = Sim.Events[^1].Id;
            bool paused = false, pausedVisibleNumber = false, completedFall = false, numberExpired = false, visibleNumber = false;
            double killAt = -1;
            int killTarget = -1;
            while (Sim.StageIndex == 0)
            {
                var previousDto = Sim.Enemies[0];
                await Frame();
                if (Sim.StageIndex != 0) break;
                Require(!ReferenceEquals(previousDto, Sim.Enemies[0]), "AdvanceDetailed must still publish cloned enemy DTOs");
                Require(ids.SequenceEqual(Enemies.Select(a => a.GetInstanceId())), "visual IDs must survive normal transactional advances");
                Require(ReferenceEquals(Field<List<ExpeditionEnemy>>(Stage, "_enemyData")[0], Sim.Enemies[0]), "stage must refresh its current DTO");
                foreach (var e in Sim.Events.Where(e => e.Id > lastEvent))
                {
                    lastEvent = e.Id;
                    if (e.Target < 3) continue;
                    if (e.Kind == "hurt") Require(Field<double>(Enemies[e.Target - 3], "_hurtAt") == e.At, "hurt reaches the correct enemy");
                    if (e.Kind == "kill")
                    {
                        kills.Add(e.Target, e.At);
                        Require(Field<double>(Enemies[e.Target - 3], "_deathAt") == e.At, "kill is anchored to the resolved event time");
                        if (killAt < 0) { killAt = e.At; killTarget = e.Target; await Capture("kill-start"); }
                    }
                }
                foreach (var kill in kills)
                {
                    Require(Field<double>(Enemies[kill.Key - 3], "_deathAt") == kill.Value, "DTO updates must not reapply kill");
                    var damageNumber = Number(kill.Key, kill.Value);
                    double numberAge = Sim.Elapsed - kill.Value;
                    Require(numberAge < .65 ? damageNumber != null : damageNumber == null, "each killing number has a 0.65s simulation lifetime, including the loot phase");
                    if (kill.Key == 4 && numberAge < .65)
                    {
                        bool shown = Field<bool>(damageNumber!, "Visible");
                        if (DisplayServer.GetName() != "headless") Require(shown, "second enemy killing number stays visible in its available HUD lane");
                        visibleNumber |= shown;
                        if (numberAge >= .15 && !pausedVisibleNumber)
                        {
                            await PauseFeedback(kill.Key, kill.Value, "paused-visible-number");
                            pausedVisibleNumber = true;
                        }
                    }
                }
                var effects = EffectIds;
                Stage._Process(0); Stage._Process(0);
                Require(effects.SequenceEqual(EffectIds), "resampling an event must not duplicate hurt effects");
                if (killAt >= 0)
                {
                    double age = Sim.Elapsed - killAt;
                    var number = Number(killTarget, killAt);
                    if (age < .65)
                    {
                        Require(number != null, "killing damage number must survive until 0.65 simulation seconds");
                        Require(Field<ulong>(number!, "Identity") == ids[killTarget - 3], "number follows its logical target");
                    }
                    else
                    {
                        Require(number == null, "number expires at 0.65 simulation seconds");
                        numberExpired = true;
                    }
                    if (age >= .15 && !paused)
                    {
                        await PauseFeedback(killTarget, killAt, "paused-fall-number");
                        paused = true;
                    }
                    if (age >= .25 && age <= .4)
                        Require(Math.Abs(Enemies[killTarget - 3].GetNode<Node3D>("Visual").Rotation.Z) is > .4f and < 1.1f, "fall passes through an intermediate pose before settling");
                    if (age >= .7 && !completedFall)
                    {
                        Require(Math.Abs(Enemies[killTarget - 3].GetNode<Node3D>("Visual").Rotation.Z) > 1.35, "fall reaches the horizontal pose after 0.65 seconds");
                        await Capture("fall-settled"); completedFall = true;
                    }
                    if (completedFall) Require(Math.Abs(Enemies[killTarget - 3].GetNode<Node3D>("Visual").Rotation.Z) > 1.35, "corpse remains fallen until encounter retirement");
                }
                Sample("normal-1x");
                Require(_wall.Elapsed.TotalSeconds < 45, "first encounter timeout");
            }
            Require(paused && pausedVisibleNumber && completedFall && numberExpired, "fall, number pause/resume and lifetime were exercised");
            if (DisplayServer.GetName() != "headless") Require(visibleNumber, "visible enemy number exercised by graphical run");
            Require(!ids.Intersect(Enemies.Select(a => a.GetInstanceId())).Any(), "new encounter replaces all enemies");
            Require(!Numbers.Any(n => Field<bool>(n, "Active") && Field<int>(n, "Actor") >= 3), "new encounter removes old enemy numbers");
            Sample("new-encounter"); _checks.Add("normal clones, event deduplication, fall, number target/lifetime, pause/resume, next encounter");
            await Capture("next-encounter");

            // Same elapsed/event cursor: these resets cannot be detected by a clock rewind.
            Sim.RestartRoute(); Sim.AdvanceDetailed(8); Stage._Process(0);
            ids = Enemies.Select(a => a.GetInstanceId()).ToArray();
            Sim.RestartRoute(); Sim.AdvanceDetailed(8); Stage._Process(0);
            CheckReset(ids, "attempt reset with equal elapsed and retained event IDs");
            ids = Enemies.Select(a => a.GetInstanceId()).ToArray();
            Sim.Reset(); Sim.AdvanceDetailed(8); Stage._Process(0);
            CheckReset(ids, "session reset with equal elapsed");
            // Fixture: replace/reorder/remove real members, retaining the encounter coordinates.
            var data = Field<List<ExpeditionEnemy>>(Sim, "_enemies");
            ids = Enemies.Select(a => a.GetInstanceId()).ToArray();
            data.Reverse(); Stage._Process(0); CheckReset(ids, "ordered composition changed at equal encounter coordinates");
            ids = Enemies.Select(a => a.GetInstanceId()).ToArray();
            data.RemoveAt(1); Stage._Process(0); CheckReset(ids, "composition count changed");
            Require(Enemies.Length == 1, "removed slot must be retired");
            ids = Enemies.Select(a => a.GetInstanceId()).ToArray();
            Stage.Bind(Sim); Stage._Process(0); CheckReset(ids, "bind running simulation skips history");
            _checks.Add("attempt/session reset, reordered/removed composition, bind history isolation");
            passed = true;
            GD.Print("PRESENTATION CHECK PASS assertions=" + _assertions);
        }
        catch (Exception ex) { error = ex.ToString(); GD.PrintErr("PRESENTATION CHECK FAIL " + error); }
        finally
        {
            if (EvidencePath.Length > 0)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(EvidencePath))!);
                File.WriteAllText(EvidencePath, JsonSerializer.Serialize(new { passed, error, fast = Fast, assertions = _assertions, checks = _checks, wallSeconds = _wall.Elapsed.TotalSeconds,
                    engine = Engine.GetVersionInfo().ToString(), renderer = RenderingServer.GetCurrentRenderingMethod(), display = DisplayServer.GetName(),
                    window = GetWindow().Size.ToString(), stage = Stage.Size.ToString(), samples = _samples }, new JsonSerializerOptions { WriteIndented = true }));
            }
            GetTree().Quit(passed ? 0 : 1);
        }
    }
    private void CheckReset(ulong[] previous, string label)
    {
        Require(!previous.Intersect(Enemies.Select(a => a.GetInstanceId())).Any(), label + ": new actor IDs");
        Require(!Numbers.Any(n => Field<bool>(n, "Active")), label + ": no old numbers");
        Require(EffectIds.Length == 0, label + ": no old event effects");
        Require(Enemies.All(a => Field<double>(a, "_hurtAt") == -100 && Field<double>(a, "_deathAt") == -1), label + ": no old hurt/death pose");
        Sample(label);
    }
    private async Task PauseFeedback(int target, double at, string label)
    {
        Sim.Paused = true; Stage._Process(0);
        var pose = Enemies[target - 3].GetNode<Node3D>("Visual").Transform;
        double elapsed = Sim.Elapsed;
        var number = Number(target, at)!;
        var rect = Field<Rect2>(number, "Rect");
        bool visible = Field<bool>(number, "Visible");
        var pauseWall = _wall.Elapsed.TotalSeconds;
        int frames = 0;
        do
        {
            await Frame(); frames++;
            Require(Sim.Elapsed == elapsed, "pause freezes simulation time");
            Require(Enemies[target - 3].GetNode<Node3D>("Visual").Transform == pose, "pause freezes fall transform");
            var current = Number(target, at);
            Require(current != null && Field<Rect2>(current, "Rect") == rect && Field<bool>(current, "Visible") == visible, "pause freezes number placement, visibility and lifetime");
            Sample(label);
        } while (Fast ? frames < 12 : _wall.Elapsed.TotalSeconds - pauseWall < .4);
        await Capture(label);
        Sim.Paused = false;
    }
}
