using Aetherforge.Domain;
using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

/// <summary>Opt-in author checks. Fixed simulation steps with real render frames;
/// synthetic presentation-only geometry/events are confined to this harness.</summary>
public partial class ExpeditionNumberLayoutCheck : Node
{
    public string EvidencePath { get; set; } = "";
    public string CaptureDirectory { get; set; } = "";
    private ExpeditionView _view = null!;
    private ExpeditionSimulation Sim => _view.Simulation;
    private ExpeditionStage Stage => _view.Stage;
    private ExpeditionVitals Hud => Stage.Hud;
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
    private readonly List<object> _samples = new();
    private readonly List<string> _checks = new();
    private readonly List<object> _transitionHealing = new();
    private readonly Stopwatch _wall = Stopwatch.StartNew();
    private int _assertions;
    private static object F(object obj, string name) => obj.GetType().GetField(name, Flags)!.GetValue(obj)!;
    private static T F<T>(object obj, string name) => (T)F(obj, name);
    private static void Set(object obj, string name, object value) => obj.GetType().GetField(name, Flags)!.SetValue(obj, value);
    private static object? Call(object obj, string name, params object[] args) => obj.GetType().GetMethod(name, Flags)!.Invoke(obj, args);
    private object[] Numbers => F<Array>(Hud, "_numbers").Cast<object>().ToArray();
    private object[] Plates => F<Array>(Hud, "_actors").Cast<object>().ToArray();
    private ExpeditionActor[] Enemies => F<List<ExpeditionActor>>(Stage, "_enemies").ToArray();
    public void Bind(ExpeditionView view) => _view = view;
    private void Require(bool condition, string message) { _assertions++; if (!condition) throw new InvalidOperationException(message); }
    private void Refresh() { Call(_view, "RefreshState", new object?[] { null }!); Stage._Process(0); }
    private void Step() { Sim.AdvanceDetailed(1.0 / 30); Refresh(); }
    private static object R(Rect2 r) => new { x = r.Position.X, y = r.Position.Y, w = r.Size.X, h = r.Size.Y };
    private void Sample(string label)
    {
        _samples.Add(new {
            label, simulation = Sim.Elapsed, wall = _wall.Elapsed.TotalSeconds, Sim.Paused, phase = Sim.Phase.ToString(),
            Sim.SessionId, Sim.AttemptId, Sim.StageIndex, Sim.EncounterAt,
            root = GetWindow().Size.ToString(), stage = Stage.Size.ToString(), hud = Hud.Size.ToString(),
            viewport = F<SubViewport>(Stage, "_viewport").Size.ToString(), transform = Hud.GetGlobalTransformWithCanvas().ToString(),
            scale = F<float>(Hud, "_scale"), fontSize = F<int>(Hud, "_numberSize"), safe = R(F<Rect2>(Hud, "_safe")),
            hovered = F<int>(Hud, "_hovered"), selected = Hud.SelectedMemberIndex, detailed = Hud.DetailedHud,
            occupied = F<Rect2[]>(Hud, "_occupied").Take(F<int>(Hud, "_occupiedCount")).Select(R).ToArray(),
            plates = Plates.Select((p, i) => new { actor = i, identity = F<ulong>(p, "Identity"), projected = F<bool>(p, "Projected"),
                visible = F<bool>(p, "Visible"), head = R(F<Rect2>(p, "Head")), vital = R(F<Rect2>(p, "Rect")),
                name = F<string>(p, "ShownName"), nameRect = R(F<Rect2>(p, "NameRect")) }).ToArray(),
            numbers = Numbers.Where(n => F<bool>(n, "Active")).Select(n => new {
                actor = F<int>(n, "Actor"), identity = F<ulong>(n, "Identity"), eventId = F<int>(n, "EventId"),
                at = F<double>(n, "At"), age = Sim.Elapsed - F<double>(n, "At"), text = F<string>(n, "Text"),
                heal = F<bool>(n, "Heal"), visible = F<bool>(n, "Visible"), candidate = F<int>(n, "Candidate"),
                pass = F<int>(n, "CoveragePass"), placement = F<string>(n, "Placement"), rise = F<float>(n, "Rise"),
                alpha = 1 - Math.Clamp(((Sim.Elapsed - F<double>(n, "At")) / .65 - .75) / .25, 0, 1),
                rect = R(F<Rect2>(n, "Rect")), travel = R(F<Rect2>(n, "Travel")), reservation = R(F<Rect2>(n, "Reservation")) }).ToArray(),
            enemies = Enemies.Select((e, i) => new { slot = i, identity = e.GetInstanceId(), pose = e.CurrentPose,
                rotation = e.GetNode<Node3D>("Visual").Rotation.Z }).ToArray()
        });
    }
    private async Task Capture(string name)
    {
        if (DisplayServer.GetName() == "headless" || CaptureDirectory.Length == 0) return;
        Directory.CreateDirectory(CaptureDirectory);
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        var root = GetViewport().GetTexture().GetImage();
        root.SavePng(Path.Combine(CaptureDirectory, name + "-root.png"));
        // The HUD is a Control above the 3D SubViewport: crop the ROOT, not its 3D texture.
        var rect = Stage.GetGlobalRect();
        root.GetRegion(new Rect2I((Vector2I)rect.Position, (Vector2I)rect.Size)).SavePng(Path.Combine(CaptureDirectory, name + "-hud.png"));
    }
    private object? Number(int actor, double at) => Numbers.FirstOrDefault(n => F<bool>(n, "Active") && F<int>(n, "Actor") == actor && F<double>(n, "At") == at);
    private void CheckGeometry(bool requireCoverage)
    {
        var shown = Numbers.Where(n => F<bool>(n, "Active") && F<bool>(n, "Visible")).ToArray();
        foreach (var n in shown)
        {
            var padded = F<Rect2>(n, "Reservation");
            Require(F<Rect2>(Hud, "_safe").Encloses(padded), "padded number trajectory inside safe area");
            Require(padded == F<Rect2>(n, "Travel").Grow(3 * BrassUi.Scale), "validated reservation matches padded trajectory");
            Require(F<Rect2>(n, "Travel").Encloses(F<Rect2>(n, "Rect")), "drawn number inside reserved trajectory");
            foreach (var p in Plates)
            {
                Require(!F<bool>(p, "Projected") || !padded.Intersects(F<Rect2>(p, "Head")), "number never crosses protected head");
                Require(!F<bool>(p, "Visible") || !padded.Intersects(F<Rect2>(p, "Rect").Grow(3 * BrassUi.Scale)), "number preserves vital row");
                Require(F<string>(p, "ShownName").Length == 0 || !padded.Intersects(F<Rect2>(p, "NameRect").Grow(3 * BrassUi.Scale)), "number preserves name and stats");
            }
            Require(F<ulong>(n, "Identity") == F<ulong>(Plates[F<int>(n, "Actor")], "Identity"), "number bound to sampled target identity");
            Require(Sim.Elapsed - F<double>(n, "At") < .65, "visible number keeps original lifetime");
            if (F<int>(n, "Candidate") >= 8)
                Require((bool)Call(Hud, "OwnsHorizontalBand", F<int>(n, "Actor"), F<Rect2>(n, "Travel"))!, "fallback belongs strictly to target horizontal cell");
        }
        for (int i = 0; i < shown.Length; i++)
            for (int j = i + 1; j < shown.Length; j++)
                Require(!F<Rect2>(shown[i], "Reservation").Intersects(F<Rect2>(shown[j], "Reservation")), "number channels do not overlap");
        if (requireCoverage)
            foreach (int actor in Numbers.Where(n => F<bool>(n, "Active")).Select(n => F<int>(n, "Actor")).Distinct())
                Require(shown.Any(n => F<int>(n, "Actor") == actor), "every natural active target has at least one visible number; actor=" + actor);
    }
    private async Task Natural()
    {
        Sim.Reset(); Refresh();
        var ids = Enemies.Select(e => e.GetInstanceId()).ToArray();
        double firstAt = -1; bool paused = false, expired = false, settled = false;
        int firstFrames = 0; var captures = new HashSet<int>();
        for (int tick = 0; tick < 900 && Sim.StageIndex == 0; tick++)
        {
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame); Step();
            var kill = Sim.Events.FirstOrDefault(e => e.Kind == "kill" && e.Target == 3);
            if (kill != null && firstAt < 0) firstAt = kill.At;
            // The encounter change can itself emit allied healing. Validate that frame
            // before retiring the old enemy-identity assertions.
            CheckGeometry(true);
            if (Sim.StageIndex != 0) break;
            Require(ids.SequenceEqual(Enemies.Select(e => e.GetInstanceId())), "natural DTO clones preserve visual identity");
            if (firstAt >= 0)
            {
                double age = Sim.Elapsed - firstAt;
                var n = Number(3, firstAt);
                if (age < .65)
                {
                    Require(n != null && F<bool>(n, "Visible") && F<string>(n, "Text") == "−10", "first rat killing -10 drawn throughout available lifetime");
                    Require(F<ulong>(n!, "Identity") == ids[0], "first killing number follows original rat");
                    firstFrames++;
                    int mark = age < .04 ? 0 : age >= .6 ? 3 : age >= .3 ? 2 : age >= .15 ? 1 : -1;
                    if (mark >= 0 && captures.Add(mark)) await Capture("kill-" + mark + "-" + Sim.Elapsed.ToString("F3", CultureInfo.InvariantCulture));
                    if (age >= .15 && !paused)
                    {
                        Sim.Paused = true; Refresh();
                        double elapsed = Sim.Elapsed; var rect = F<Rect2>(n!, "Rect"); int candidate = F<int>(n!, "Candidate");
                        var pose = Enemies[0].GetNode<Node3D>("Visual").Transform;
                        double wallAt = _wall.Elapsed.TotalSeconds;
                        for (int i = 0; i < 12; i++)
                        {
                            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame); Step();
                            Require(Sim.Elapsed == elapsed && F<Rect2>(n!, "Rect") == rect && F<int>(n!, "Candidate") == candidate && F<bool>(n!, "Visible"), "pause freezes age, placement and visibility");
                            Require(Enemies[0].GetNode<Node3D>("Visual").Transform == pose, "pause freezes fall");
                        }
                        Require(_wall.Elapsed.TotalSeconds > wallAt, "pause observed across real wall time");
                        Sample("natural-pause"); await Capture("paused-number");
                        await InspectPausedFeedback(n!);
                        Sim.Paused = false; paused = true;
                    }
                }
                else { Require(n == null, "first number expires on original simulation deadline"); expired = true; }
                if (age >= .7)
                {
                    Require(Math.Abs(Enemies[0].GetNode<Node3D>("Visual").Rotation.Z) > 1.35, "rat fall settles without replay");
                    if (!settled) await Capture("fall-settled"); settled = true;
                }
            }
            if (Numbers.Any(n => F<bool>(n, "Active")) || (firstAt >= 0 && Sim.Elapsed - firstAt < .75)) Sample("natural-fixed-rendered");
        }
        Require(firstFrames >= 19 && paused && expired && settled, "complete first number lifetime, pause/resume and fall exercised");
        Require(Sim.StageIndex == 1 && !ids.Intersect(Enemies.Select(e => e.GetInstanceId())).Any(), "next encounter replaces enemy identity");
        Require(!Numbers.Any(n => F<bool>(n, "Active") && F<int>(n, "Actor") >= 3), "old enemy number cannot reach next encounter");
        Sample("next-encounter"); await Capture("next-encounter");
        await FollowTransitionHealing();
        _checks.Add("natural fixed 1/30 simulation with render: first rat -10, all active target coverage, pause/resume, fall and next encounter");
    }

    private async Task FollowTransitionHealing()
    {
        var heal = Sim.Events.LastOrDefault(e => e.Kind == "heal" && e.Target == 0);
        Require(heal != null && heal.Amount == 36 && Math.Abs(Sim.Elapsed - heal.At) < .001,
            "transition precondition: natural guard +36 emitted on encounter change");
        ulong identity = F<ulong>(Plates[0], "Identity");
        int visibleFrames = 0;
        for (int tick = 0; tick < 24; tick++)
        {
            var number = Numbers.FirstOrDefault(n => F<bool>(n, "Active") && F<int>(n, "EventId") == heal!.Id);
            double age = Sim.Elapsed - heal!.At;
            bool visible = number != null && F<bool>(number, "Visible");
            _transitionHealing.Add(new { heal.Id, heal.Target, heal.At, deadline = heal.At + .65, age,
                elapsed = Sim.Elapsed, identity, sampledIdentity = F<ulong>(Plates[0], "Identity"),
                active = number != null, visible, numberAt = number == null ? (double?)null : F<double>(number, "At"),
                numberIdentity = number == null ? (ulong?)null : F<ulong>(number, "Identity"),
                root = GetWindow().Size.ToString(), stage = Stage.Size.ToString(), hud = Hud.Size.ToString(),
                rect = number == null ? null : R(F<Rect2>(number, "Rect")) });
            Sample(age < .65 ? "transition-heal-active" : "transition-heal-expired");
            CheckGeometry(true);
            Require(F<ulong>(Plates[0], "Identity") == identity, "guard identity survives encounter transition");
            if (age >= .65)
            {
                Require(number == null && visibleFrames >= 19, "transition healing expires at original .65s after visible lifetime");
                await Capture("transition-heal-expired");
                _checks.Add("natural transition guard +36 visible frames=" + visibleFrames + "; original At=" + heal.At.ToString("R", CultureInfo.InvariantCulture));
                return;
            }
            Require(visible, "natural transition guard +36 is drawn throughout original lifetime");
            Require(F<double>(number!, "At") == heal.At && F<ulong>(number!, "Identity") == identity &&
                F<int>(number!, "Actor") == 0 && F<string>(number!, "Text") == "+36", "transition heal retains event time, target, identity and text");
            if (visibleFrames == 0 || visibleFrames == 10 || age >= .6)
                await Capture("transition-heal-" + visibleFrames);
            visibleFrames++;
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame); Step();
        }
        Require(false, "transition healing must expire within original deadline");
    }

    private async Task InspectPausedFeedback(object number)
    {
        double at = F<double>(number, "At"), elapsed = Sim.Elapsed;
        Call(_view, "SelectMember", 1); Stage.DetailedHud = true;
        var head = F<Rect2>(Plates[4], "Head");
        Input.WarpMouse(Hud.GetGlobalTransformWithCanvas() * head.GetCenter());
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame); Refresh();
        Require(F<int>(Hud, "_hovered") == 4 && Hud.SelectedMemberIndex == 1 && Hud.DetailedHud, "automated inspection precondition: different selected member, hover living enemy, Details");
        CheckGeometry(true); Sample("paused-selection-hover-details"); await Capture("inspection-number");
        string Identification() => JsonSerializer.Serialize(Plates.Select(p => new {
            visible = F<bool>(p, "Visible"), vital = R(F<Rect2>(p, "Rect")), name = F<string>(p, "ShownName"), rect = R(F<Rect2>(p, "NameRect")) }));
        string withNumbers = Identification();
        var active = Numbers.Select(n => F<bool>(n, "Active")).ToArray();
        foreach (var n in Numbers) Set(n, "Active", false);
        Hud.Refresh();
        Require(Identification() == withNumbers, "HP and names identical with and without numbers under inspection");
        for (int i = 0; i < Numbers.Length; i++) Set(Numbers[i], "Active", active[i]);
        Hud.Refresh();
        Require(Identification() == withNumbers, "restoring numbers cannot displace HP/names");

        var size = GetWindow().Size; GetWindow().Size = size + new Vector2I(80, 40);
        for (int i = 0; i < 3; i++) await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        Refresh(); CheckGeometry(true); Sample("paused-resize"); await Capture("resized-number");
        Require(Sim.Elapsed == elapsed && F<double>(number, "At") == at, "inspection and resize never restart event clock");
        GetWindow().Size = size; Stage.DetailedHud = false; Call(_view, "SelectMember", 0); Input.WarpMouse(Vector2.One * 4);
        for (int i = 0; i < 3; i++) await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        Refresh(); CheckGeometry(true); Sample("paused-layout-restored");
        _checks.Add("automated selection/hover/Details and resize at frozen simulation time; HP/name rectangles match with and without numbers; not human input");
    }

    private void ClearNumbers() { foreach (var n in Numbers) { Set(n, "Active", false); Set(n, "Visible", false); Set(n, "Candidate", -1); } }
    private void Geometry(Rect2 head0, Rect2 head1, params Rect2[] blockers)
    {
        Set(Hud, "_safe", new Rect2(6, 6, 388, 188));
        for (int i = 0; i < Plates.Length; i++)
        {
            Set(Plates[i], "Projected", i < 2); Set(Plates[i], "Visible", false); Set(Plates[i], "ShownName", "");
            var head = i == 0 ? head0 : head1; Set(Plates[i], "Head", head);
            Set(Plates[i], "Anchor", new Vector2(head.GetCenter().X, head.Position.Y + 2));
        }
        Blockers(blockers);
    }
    private void Blockers(Rect2[] blockers)
    {
        var occupied = F<Rect2[]>(Hud, "_occupied"); Array.Copy(blockers, occupied, blockers.Length);
        Set(Hud, "_occupiedCount", blockers.Length);
    }
    private void Seed(object n, int target, int id, double at)
    {
        Set(n, "Active", true); Set(n, "Visible", false); Set(n, "Candidate", -1);
        Set(n, "Actor", target); Set(n, "Identity", F<ulong>(Plates[target], "Identity"));
        Set(n, "EventId", id); Set(n, "At", at); Set(n, "Heal", false); Set(n, "Text", "−10");
        var font = F<Font>(Hud, "_strong");
        Set(n, "Measure", font.GetStringSize("−10", HorizontalAlignment.Left, -1, F<int>(Hud, "_numberSize")));
        Set(n, "Reservation", default(Rect2));
    }
    private void Fixtures()
    {
        ClearNumbers();
        var n = Numbers; double at = Sim.Elapsed - .1;
        // A preferred top gap can miss the padded safe edge by a fraction of a pixel.
        // Stationary candidates may fit that edge only if all protected regions survive.
        Geometry(new Rect2(150, 45.5f, 30, 25), new Rect2(300, 100, 30, 25));
        Seed(n[0], 0, 10000, at);
        object[] candidateArgs = { n[0], 14, false, default(Rect2), default(Rect2), 0f, false };
        Require((bool)Call(Hud, "NumberCandidateFits", candidateArgs)!, "stationary fallback fits padded upper edge");
        var edge = (Rect2)candidateArgs[4];
        Require(F<Rect2>(Hud, "_safe").Encloses(edge) && Math.Abs(edge.Position.Y - 6) < .001 &&
            !edge.Intersects(F<Rect2>(Plates[0], "Head")), "edge fallback preserves safe margin and head");
        candidateArgs[1] = 8;
        Require(!(bool)Call(Hud, "NumberCandidateFits", candidateArgs)!, "moving candidate cannot borrow stationary edge clearance");
        candidateArgs[1] = 14;
        Blockers(new[] { new Rect2(140, 39, 70, 5) });
        Require(!(bool)Call(Hud, "NumberCandidateFits", candidateArgs)!, "edge fallback still rejects protected reservations");
        Blockers(Array.Empty<Rect2>());
        Set(Hud, "_safe", new Rect2(6, 6, 388, 30));
        Require(!(bool)Call(Hud, "NumberCandidateFits", candidateArgs)!, "too short safe area rejects edge fallback without invalid clamp");
        ClearNumbers();
        // Two safe holes, A and B. Existing target's second occupies B; new target
        // has only B. All other space is explicitly unavailable in this geometry fixture.
        var blockers = new[] { new Rect2(6, 6, 388, 64), new Rect2(6, 121, 388, 73),
            new Rect2(6, 70, 84, 51), new Rect2(147, 70, 36, 51), new Rect2(240, 70, 154, 51) };
        Geometry(new Rect2(150, 80, 30, 25), new Rect2(196.5f, 124, 30, 25), blockers);
        Seed(n[0], 0, 10001, at); Seed(n[1], 0, 10002, at);
        Call(Hud, "PlaceNumbers");
        Require(F<bool>(n[0], "Visible") && F<bool>(n[1], "Visible") && F<int>(n[1], "CoveragePass") == 2, "precondition existing target has two placed numbers");
        var shared = F<Rect2>(n[1], "Reservation"); Sample("fixture-before-new-target");
        Seed(n[2], 1, 10003, at); Blockers(blockers); Call(Hud, "PlaceNumbers");
        Require(F<bool>(n[2], "Visible") && F<int>(n[2], "CoveragePass") == 1, "new target gets coverage before an existing second");
        Require(F<Rect2>(n[2], "Reservation") == shared, "new target needs exactly the previously occupied shared channel");
        Require(!F<bool>(n[1], "Visible") && F<bool>(n[1], "Active"), "old second yields reservation and remains eligible pending");
        Require(F<double>(n[2], "At") == at && F<double>(n[1], "At") == at, "coverage transfer does not restart lifetime");
        Sample("fixture-second-yields-to-new-target");
        Set(n[2], "Active", false); Blockers(blockers); Call(Hud, "PlaceNumbers");
        Require(F<bool>(n[1], "Visible") && F<double>(n[1], "At") == at, "pending second returns with original age when space is released");

        Geometry(new Rect2(150, 80, 30, 25), new Rect2(150, 124, 30, 25));
        Require(!(bool)Call(Hud, "OwnsHorizontalBand", 0, new Rect2(150, 30, 30, 31))!, "equal head abscissas reject target 0");
        Require(!(bool)Call(Hud, "OwnsHorizontalBand", 1, new Rect2(150, 30, 30, 31))!, "equal head abscissas reject target 1 too");
        Geometry(new Rect2(150, 80, 60, 25), new Rect2(170, 124, 60, 25));
        Require(!(bool)Call(Hud, "OwnsHorizontalBand", 0, new Rect2(175, 30, 30, 31))!, "equal nearest distances reject midpoint association");
        Require((bool)Call(Hud, "OwnsHorizontalBand", 0, new Rect2(160, 30, 30, 31))!, "strictly nearer overlapping target accepted");

        ClearNumbers();
        // A 37-high safe hole accommodates one stationary 31-high box + margin,
        // and cannot accommodate any 45-high rising trajectory + margin.
        var small = new[] { new Rect2(6, 6, 388, 34), new Rect2(6, 77, 388, 117),
            new Rect2(6, 40, 130.5f, 37), new Rect2(193.5f, 40, 200.5f, 37) };
        Geometry(new Rect2(150, 80, 30, 25), new Rect2(300, 124, 30, 25), small);
        Seed(n[0], 0, 10004, at); Call(Hud, "PlaceNumbers");
        Require(F<bool>(n[0], "Visible") && F<float>(n[0], "Rise") == 0 && F<int>(n[0], "Candidate") >= 14, "stationary fallback works when no full travel fits");
        Require(F<double>(n[0], "At") == at, "stationary fallback preserves event age"); Sample("fixture-stationary-fallback");
        Set(n[0], "Measure", new Vector2(2000, 31)); Blockers(small); Call(Hud, "PlaceNumbers");
        Require(!F<bool>(n[0], "Visible") && F<bool>(n[0], "Active") && F<string>(n[0], "Placement") == "no-safe-space", "oversized value explicitly pending without false visibility");
        Require(F<double>(n[0], "At") == at, "no-space does not restart lifetime"); Sample("fixture-oversized-value");

        ClearNumbers();
        int eventId = 20000;
        ExpeditionEvent Event(int target, string kind, double time) => (ExpeditionEvent)Activator.CreateInstance(typeof(ExpeditionEvent), Flags, null,
            new object[] { ++eventId, kind, 0, target, 10f, time, "GAMELAB-30 synthetic presentation pool fixture" }, null)!;
        for (int target = 0; target < 5; target++)
        { Call(Hud, "AddNumber", Event(target, "hurt", at)); Call(Hud, "AddNumber", Event(target, "heal", at)); }
        Require(Numbers.Count(x => F<bool>(x, "Active")) == 10, "pool reaches ten entries");
        int healId = Numbers.Where(x => F<int>(x, "Actor") == 0 && F<bool>(x, "Heal")).Select(x => F<int>(x, "EventId")).Single();
        var third = Event(0, "hurt", Sim.Elapsed - .05); Call(Hud, "AddNumber", third);
        Require(Numbers.Count(x => F<bool>(x, "Active") && F<int>(x, "Actor") == 0) == 2, "third event preserves per-target cap");
        Require(Numbers.Any(x => F<int>(x, "EventId") == healId && F<bool>(x, "Heal")), "same-kind replacement preserves healing separately");
        Require(Numbers.Any(x => F<int>(x, "EventId") == third.Id && F<double>(x, "At") == third.At && F<int>(x, "Candidate") == -1), "replacement takes new event time and clears previous layout");
        _checks.Add("synthetic presentation fixtures: two existing / new target shared channel, second yields, pending retry original age, equal x / midpoint ties, stationary fallback, oversized value, pool 10 / third same-kind replacement");
    }
    public override async void _Ready()
    {
        bool passed = false; string error = "";
        try
        {
            _view.SetProcess(false); Stage.SetProcess(false); Engine.TimeScale = 1;
            for (int i = 0; i < 5; i++) await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            Input.WarpMouse(Vector2.One * 4);
            await Natural(); Fixtures(); passed = true;
            GD.Print("NUMBER LAYOUT CHECK PASS assertions=" + _assertions);
        }
        catch (Exception ex)
        {
            error = ex.ToString(); Sample("failure-state");
            await Capture("failure-state"); GD.PrintErr("NUMBER LAYOUT CHECK FAIL " + error);
        }
        finally
        {
            if (EvidencePath.Length > 0)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(EvidencePath))!);
                File.WriteAllText(EvidencePath, JsonSerializer.Serialize(new { passed, error, assertions = _assertions, checks = _checks, transitionHealing = _transitionHealing,
                    method = "author fixed 1/30 simulation; graphical frames when non-headless; synthetic fixtures labeled separately; no human input or physical DPI validation",
                    engine = Engine.GetVersionInfo().ToString(), display = DisplayServer.GetName(), renderer = RenderingServer.GetCurrentRenderingMethod().ToString(),
                    wall = _wall.Elapsed.TotalSeconds, samples = _samples }, new JsonSerializerOptions { WriteIndented = true }));
            }
            GetTree().Quit(passed ? 0 : 1);
        }
    }
}
