using Godot;
using Aetherforge.Domain;
using System;
using System.Collections.Generic;

/// <summary>
/// Read-only presentation of ExpeditionSimulation. The view owns Advance; this stage
/// samples Elapsed, absolute Distance and real events, including while paused.
/// </summary>
public partial class ExpeditionStage : SubViewportContainer
{
    private ExpeditionSimulation _simulation = null!;
    private SubViewport _viewport = null!;
    private Node3D _world = null!;
    private Camera3D _camera = null!;
    private ExpeditionGeometry _geometry = null!;
    private ExpeditionVitals _vitals = null!;
    public ExpeditionVitals Hud => _vitals;
    private bool _detailedHud;
    private int _selectedMemberIndex;

    public bool DetailedHud
    {
        get => _detailedHud;
        set { _detailedHud = value; if (_vitals != null) _vitals.DetailedHud = value; }
    }
    public int SelectedMemberIndex
    {
        get => _selectedMemberIndex;
        set { _selectedMemberIndex = Math.Clamp(value, 0, 2); if (_vitals != null) _vitals.SelectedMemberIndex = _selectedMemberIndex; }
    }
    private readonly ExpeditionActor[] _party = new ExpeditionActor[3];
    private readonly PartyMember?[] _partyData = new PartyMember?[3];
    private readonly List<ExpeditionActor> _enemies = new();
    private readonly List<ExpeditionEnemy> _enemyData = new();
    private readonly List<Module> _modules = new();
    private readonly List<Effect> _effects = new();
    private int _lastEvent, _encounterStage = -1;
    private double _lastTime = -1, _encounterAt = double.NaN;
    private float _cameraX;
    private bool _snapCamera = true, _resetRequested;
    private const float ModuleLength = 6;
    private const int ModuleCount = 8;

    private sealed class Module
    {
        internal Node3D Root = null!;
        internal int Ordinal = int.MinValue;
        internal readonly List<Node3D> Variants = new();
        internal Label3D Plate = null!;
    }

    private sealed class Effect
    {
        internal Node3D Node = null!;
        internal Vector3 From, To, Velocity;
        internal double At;
        internal float Life;
        internal string Kind = "";
    }

    public void Bind(ExpeditionSimulation simulation)
    {
        _simulation = simulation ?? throw new ArgumentNullException(nameof(simulation));
        if (_vitals != null) _vitals.Bind(_simulation, _camera, _viewport, _party, _enemies);
        _resetRequested = true;
    }

    public override void _Ready()
    {
        Stretch = true;
        MouseFilter = MouseFilterEnum.Ignore;
        _geometry = new ExpeditionGeometry();
        _viewport = new SubViewport
        {
            Name = "ExpeditionViewport", OwnWorld3D = true, TransparentBg = false,
            Msaa3D = Viewport.Msaa.Msaa2X,
            RenderTargetUpdateMode = SubViewport.UpdateMode.WhenParentVisible,
            Size = new Vector2I(1000, 320)
        };
        AddChild(_viewport);
        _world = new Node3D { Name = "ServiceGallery" };
        _viewport.AddChild(_world);
        BuildLighting();
        _camera = new Camera3D
        {
            Name = "TrackingOrthographicCamera", Projection = Camera3D.ProjectionType.Orthogonal,
            KeepAspect = Camera3D.KeepAspectEnum.Height, Size = 5.6f, Near = .1f, Far = 65, Current = true
        };
        _world.AddChild(_camera);
        for (int i = 0; i < ModuleCount; i++) _modules.Add(BuildModule(i));
        string[] kinds = { "guard", "gunner", "healer" };
        for (int i = 0; i < _party.Length; i++)
        {
            _party[i] = new ExpeditionActor { Name = "Party_" + kinds[i] };
            _world.AddChild(_party[i]); _party[i].Build(_geometry, kinds[i], i);
        }
        _vitals = new ExpeditionVitals
        {
            Name = "ActorVitals", DetailedHud = DetailedHud, SelectedMemberIndex = SelectedMemberIndex
        };
        _vitals.Bind(_simulation, _camera, _viewport, _party, _enemies); AddChild(_vitals);
        Resized += ResizeViewport;
        ResizeViewport();
        if (_simulation == null) GD.PushWarning("ExpeditionStage needs Bind(simulation) before AddChild.");
        else Sample();
    }

