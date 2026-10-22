using Godot;
using Aetherforge.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

public partial class Main : Node
{
    private readonly Dictionary<string, string> _options = new();
    private readonly Dictionary<long, CharacterBody3D> _bodies = new();
    private readonly Dictionary<long, Vector2> _inputs = new();
    private readonly Dictionary<long, Avatar> _avatars = new();
    private Node3D _world = null!;
    private Camera3D _camera = null!;
    private Label _status = null!;
    private string _mode = "idle";
    private double _elapsed, _sendClock;
    private float _orbit = Mathf.Pi / 4;
    private int _snapshots, _maxPeers;
    private float _maxDisplacement;
    private bool _finished;
    private bool _invalidSnapshot, _wallReached;
    private readonly Dictionary<long, Vector3> _firstSeen = new();
    private IdleView? _idle;
    private ExpeditionView? _expedition;
    private bool _expeditionLayoutOk, _sawMarch, _sawCombat, _sawLoot, _sawHeal, _sawDamage, _distanceMovedInCombat;
    private double _lastExpeditionDistance;
    private ExpeditionPhase _lastExpeditionPhase = ExpeditionPhase.March;
    private readonly HashSet<string> _encounterKinds = new();
    private double _sceneMinX = double.PositiveInfinity, _sceneMaxX = double.NegativeInfinity;
    private double _cameraMinX = double.PositiveInfinity, _cameraMaxX = double.NegativeInfinity;
    private bool _sawWalkPose, _sawAttackPose;
    private int _maxHudFeedback, _maxHudPerActor, _maxHudVitals, _maxHudNames;
    private bool _sawHudDamage, _sawHudHealing;
    private IntegrationClient _integration = null!;
    private IntegrationSnapshot? _inventory;
    private Label _inventoryText = null!, _itemMessage = null!;
    private Button _equip = null!, _hit = null!;
    private bool _readingInventory, _itemCommand, _autoEquip, _autoHit;
    private double _nextInventory, _hitAt = -2;
    private int _lastHitCount;