    public override void _Process(double delta)
    {
        // Intentionally ignore engine delta. Pausing must freeze camera, poses and all effects.
        if (_world != null && _simulation != null) Sample();
    }

    private void ResizeViewport()
    {
        if (_viewport == null) return;
        // Stretch owns the direct SubViewport size; changing it manually is rejected by Godot.
        // Only adapt camera framing to the effective Control rect here.
        if (_camera != null)
        {
            float aspect = Math.Max(.6f, Size.X / Math.Max(2, Size.Y));
            // Fit the whole formation and an encounter even in a narrower embedded review panel.
            _camera.Size = Math.Max(5.0f, 14.2f / aspect);
        }
    }

    private void BuildLighting()
    {
        _world.AddChild(new WorldEnvironment
        {
            Environment = new Godot.Environment
            {
                BackgroundMode = Godot.Environment.BGMode.Color,
                BackgroundColor = new Color("#241F1A"),
                AmbientLightSource = Godot.Environment.AmbientSource.Color,
                AmbientLightColor = new Color("#BFB49E"), AmbientLightEnergy = .68f,
                ReflectedLightSource = Godot.Environment.ReflectionSource.Disabled,
                TonemapMode = Godot.Environment.ToneMapper.Filmic,
                FogEnabled = false, GlowEnabled = false, SsaoEnabled = false
            }
        });
        _world.AddChild(new DirectionalLight3D
        {
            Name = "WarmKey", RotationDegrees = new Vector3(-42, -28, -12),
            LightColor = new Color("#F2D3A0"), LightEnergy = 1.18f, ShadowEnabled = false
        });
        _world.AddChild(new DirectionalLight3D
        {
            Name = "LinenFill", RotationDegrees = new Vector3(-26, 145, 0),
            LightColor = new Color("#B6A080"), LightEnergy = .48f, ShadowEnabled = false
        });
    }

    private void Sample()
    {
        var sim = _simulation;
        int maxId = sim.Events.Count == 0 ? 0 : sim.Events[sim.Events.Count - 1].Id;
        bool replacedParty = false;
        for (int i = 0; i < _partyData.Length; i++)
            if (_partyData[i] != null && !ReferenceEquals(_partyData[i], sim.Party[i])) replacedParty = true;
        if (_resetRequested || replacedParty || sim.Elapsed < _lastTime || maxId < _lastEvent)
        {
            ResetPresentation(); _resetRequested = false;
        }
        float dt = _lastTime < 0 ? 0 : (float)Math.Max(0, sim.Elapsed - _lastTime);
        _lastTime = sim.Elapsed;
        SyncEnemies();
        for (int i = 0; i < _party.Length; i++)
        {
            _partyData[i] = sim.Party[i];
            _party[i].Position = new Vector3((float)sim.Distance - i * 1.65f, 0, i * .12f);
        }
        for (int i = 0; i < _enemies.Count; i++)
            _enemies[i].Position = new Vector3((float)sim.EncounterAt + 3.6f + i * 1.5f, 0, 0);

        foreach (var e in sim.Events)
        {
            if (e.Id <= _lastEvent) continue;
            _lastEvent = e.Id;
            // Binding an already-running simulation must not replay its retained event history.
            if (sim.Elapsed - e.At <= 1.4 && e.At <= sim.Elapsed) Present(e);
        }
        bool march = sim.Phase == ExpeditionPhase.March;
        bool settled = sim.Phase == ExpeditionPhase.Complete || sim.Phase == ExpeditionPhase.Defeat || sim.Phase == ExpeditionPhase.Retreated;
        for (int i = 0; i < _party.Length; i++)
            _party[i].Pose(sim.Elapsed, sim.Distance, march, sim.Party[i].Health > 0, i == 0 && sim.ShieldRemaining > 0, settled);
        for (int i = 0; i < _enemies.Count; i++)
            _enemies[i].Pose(sim.Elapsed, 0, false, sim.Enemies[i].Health > 0, false, settled);
        if (settled)
        {
            // Terminal Elapsed is frozen; retire transient effects rather than retain them forever.
            foreach (var fx in _effects) Retire(fx.Node);
            _effects.Clear();
        }
        else UpdateEffects(sim.Elapsed);
        TrackCamera(dt);
        RecycleModules();
        _vitals.Refresh();
    }