    public override void _Ready()
    {
        foreach (var argument in OS.GetCmdlineUserArgs())
            if (argument.StartsWith("--")) { var parts = argument[2..].Split('=', 2); _options[parts[0]] = parts.Length > 1 ? parts[1] : "true"; }
        _mode = Option("mode", "expedition");
        if (_mode == "bake") { AddChild(new SpriteBaker()); return; }
        if (_mode is "expedition" or "ui-proof")
        {
            BrassMotion.Reduced = _options.ContainsKey("reduce-ui-motion");
            if (float.TryParse(Option("ui-scale", "1"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var uiScale) && float.IsFinite(uiScale))
                BrassUi.Scale = Math.Clamp(uiScale, 1, 1.5f);
        }
        if (_mode == "ui-proof")
        {
            Engine.MaxFps = 30;
            GetWindow().Size = new Vector2I(1120, 760);
            AddChild(new BrassTypeProof());
            return;
        }
        if (_mode == "expedition")
        {
            Engine.MaxFps = 30;
            if (_options.TryGetValue("time-scale", out var scale) && double.TryParse(scale, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var speed))
                Engine.TimeScale = Math.Clamp(speed, .1, 16);
            _expedition = new ExpeditionView { Overview = _options.ContainsKey("overview") || DisplayServer.GetName() == "headless" };
            AddChild(_expedition);
            _expedition.DetailedHud = _options.ContainsKey("hud-details");
            if (_options.ContainsKey("ui-check"))
            {
                if (_options.TryGetValue("qa-size", out var qaSize))
                {
                    var dimensions = qaSize.Split('x', 2);
                    if (dimensions.Length == 2 && int.TryParse(dimensions[0], out var width) && int.TryParse(dimensions[1], out var height))
                        GetWindow().Size = new Vector2I(Math.Clamp(width, 1000, 2560), Math.Clamp(height, 320, 1440));
                }
                var uiCheck = new UiV14Check { EvidencePath = Option("ui-evidence", ""), CaptureDirectory = Option("ui-captures", "") };
                uiCheck.Bind(_expedition); AddChild(uiCheck);
            }
            if (_options.ContainsKey("component-check"))
            {
                var componentCheck = new UiV15Check { EvidencePath = Option("ui-evidence", ""), CaptureDirectory = Option("ui-captures", "") };
                componentCheck.Bind(_expedition); AddChild(componentCheck);
            }
            var audio = new ExpeditionAudio(); audio.Bind(_expedition.Simulation); AddChild(audio);
            if (_options.ContainsKey("layout-check") || _options.ContainsKey("expedition-smoke")) VerifyExpedition(_expedition);
            if (_options.ContainsKey("interaction-check"))
            {
                var interactionCheck = new ExpeditionInteractionCheck(); interactionCheck.Bind(_expedition); AddChild(interactionCheck);
            }
            GD.Print($"EXPEDITION READY — Brass v12 / UI v15 / scale {BrassUi.Scale:0.##}; legacy modes unchanged");
            return;
        }
        if (_mode == "idle")
        {
            Engine.MaxFps = 30;
            var idle = new IdleView { ApiUrl = Option("api", "http://127.0.0.1:5080"), AutoTransfer = _options.ContainsKey("integration-smoke") }; _idle = idle; AddChild(idle);
            if (_options.ContainsKey("layout-check")) VerifyIdle(idle);
            return;
        }
        _world = new Node3D(); AddChild(_world);
        WorldBuilder.Build(_world, _mode != "server" && DisplayServer.GetName() != "headless");
        var peer = new ENetMultiplayerPeer();
        var port = int.Parse(Option("port", "7777"));
        if (_mode == "server")
        {
            peer.SetBindIP("127.0.0.1");
            var error = peer.CreateServer(port, 8, 1);
            if (error != Error.Ok) { GD.PrintErr("SERVER failed: " + error); GetTree().Quit(1); return; }
            Multiplayer.MultiplayerPeer = peer;
            Multiplayer.PeerConnected += AddPeer;
            Multiplayer.PeerDisconnected += RemovePeer;
            GD.Print($"SERVER READY localhost:{port} — authoritative movement, M0");
        }
        else
        {
            BuildMapView();
            var error = peer.CreateClient("127.0.0.1", port, 1);
            if (error != Error.Ok) { GD.PrintErr("CLIENT failed: " + error); GetTree().Quit(1); return; }
            Multiplayer.MultiplayerPeer = peer;
            Multiplayer.ConnectedToServer += () => GD.Print("CLIENT CONNECTED " + Multiplayer.GetUniqueId());
            Multiplayer.ConnectionFailed += () => { _status.Text = "Servidor indisponível. Inicie scripts/Run.ps1 -Mode Server."; if (_options.ContainsKey("smoke")) GetTree().Quit(1); };
            Multiplayer.ServerDisconnected += () => { _status.Text = "Servidor desconectado"; if (_options.ContainsKey("smoke")) GetTree().Quit(1); };
        }
    }

    private string Option(string key, string fallback) => _options.TryGetValue(key, out var value) ? value : fallback;

    private void AddPeer(long id)
    {
        var body = new CharacterBody3D { Position = new Vector3((_bodies.Count % 3 - 1) * 2, .05f, 1) };
        body.AddChild(new CollisionShape3D { Shape = new CapsuleShape3D { Radius = .28f, Height = 1.6f }, Position = new Vector3(0, .8f, 0) });
        _world.AddChild(body); _bodies.Add(id, body); _inputs.Add(id, Vector2.Zero);
        GD.Print("SERVER peer joined: " + id);
    }

    private void RemovePeer(long id)
    {
        if (_bodies.Remove(id, out var body)) body.QueueFree();
        _inputs.Remove(id); GD.Print("SERVER peer left: " + id);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.UnreliableOrdered)]
    public void SendInput(Vector2 direction)
    {
        if (_mode != "server") return;
        var id = Multiplayer.GetRemoteSenderId();
        if (!_inputs.ContainsKey(id)) return;
        var validated = MovementRules.Validate(direction.X, direction.Y);
        _inputs[id] = new Vector2(validated.X, validated.Y);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_mode is "idle" or "bake" or "expedition" or "ui-proof") return;
        _sendClock += delta;
        if (_mode == "server")
        {
            foreach (var (id, body) in _bodies)
            {
                var input = _inputs[id];
                body.Velocity = new Vector3(input.X * MovementRules.Speed, -1, input.Y * MovementRules.Speed);
                body.MoveAndSlide();
            }
            if (_sendClock >= .1 && _bodies.Count > 0)
            {
                _sendClock = 0;
                var state = _bodies.Select(pair => new PlayerState { Id = pair.Key, X = pair.Value.Position.X, Z = pair.Value.Position.Z, Vx = pair.Value.Velocity.X, Vz = pair.Value.Velocity.Z }).ToArray();
                Rpc(MethodName.ReceiveSnapshot, JsonSerializer.Serialize(state));
            }
        }
        else if (Multiplayer.MultiplayerPeer.GetConnectionStatus() == MultiplayerPeer.ConnectionStatus.Connected && _sendClock >= .05)
        {
            _sendClock = 0;
            var direction = Vector2.Zero;
            if (_options.ContainsKey("bot"))
            {
                if (Option("bot", "0") == "wall") direction = Vector2.Right;
                else
                {
                    var phase = float.Parse(Option("bot", "0"), System.Globalization.CultureInfo.InvariantCulture);
                    direction = new Vector2((float)Math.Cos(_elapsed + phase), (float)Math.Sin(_elapsed + phase));
                }
            }
            else
            {
                direction.X = (Input.IsPhysicalKeyPressed(Key.D) ? 1 : 0) - (Input.IsPhysicalKeyPressed(Key.A) ? 1 : 0);
                direction.Y = (Input.IsPhysicalKeyPressed(Key.S) ? 1 : 0) - (Input.IsPhysicalKeyPressed(Key.W) ? 1 : 0);
                direction = direction.Rotated(-_orbit);
            }
            RpcId(1, MethodName.SendInput, direction);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.UnreliableOrdered)]
    public void ReceiveSnapshot(string json)
    {
        if (_mode == "server") return;
        var state = JsonSerializer.Deserialize<PlayerState[]>(json)!;
        _snapshots++; _maxPeers = Math.Max(_maxPeers, state.Length);
        var existing = state.Select(p => p.Id).ToHashSet();
        foreach (var id in _avatars.Keys.Where(id => !existing.Contains(id)).ToArray()) { _avatars[id].Node.QueueFree(); _avatars.Remove(id); }
        foreach (var player in state)
        {
            _invalidSnapshot |= !float.IsFinite(player.X) || !float.IsFinite(player.Z) || Math.Abs(player.X) > 12.6 || Math.Abs(player.Z) > 10.6;
            _wallReached |= Math.Abs(player.X) > 12 && Math.Abs(player.Vx) < .1;
            var target = new Vector3(player.X, 0, player.Z);
            if (!_avatars.TryGetValue(player.Id, out var avatar))
            {
                avatar = BuildAvatar(player.Id, target); _avatars.Add(player.Id, avatar); _firstSeen[player.Id] = target;
            }
            avatar.Target = target; avatar.Velocity = new Vector2(player.Vx, player.Vz);
            _maxDisplacement = Math.Max(_maxDisplacement, target.DistanceTo(_firstSeen[player.Id]));
        }
        _status.Text = $"ONLINE  ·  {state.Length} jogadores  ·  servidor autoritativo";
    }

    public override void _Process(double delta)
    {
        _elapsed += delta;
        if (_expedition != null)
        {
            var simulation = _expedition.Simulation;
            var hud = _expedition.Stage.Hud;
            _maxHudFeedback = Math.Max(_maxHudFeedback, hud.ActiveFeedbackCount);
            _maxHudPerActor = Math.Max(_maxHudPerActor, hud.MaxFeedbackPerActor);
            _maxHudVitals = Math.Max(_maxHudVitals, hud.VisibleVitalCount);
            _maxHudNames = Math.Max(_maxHudNames, hud.VisibleNameCount);
            _sawHudDamage |= hud.VisibleDamage; _sawHudHealing |= hud.VisibleHealing;
            _sawMarch |= simulation.Phase == ExpeditionPhase.March && simulation.Distance > .1;
            _sawCombat |= simulation.Phase == ExpeditionPhase.Combat;
            _sawLoot |= simulation.Phase == ExpeditionPhase.Loot;
            _sawHeal |= simulation.Events.Any(e => e.Kind == "heal");
            _sawDamage |= simulation.Party.Any(p => p.Health < p.MaxHealth);
            if (_lastExpeditionPhase == ExpeditionPhase.Combat && simulation.Phase == ExpeditionPhase.Combat && Math.Abs(simulation.Distance - _lastExpeditionDistance) > .001)
                _distanceMovedInCombat = true;
            _lastExpeditionDistance = simulation.Distance; _lastExpeditionPhase = simulation.Phase;
            foreach (var enemy in simulation.Enemies) _encounterKinds.Add(enemy.Kind);
            var world = _expedition.Stage.GetNodeOrNull<Node3D>("ExpeditionViewport/ServiceGallery");
            if (world != null)
            {
                var guard = world.GetNodeOrNull<ExpeditionActor>("Party_guard");
                var camera = world.GetNodeOrNull<Camera3D>("TrackingOrthographicCamera");
                if (guard != null)
                {
                    _sceneMinX = Math.Min(_sceneMinX, guard.Position.X); _sceneMaxX = Math.Max(_sceneMaxX, guard.Position.X);
                    _sawWalkPose |= guard.CurrentPose == "walk"; _sawAttackPose |= guard.CurrentPose == "attack";
                }
                if (camera != null) { _cameraMinX = Math.Min(_cameraMinX, camera.Position.X); _cameraMaxX = Math.Max(_cameraMaxX, camera.Position.X); }
            }
        }
        if (_mode == "map")
        {
            if (_elapsed >= _nextInventory && !_readingInventory) { _nextInventory = _elapsed + .4; ReadInventory(); }
            if (Input.IsPhysicalKeyPressed(Key.Q)) _orbit += (float)delta;
            if (Input.IsPhysicalKeyPressed(Key.E)) _orbit -= (float)delta;
            if (_options.ContainsKey("rotate")) _orbit += (float)delta * .35f;
            UpdateCamera();
            foreach (var avatar in _avatars.Values)
            {
                avatar.Node.Position = avatar.Node.Position.Lerp(avatar.Target, Mathf.Min(1, (float)delta * 15));
                var walking = avatar.Velocity.LengthSquared() > .05f;
                if (walking) avatar.Heading = Mathf.Atan2(avatar.Velocity.X, avatar.Velocity.Y);
                var direction = Mathf.PosMod(Mathf.RoundToInt((avatar.Heading - _orbit) / (Mathf.Pi / 4)), 8);
                avatar.Sprite.Texture = IdleView.SpriteFrame(avatar.Model, direction, walking, (int)(_elapsed * 6) % 4);
                if (avatar.IsLocal)
                {
                    avatar.Sprite.Modulate = _elapsed - _hitAt < .25 ? new Color("f08b77") : _inventory?.Character.EquippedItemId != null ? new Color("ffce84") : Colors.White;
                    avatar.Label.Text = _inventory?.Character.EquippedItemId != null ? "VOCÊ · DEF " + _inventory.Character.Defense : "VOCÊ";
                }
            }
        }
        if (_options.TryGetValue("duration", out var seconds) && _elapsed > double.Parse(seconds, System.Globalization.CultureInfo.InvariantCulture) && !_finished)
        {
            _finished = true;
            Finish();
        }
        if (Input.IsPhysicalKeyPressed(Key.Escape) && _mode is not ("bake" or "expedition")) GetTree().Quit();
    }

    public override void _UnhandledKeyInput(InputEvent input)
    {
        if (_mode == "expedition" && input is InputEventKey { Pressed: true, Echo: false, Keycode: Key.Escape })
            GetTree().Quit();
    }

    private async void Finish()
    {
        if (_options.TryGetValue("screenshot", out var file) && DisplayServer.GetName() != "headless")
        {
            await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
            var result = GetViewport().GetTexture().GetImage().SavePng(file);
            GD.Print("SCREENSHOT " + result + " " + file);
        }
        if (_mode == "expedition" && _expedition != null)
        {
            var simulation = _expedition.Simulation;
            var passed = _expeditionLayoutOk && _sawMarch && _sawCombat && _sawLoot && _sawHeal && _sawDamage && !_distanceMovedInCombat
                && simulation.Phase == ExpeditionPhase.Complete && simulation.Victories == 5 && _encounterKinds.Count == 6
                && _sceneMaxX - _sceneMinX > 100 && _cameraMaxX - _cameraMinX > 100 && _sawWalkPose && _sawAttackPose;
            var evidence = new { phase = simulation.Phase.ToString(), simulation.Victories, simulation.Distance, simulation.Elapsed,
                kinds = _encounterKinds.OrderBy(k => k).ToArray(), layout = _expeditionLayoutOk, march = _sawMarch, combat = _sawCombat, loot = _sawLoot,
                healing = _sawHeal, damage = _sawDamage, distanceMovedInCombat = _distanceMovedInCombat,
                sceneTravel = double.IsFinite(_sceneMaxX - _sceneMinX) ? _sceneMaxX - _sceneMinX : 0,
                cameraTravel = double.IsFinite(_cameraMaxX - _cameraMinX) ? _cameraMaxX - _cameraMinX : 0,
                walkPose = _sawWalkPose, attackPose = _sawAttackPose,
                ui = new { scale = BrassUi.Scale, maxActiveFeedback = _maxHudFeedback, maxFeedbackPerActor = _maxHudPerActor,
                    maxVisibleVitals = _maxHudVitals, maxVisibleNames = _maxHudNames, damageNumber = _sawHudDamage, healNumber = _sawHudHealing,
                    boundedFeedback = _maxHudFeedback <= 10 && _maxHudPerActor <= 2 },
                party = simulation.Party.Select(p => new { p.Name, p.Health, p.MaxHealth }).ToArray(), passed };
            if (_options.TryGetValue("evidence", out var evidenceFile))
            {
                var directory = System.IO.Path.GetDirectoryName(evidenceFile);
                if (!string.IsNullOrEmpty(directory)) System.IO.Directory.CreateDirectory(directory);
                System.IO.File.WriteAllText(evidenceFile, JsonSerializer.Serialize(evidence, new JsonSerializerOptions { WriteIndented = true }));
            }
            GD.Print("EXPEDITION " + (passed ? "PASS" : "REVIEW") + " " + JsonSerializer.Serialize(evidence));
            GetTree().Quit(_options.ContainsKey("expedition-smoke") && !passed ? 1 : 0);
        }
        else if (_options.ContainsKey("integration-smoke"))
        {
            var item = _mode == "idle" ? _idle?.InventoryState?.Items.SingleOrDefault() : _inventory?.Items.SingleOrDefault();
            var passed = _mode == "idle" ? _idle?.SentItemId == item?.Id && item != null && item.Scope != EquipmentScope.Idle
                : item?.Scope == EquipmentScope.Equipped && _inventory?.Character.EquippedItemId == item.Id && _inventory.Character.Defense == 13 && _inventory.Character.LastDamage == 17 && _snapshots >= 10;
            GD.Print($"INTEGRATION {_mode} {(passed ? "PASS" : "FAIL")} item={item?.Id} defense={_inventory?.Character.Defense} damage={_inventory?.Character.LastDamage}");
            GetTree().Quit(passed ? 0 : 1);
        }
        else if (_mode == "map" && _options.ContainsKey("smoke"))
        {
            var passed = _maxPeers >= 2 && _snapshots >= 10 && _maxDisplacement > 1 && !_invalidSnapshot && (!_options.ContainsKey("expect-wall") || _wallReached);
            GD.Print($"NETWORK {(passed ? "PASS" : "FAIL")}: peers={_maxPeers}, snapshots={_snapshots}, displacement={_maxDisplacement:F2}, bounds_valid={!_invalidSnapshot}, wall_collision={_wallReached}");
            GetTree().Quit(passed ? 0 : 1);
        }
        else GetTree().Quit();
    }

    private async void VerifyExpedition(ExpeditionView view)
    {
        try { await view.VerifyLayouts(); _expeditionLayoutOk = true; GD.Print("EXPEDITION LAYOUT PASS"); }
        catch (Exception error) { GD.PrintErr(error); GetTree().Quit(1); }
    }

    private async void VerifyIdle(IdleView idle)
    {
        try { await idle.VerifyLayouts(); }
        catch (Exception error) { GD.PrintErr(error); GetTree().Quit(1); }
    }

    private async void ReadInventory()
    {
        _readingInventory = true;
        try
        {
            var state = await _integration.Read();
            if (!IsInsideTree()) return;
            _inventory = state;
            if (state.Character.HitCount != _lastHitCount) { _lastHitCount = state.Character.HitCount; _hitAt = _elapsed; }
            var item = state.Items.FirstOrDefault(i => i.Scope != EquipmentScope.Idle);
            var scope = item?.Scope == EquipmentScope.Equipped ? "equipada" : "na mochila";
            _inventoryText.Text = $"{item?.Name ?? "Nenhum equipamento recebido"}\n{(item == null ? "Envie pelo idle após a vitória." : "#" + item.Id[..8] + " · " + scope)}\nDefesa: {state.Character.BaseDefense} → {state.Character.Defense}\nVida: {state.Character.Health} / {state.Character.MaxHealth}\nÚltimo dano do canhão: {state.Character.LastDamage}";
            _equip.Text = item?.Scope == EquipmentScope.Equipped ? "Desequipar armadura" : "Equipar armadura · DEF +3";
            _equip.Disabled = _itemCommand || item == null;
            _hit.Disabled = _itemCommand;
            if (_options.ContainsKey("integration-smoke") && item?.Scope == EquipmentScope.Shared && !_autoEquip)
            {
                _autoEquip = true; _equip.EmitSignal(Button.SignalName.Pressed);
            }
            if (_options.ContainsKey("integration-smoke") && item?.Scope == EquipmentScope.Equipped && !_autoHit && !_itemCommand)
            {
                _autoHit = true; _hit.EmitSignal(Button.SignalName.Pressed);
            }
        }
        catch (Exception) { if (IsInsideTree()) { _equip.Disabled = true; _hit.Disabled = true; _itemMessage.Text = "Abra Testar Integração.cmd."; } }
        finally { _readingInventory = false; }
    }

    private void ToggleEquipment()
    {
        var item = _inventory?.Items.FirstOrDefault(i => i.Scope != EquipmentScope.Idle);
        if (item != null) ExecuteItemCommand(item.Scope == EquipmentScope.Equipped ? "unequip" : "equip", item.Id);
    }
    private void TestHit() => ExecuteItemCommand("hit", "");
    private async void ExecuteItemCommand(string action, string itemId)
    {
        if (_itemCommand) return;
        _itemCommand = true; _equip.Disabled = true; _hit.Disabled = true;
        try
        {
            var receipt = await _integration.Command(action,itemId);
            if (IsInsideTree()) _itemMessage.Text = receipt.Message;
            GD.Print($"ITEM {action} PASS item={receipt.ItemId} {receipt.Message}");
        }
        catch (Exception error) { if (IsInsideTree()) _itemMessage.Text = "Pedido recusado: " + error.Message; }
        finally { _itemCommand = false; }
    }

    private void BuildMapView()
    {
        Engine.MaxFps = 60;
        _camera = new Camera3D { Projection = Camera3D.ProjectionType.Orthogonal, Size = 19, Current = true, Far = 100 };
        _world.AddChild(_camera); UpdateCamera();
        var layer = new CanvasLayer(); AddChild(layer);
        var ui = new Control { Theme = PrototypeUi.Theme() }; layer.AddChild(ui); ui.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        var panel = new Panel { Position = new Vector2(20, 18), Size = new Vector2(1080, 83), MouseFilter = Control.MouseFilterEnum.Ignore };
        panel.AddThemeStyleboxOverride("panel", PrototypeUi.Box(new Color("14272aef"), new Color("7c684a"))); ui.AddChild(panel);
        panel.AddChild(PrototypeUi.Label("AETHERFORGE", 25, PrototypeUi.Gold).At(20, 10));
        panel.AddChild(PrototypeUi.Label("BRUMAFÉRREA   /   PÁTIO DAS CALDEIRAS", 13, PrototypeUi.Muted).At(20, 47));
        _status = PrototypeUi.Label("Conectando ao servidor local…", 14).At(576, 32); panel.AddChild(_status);
        var footer = new Panel { Position = new Vector2(20, 594), Size = new Vector2(1080, 66), MouseFilter = Control.MouseFilterEnum.Ignore };
        footer.AddThemeStyleboxOverride("panel", PrototypeUi.Box(new Color("14272aef"), new Color("7c684a"))); ui.AddChild(footer);
        footer.AddChild(PrototypeUi.Label("W A S D    Mover       Q / E    Girar câmera       ESC    Sair", 15).At(20, 12));
        footer.AddChild(PrototypeUi.Label("M0 · cenário de teste · personagens em sprites renderizados de modelos 3D", 12, PrototypeUi.Muted).At(20, 37));
        _integration = new IntegrationClient(Option("api", "http://127.0.0.1:5080"));
        var inventory = new Panel { Position = new Vector2(796, 118), Size = new Vector2(304, 285) };
        inventory.AddThemeStyleboxOverride("panel", PrototypeUi.Box(new Color("14272aef"),new Color("7c684a"))); ui.AddChild(inventory);
        inventory.AddChild(PrototypeUi.Label("MOCHILA / ARMADURA",16,PrototypeUi.Gold).At(16,12));
        _inventoryText = PrototypeUi.Label("Conectando inventário…",14).At(16,48); inventory.AddChild(_inventoryText);
        _equip = new Button { Text = "Equipar armadura", Position = new Vector2(16,160), Size = new Vector2(272,38), Disabled = true }; inventory.AddChild(_equip); _equip.Pressed += ToggleEquipment;
        _hit = new Button { Text = "Canhão de teste · dano bruto 30", Position = new Vector2(16,204), Size = new Vector2(272,38), Disabled = true }; inventory.AddChild(_hit); _hit.Pressed += TestHit;
        _itemMessage = PrototypeUi.Label("Envie o drop pelo cliente idle.",12,PrototypeUi.Muted).At(16,251); inventory.AddChild(_itemMessage);
    }

    private void UpdateCamera()
    {
        _camera.Position = new Vector3(Mathf.Sin(_orbit) * 18, 17, Mathf.Cos(_orbit) * 18);
        _camera.LookAt(new Vector3(0, 0, 0));
    }

    private Avatar BuildAvatar(long id, Vector3 position)
    {
        var node = new Node3D { Position = position }; _world.AddChild(node);
        var model = _avatars.Count % 2 == 0 ? "Knight" : "Rogue";
        var sprite = new Sprite3D { Texture = IdleView.SpriteFrame(model, 0, false, 0), Billboard = BaseMaterial3D.BillboardModeEnum.Enabled, PixelSize = .019f, Offset = new Vector2(0, 58), AlphaCut = SpriteBase3D.AlphaCutMode.Discard, Shaded = false };
        node.AddChild(sprite);
        var label = new Label3D { Text = id == Multiplayer.GetUniqueId() ? "VOCÊ" : "EXPLORADOR", Position = new Vector3(0, 2.5f, 0), Billboard = BaseMaterial3D.BillboardModeEnum.Enabled, FontSize = 26, Modulate = id == Multiplayer.GetUniqueId() ? new Color("e6c07e") : new Color("a6d9c7") };
        node.AddChild(label);
        var ring = new MeshInstance3D { Mesh = new CylinderMesh { TopRadius = .55f, BottomRadius = .55f, Height = .02f }, Position = new Vector3(0, .03f, 0), MaterialOverride = WorldBuilder.Material(id == Multiplayer.GetUniqueId() ? "ab814a" : "629485") };
        node.AddChild(ring);
        return new Avatar { Node = node, Sprite = sprite, Label = label, Model = model, Target = position, IsLocal = id == Multiplayer.GetUniqueId() };
    }

    private sealed class Avatar
    {
        public Node3D Node = null!; public Sprite3D Sprite = null!; public Label3D Label = null!; public string Model = ""; public bool IsLocal;
        public Vector3 Target; public Vector2 Velocity; public float Heading;
    }
    public sealed class PlayerState
    {
        public long Id { get; set; }
        public float X { get; set; } public float Z { get; set; }
        public float Vx { get; set; } public float Vz { get; set; }
    }
}