    private void ResetPresentation()
    {
        _vitals.ResetFeedback();
        foreach (var actor in _party) actor.ResetPose();
        ClearEnemies();
        foreach (var fx in _effects) Retire(fx.Node);
        _effects.Clear(); _lastEvent = 0; _lastTime = -1;
        _encounterStage = -1; _encounterAt = double.NaN; _snapCamera = true;
        foreach (var module in _modules) module.Ordinal = int.MinValue;
    }

    private static void Retire(Node node)
    {
        // Detach immediately, so a reset never shows a queued corpse/effect for one more frame.
        node.GetParent()?.RemoveChild(node); node.QueueFree();
    }

    private void ClearEnemies()
    {
        foreach (var enemy in _enemies) Retire(enemy);
        _enemies.Clear(); _enemyData.Clear();
    }

    private void SyncEnemies()
    {
        var sim = _simulation;
        bool changed = _encounterStage != sim.StageIndex || _encounterAt != sim.EncounterAt || _enemyData.Count != sim.Enemies.Count;
        if (!changed)
            for (int i = 0; i < _enemyData.Count; i++)
                if (!ReferenceEquals(_enemyData[i], sim.Enemies[i])) { changed = true; break; }
        if (!changed) return;
        ClearEnemies();
        _encounterStage = sim.StageIndex; _encounterAt = sim.EncounterAt;
        for (int i = 0; i < sim.Enemies.Count; i++)
        {
            var actor = new ExpeditionActor { Name = "Enemy_" + i + "_" + sim.Enemies[i].Kind };
            _world.AddChild(actor); actor.Build(_geometry, sim.Enemies[i].Kind, i + 3);
            _enemies.Add(actor); _enemyData.Add(sim.Enemies[i]);
        }
    }

    private ExpeditionActor? Actor(int id) => id >= 0 && id < 3 ? _party[id] : id >= 3 && id - 3 < _enemies.Count ? _enemies[id - 3] : null;

    private void Present(ExpeditionEvent e)
    {
        var actor = Actor(e.Actor); var target = Actor(e.Target);
        switch (e.Kind)
        {
            case "attack":
                if (actor == null || target == null) break;
                actor.AimAttack(target.GlobalPosition); actor.OnEvent("attack", e.At);
                if (actor.Kind == "gunner")
                {
                    Projectile(actor.Muzzle, target.AimPoint, e.At, .11f, "bullet", "lamp");
                    Burst(actor.Muzzle, e.At, "lamp", 3, .12f);
                }
                else if (actor.Kind == "healer")
                    Projectile(actor.Muzzle, target.AimPoint, e.At, .34f, "infusion", "jade");
                else if (actor.Kind == "watcher" || actor.Kind == "drone")
                    Projectile(actor.Muzzle, target.AimPoint, e.At + .10, .12f, "beam", "ember");
                else
                    Burst(target.AimPoint, e.At + .30, "brass", actor.Kind == "foreman" ? 6 : 3, .28f);
                break;
            case "hurt":
                if (target == null) break;
                target.OnEvent("hurt", e.At);
                Burst(target.AimPoint, e.At, "brass", 3, .25f);
                break;
            case "heal":
                if (actor != null)
                {
                    actor.OnEvent("heal", e.At);
                    if (target != null) Projectile(actor.Muzzle, target.AimPoint, e.At, .52f, "infusion", "jade");
                }
                if (target != null)
                {
                    Ring(target.GlobalPosition + Vector3.Up * .10f, e.At, "jade", "heal");
                }
                break;
            case "shield":
                var guard = actor ?? _party[0];
                guard.OnEvent("shield", e.At);
                Ring(guard.AimPoint, e.At, "ochre", "shield");
                break;
            case "kill":
                target?.OnEvent("kill", e.At);
                break;
        }
    }

    private void AddEffect(Effect effect)
    {
        // Presentation is bounded even if a producer sends a large event batch.
        if (_effects.Count >= 96) { Retire(_effects[0].Node); _effects.RemoveAt(0); }
        _world.AddChild(effect.Node); effect.Node.Position = effect.From; _effects.Add(effect);
    }

    private void Projectile(Vector3 from, Vector3 to, double at, float life, string kind, string mat)
    {
        var root = new Node3D { Name = "Event_" + kind };
        _geometry.Part(root, kind == "beam" ? "cylinder" : "sphere", mat, Vector3.Zero,
            kind == "beam" ? Vector3.One : kind == "bullet" ? new Vector3(.22f, .035f, .035f) : Vector3.One * .085f);
        AddEffect(new Effect { Node = root, From = from, To = to, At = at, Life = life, Kind = kind });
    }

    private void Burst(Vector3 at, double time, string mat, int count, float life)
    {
        for (int i = 0; i < count; i++)
        {
            var root = new Node3D { Name = "EventSpark" };
            _geometry.Part(root, "sphere", mat, Vector3.Zero, new Vector3(.035f, .075f, .035f));
            float angle = i * 2.39996f;
            AddEffect(new Effect
            {
                Node = root, From = at, At = time, Life = life, Kind = "spark",
                Velocity = new Vector3(Mathf.Cos(angle) * 1.8f, 1.2f + i * .17f, Mathf.Sin(angle) * .7f)
            });
        }
    }

    private void Ring(Vector3 at, double time, string mat, string kind)
    {
        var root = new Node3D { Name = "Event_" + kind };
        _geometry.Part(root, "ring", mat, Vector3.Zero, new Vector3(.85f, .035f, .85f),
            kind == "shield" ? new Vector3(0, 0, Mathf.Pi / 2) : Vector3.Zero);
        AddEffect(new Effect { Node = root, From = at, At = time, Life = .65f, Kind = kind });
    }

    private void UpdateEffects(double elapsed)
    {
        for (int i = _effects.Count - 1; i >= 0; i--)
        {
            var fx = _effects[i]; float age = (float)(elapsed - fx.At);
            if (age >= fx.Life) { Retire(fx.Node); _effects.RemoveAt(i); continue; }
            fx.Node.Visible = age >= 0;
            if (age < 0) continue;
            float p = Mathf.Clamp(age / fx.Life, 0, 1);
            switch (fx.Kind)
            {
                case "bullet": case "infusion": case "beam":
                    fx.Node.Position = fx.From.Lerp(fx.To, p);
                    if (fx.Kind == "infusion") fx.Node.Position += Vector3.Up * Mathf.Sin(p * Mathf.Pi) * .45f;
                    if (fx.Kind == "beam")
                    {
                        var direction = fx.To - fx.From;
                        fx.Node.Position = (fx.From + fx.To) * .5f;
                        fx.Node.Quaternion = new Quaternion(Vector3.Up, direction.Normalized());
                        fx.Node.Scale = new Vector3(.025f * (1 - p * .8f), direction.Length(), .025f * (1 - p * .8f));
                    }
                    break;
                case "spark":
                    fx.Node.Position = fx.From + fx.Velocity * age + Vector3.Down * age * age * 2.3f;
                    fx.Node.Scale = Vector3.One * (1 - p * .85f);
                    break;
                default:
                    fx.Node.Scale = Vector3.One * (.85f + p * .75f);
                    fx.Node.Visible = p < .82f;
                    break;
            }
        }
    }

    private void TrackCamera(float dt)
    {
        float desired = (float)_simulation.Distance + .65f;
        if (_snapCamera) { _cameraX = desired; _snapCamera = false; }
        else _cameraX = Mathf.Lerp(_cameraX, desired, 1 - Mathf.Exp(-dt * 5.0f));
        // No camera shake. A small positive-X angle makes human faces and shields readable.
        var focus = new Vector3(_cameraX, 1.36f, -.20f);
        _camera.Position = focus + new Vector3(3.4f, 3.20f, 12.0f);
        _camera.LookAt(focus, Vector3.Up);
    }

    private void RecycleModules()
    {
        int first = (int)Math.Floor((_cameraX - 19) / ModuleLength);
        for (int i = 0; i < ModuleCount; i++)
        {
            int ordinal = first + i;
            // Stable modulo slot: only the module outside the visible frustum is moved.
            int slot = ((ordinal % ModuleCount) + ModuleCount) % ModuleCount;
            var m = _modules[slot];
            if (m.Ordinal == ordinal) continue;
            m.Ordinal = ordinal; m.Root.Position = new Vector3(ordinal * ModuleLength, 0, 0);
            int variant = ((ordinal % 5) + 5) % 5;
            for (int v = 0; v < m.Variants.Count; v++) m.Variants[v].Visible = v == variant;
            m.Plate.Text = "G / " + Math.Abs(ordinal).ToString("00");
        }
    }

    private Module BuildModule(int index)
    {
        var module = new Module { Root = new Node3D { Name = "GalleryModule" + index } };
        _world.AddChild(module.Root);
        var b = new StaticBatch(_geometry);
        b.Box("floor", new Vector3(3, -.16f, -.70f), new Vector3(6, .30f, 6.2f));
        b.Box("soot", new Vector3(3, -.025f, 1.60f), new Vector3(6, .035f, .43f));
        // Two rails and sleepers continue exactly across module boundaries.
        for (int i = 0; i < 8; i++) b.Box("wood", new Vector3(i * .75f + .30f, -.005f, .15f), new Vector3(.15f, .055f, 2.18f));
        for (int s = -1; s <= 1; s += 2)
        {
            b.Box("steel", new Vector3(3, .018f, .15f + s * .86f), new Vector3(6, .055f, .055f));
            b.Box("iron", new Vector3(3, -.005f, .15f + s * .86f), new Vector3(6, .025f, .16f));
        }
        b.Box("wall", new Vector3(3, 2.50f, -3.75f), new Vector3(6, 5.25f, .34f));
        b.Box("wine", new Vector3(3, .70f, -3.54f), new Vector3(6, 1.30f, .09f));
        b.Box("iron", new Vector3(3, 1.41f, -3.47f), new Vector3(6, .10f, .17f));
        // Subtle masonry courses behind structural metal.
        for (int row = 0; row < 4; row++)
        {
            b.Box("tobacco", new Vector3(3, 1.8f + row * .67f, -3.565f), new Vector3(6, .025f, .018f));
            for (int col = 0; col < 4; col++)
                b.Box("tobacco", new Vector3(col * 1.5f + (row % 2) * .55f + .3f, 2.10f + row * .67f, -3.565f), new Vector3(.025f, .60f, .018f));
        }
        // Riveted upright and interrupted arch: the gap is an intentional original motif.
        b.Box("iron", new Vector3(.12f, 2.1f, -3.15f), new Vector3(.25f, 4.2f, .42f));
        b.Box("brass", new Vector3(.12f, 2.1f, -2.92f), new Vector3(.045f, 4.0f, .025f));
        for (int y = 0; y < 7; y++) b.Sphere("brass", new Vector3(.12f, .30f + y * .57f, -2.88f), Vector3.One * .055f);
        for (int j = 0; j < 12; j++)
        {
            if (j == 8 || j == 9) continue;
            float a = Mathf.Pi * j / 12f, c = Mathf.Pi * (j + 1) / 12f;
            b.Rod("iron", new Vector3(3 + Mathf.Cos(a) * 2.82f, 2.02f + Mathf.Sin(a) * 1.90f, -3.15f),
                new Vector3(3 + Mathf.Cos(c) * 2.82f, 2.02f + Mathf.Sin(c) * 1.90f, -3.15f), .17f);
        }
        b.Box("iron", new Vector3(3, 4.68f, -1.95f), new Vector3(6, .22f, .18f));
        b.Rod("iron", new Vector3(.18f, 3.43f, -3.12f), new Vector3(1.45f, 4.63f, -2.0f), .12f);
        // Pipes run in X; collars, downpipes, a wheel and gauge break up repetition.
        for (int p = 0; p < 2; p++)
        {
            float y = 2.64f + p * .34f;
            b.Rod(p == 0 ? "tobacco" : "steel", new Vector3(0, y, -3.25f), new Vector3(6, y, -3.25f), p == 0 ? .22f : .11f);
            for (int c = 0; c < 3; c++) b.Shape("cylinder", "brass", new Vector3(c * 2 + .50f, y, -3.25f), new Vector3(.25f, .08f, .25f), new Vector3(0, 0, Mathf.Pi / 2));
        }
        b.Rod("iron", new Vector3(4.91f, .20f, -3.18f), new Vector3(4.91f, 2.65f, -3.18f), .18f);
        b.Shape("ring", "wine", new Vector3(4.91f, 1.44f, -3.0f), new Vector3(.37f, .09f, .37f), new Vector3(Mathf.Pi / 2, 0, 0));
        b.Rod("brass", new Vector3(4.75f, 1.44f, -2.99f), new Vector3(5.07f, 1.44f, -2.99f), .028f);
        b.Shape("cylinder", "linen", new Vector3(4.91f, 2.09f, -3.02f), new Vector3(.24f, .055f, .24f), new Vector3(Mathf.Pi / 2, 0, 0));
        b.Rod("soot", new Vector3(4.91f, 2.09f, -2.97f), new Vector3(4.97f, 2.16f, -2.97f), .017f);
        // Recessed wall grille with depth, not a flat painted backdrop.
        b.Box("soot", new Vector3(2.8f, 1.95f, -3.48f), new Vector3(1.70f, 2.26f, .20f));
        for (int i = 0; i < 9; i++) b.Box("iron", new Vector3(2.04f + i * .19f, 1.95f, -3.28f), new Vector3(.035f, 2.17f, .055f));
        b.Box("steel", new Vector3(2.80f, 1.03f, -3.23f), new Vector3(1.80f, .075f, .17f));
        b.Box("steel", new Vector3(2.80f, 3.03f, -3.23f), new Vector3(1.80f, .075f, .17f));
        // Foreground service gutter and short supports frame feet without hiding bodies.
        for (int i = 0; i < 13; i++) b.Box("steel", new Vector3(i * .46f, .005f, 1.60f), new Vector3(.055f, .04f, .40f));
        b.Box("iron", new Vector3(3, -.12f, 2.08f), new Vector3(6, .16f, .15f));
        // Caged amber luminaire. Local lights do not cast shadows.
        b.Box("iron", new Vector3(.64f, 3.48f, -3.16f), new Vector3(.49f, .13f, .41f));
        b.Shape("cylinder", "lamp", new Vector3(.64f, 3.25f, -3.08f), new Vector3(.18f, .36f, .18f));
        for (int s = -1; s <= 1; s += 2) b.Rod("iron", new Vector3(.64f + s * .12f, 3.07f, -2.98f), new Vector3(.64f + s * .12f, 3.43f, -2.98f), .025f);
        b.Flush(module.Root);
        module.Root.AddChild(new OmniLight3D
        {
            Position = new Vector3(.64f, 3.19f, -2.65f), LightColor = new Color("#E8B574"),
            LightEnergy = .65f, OmniRange = 4.0f, ShadowEnabled = false
        });
        module.Plate = new Label3D
        {
            Position = new Vector3(4.13f, .80f, -3.35f), Text = "G / 00", FontSize = 32,
            PixelSize = .006f, Modulate = new Color("#BFB49E"), OutlineSize = 0
        };
        module.Root.AddChild(module.Plate);
        for (int v = 0; v < 5; v++) module.Variants.Add(BuildVariant(module.Root, v));
        return module;
    }

    private Node3D BuildVariant(Node3D parent, int variant)
    {
        var root = new Node3D { Name = "RouteVariant" + variant };
        parent.AddChild(root);
        var b = new StaticBatch(_geometry);
        switch (variant)
        {
            case 0: // Entry service lockers and pale inspection marks.
                for (int i = 0; i < 2; i++)
                {
                    b.Box("wine", new Vector3(3.80f + i * .47f, .62f, -2.89f), new Vector3(.42f, 1.22f, .40f));
                    b.Box("brass", new Vector3(3.95f + i * .47f, .72f, -2.66f), new Vector3(.035f, .13f, .04f));
                }
                break;
            case 1: // Rail maintenance: wood crib, spare rails and warning stripes.
                for (int i = 0; i < 3; i++) b.Box("wood", new Vector3(3.7f, .12f + i * .12f, -2.35f), new Vector3(1.5f, .11f, .47f));
                b.Rod("steel", new Vector3(1.3f, .10f, -2.44f), new Vector3(2.1f, 1.53f, -3.20f), .09f);
                for (int i = 0; i < 5; i++) b.Box("ochre", new Vector3(.54f + i * .19f, .04f, 1.16f), new Vector3(.09f, .015f, .24f), new Vector3(0, .5f, 0));
                break;
            case 2: // Pressure manifolds with brass flanges.
                for (int i = 0; i < 3; i++)
                {
                    b.Shape("cylinder", "wine", new Vector3(3.8f + i * .42f, 1.0f, -2.80f), new Vector3(.30f, 1.8f, .30f));
                    b.Shape("cylinder", "brass", new Vector3(3.8f + i * .42f, 1.65f, -2.80f), new Vector3(.35f, .08f, .35f));
                }
                b.Rod("brass", new Vector3(3.8f, 1.9f, -2.8f), new Vector3(4.64f, 1.9f, -2.8f), .12f);
                break;
            case 3: // Nest: broken crates and a handful of mineral intrusions.
                b.Box("wood", new Vector3(4.2f, .20f, -2.65f), new Vector3(1.1f, .35f, .65f), new Vector3(0, .23f, .07f));
                for (int i = 0; i < 5; i++) b.Shape("taper", "jadeDull", new Vector3(3.9f + i * .21f, .32f, -2.39f), new Vector3(.12f, .32f + i % 2 * .12f, .14f), new Vector3(.13f, 0, -.21f));
                b.Box("wood", new Vector3(3.3f, .03f, -1.95f), new Vector3(.65f, .055f, .14f), new Vector3(0, .5f, 0));
                break;
            case 4: // Furnace doors and guarded hot iron; no bloom or luminous fog.
                b.Box("iron", new Vector3(3.75f, 1.14f, -3.15f), new Vector3(1.22f, 1.9f, .50f));
                b.Box("ember", new Vector3(3.75f, 1.12f, -2.88f), new Vector3(.84f, 1.32f, .02f));
                for (int i = 0; i < 5; i++) b.Box("soot", new Vector3(3.35f + i * .20f, 1.12f, -2.84f), new Vector3(.08f, 1.46f, .10f));
                b.Box("brass", new Vector3(3.75f, .47f, -2.83f), new Vector3(1.02f, .12f, .15f));
                break;
        }
        b.Flush(root); return root;
    }

    /// <summary>Static geometry grouped by shared mesh/material into instanced draws.</summary>
    private sealed class StaticBatch
    {
        private readonly ExpeditionGeometry _g;
        private readonly Dictionary<(string Mesh, string Mat), List<Transform3D>> _items = new();
        internal StaticBatch(ExpeditionGeometry geometry) => _g = geometry;
        internal void Shape(string mesh, string material, Vector3 at, Vector3 size, Vector3 rotation = default)
        {
            var key = (mesh, material);
            if (!_items.TryGetValue(key, out var list)) { list = new List<Transform3D>(); _items[key] = list; }
            list.Add(new Transform3D(Basis.FromEuler(rotation).ScaledLocal(size), at));
        }
        internal void Box(string mat, Vector3 at, Vector3 size, Vector3 rotation = default) => Shape("box", mat, at, size, rotation);
        internal void Sphere(string mat, Vector3 at, Vector3 size) => Shape("sphere", mat, at, size);
        internal void Rod(string mat, Vector3 from, Vector3 to, float width)
        {
            var key = ("cylinder", mat);
            if (!_items.TryGetValue(key, out var list)) { list = new List<Transform3D>(); _items[key] = list; }
            var delta = to - from;
            list.Add(new Transform3D(new Basis(new Quaternion(Vector3.Up, delta.Normalized())).ScaledLocal(new Vector3(width, delta.Length(), width)), (from + to) * .5f));
        }
        internal void Flush(Node3D parent)
        {
            foreach (var pair in _items)
            {
                var multimesh = new MultiMesh
                {
                    TransformFormat = MultiMesh.TransformFormatEnum.Transform3D,
                    Mesh = _g.Mesh(pair.Key.Mesh), InstanceCount = pair.Value.Count
                };
                for (int i = 0; i < pair.Value.Count; i++) multimesh.SetInstanceTransform(i, pair.Value[i]);
                parent.AddChild(new MultiMeshInstance3D
                {
                    Multimesh = multimesh, MaterialOverride = _g.Material(pair.Key.Mat),
                    CastShadow = GeometryInstance3D.ShadowCastingSetting.Off
                });
            }
        }
    }
}
