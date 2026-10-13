using Godot;
using Aetherforge.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

public partial class IdleView : Control
{
    private readonly CombatPreview _preview = new();
    private readonly TextureRect[] _portraits = new TextureRect[3];
    private Label _status = null!, _enemies = null!;
    private Label _route = null!, _footer = null!;
    private readonly Label[] _roles = new Label[3];
    private static readonly Dictionary<(string, int, int, int), AtlasTexture> Frames = new();
    private double _clock, _nextTick = 1;
    private double _attackAt = -2, _healAt = -2, _walkingUntil;
    private float _damage;
    private bool _dock;
    public string ApiUrl { get; set; } = "http://127.0.0.1:5080";
    public bool AutoTransfer { get; set; }
    public string SentItemId { get; private set; } = "";
    public IntegrationSnapshot? InventoryState { get; private set; }
    private IntegrationClient _integration = null!;
    private Label _loot = null!, _lootMessage = null!;
    private Button _transfer = null!;
    private bool _reading, _connected, _sending, _autoStarted;
    private double _nextRead;
    private int _serverTick = -1;

    public override void _Ready()
    {
        Theme = PrototypeUi.Theme();
        _integration = new IntegrationClient(ApiUrl);
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        var header = new HBoxContainer { Position = new Vector2(22, 12) }; AddChild(header);
        header.AddChild(PrototypeUi.Label("AETHERFORGE", 22, PrototypeUi.Gold));
        header.AddChild(PrototypeUi.Label("    EXPEDIÇÃO IDLE", 13, PrototypeUi.Muted));
        var dock = new Button { Text = "Barra / janela", Position = new Vector2(606, 8), Size = new Vector2(139, 38) };
        AddChild(dock); dock.Pressed += ToggleDock;
        var close = new Button { Text = "×", Position = new Vector2(752, 8), Size = new Vector2(40, 38) };
        AddChild(close); close.Pressed += () => GetTree().Quit();
        _route = PrototypeUi.Label("GALERIAS DA CALDEIRA   ·   Entrada de serviço", 14, PrototypeUi.Muted).At(22, 51); AddChild(_route);
        for (var i = 0; i < 3; i++)
        {
            var x = 30 + (2 - i) * 151;
            var role = i == 0 ? "TANK" : i == 1 ? "DPS" : "HEALER";
            _roles[i] = PrototypeUi.Label(role, 12, PrototypeUi.Gold).At(x + 28, 82); AddChild(_roles[i]);
            var portrait = new TextureRect { Position = new Vector2(x, 102), Size = new Vector2(116, 130), ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered, MouseFilter = MouseFilterEnum.Ignore };
            _portraits[i] = portrait; AddChild(portrait);
        }
        _enemies = PrototypeUi.Label("SENTINELA", 12, PrototypeUi.Muted).At(574, 54); AddChild(_enemies);
        _status = PrototypeUi.Label("Equipe em marcha", 14).At(22, 260); AddChild(_status);
        _footer = PrototypeUi.Label("Idle + equipamento compartilhado", 12, PrototypeUi.Muted).At(552, 264); AddChild(_footer);
        _loot = PrototypeUi.Label("Primeira vitória: drop garantido de armadura", 14, PrototypeUi.Gold); AddChild(_loot);
        _lootMessage = PrototypeUi.Label("Abra Testar Integração.cmd para conectar os dois jogos.", 12, PrototypeUi.Muted); AddChild(_lootMessage);
        _transfer = new Button { Text = "Enviar para o MMO", Size = new Vector2(229,38), Disabled = true }; AddChild(_transfer);
        _transfer.Pressed += Transfer;
        GetWindow().Size = new Vector2I(812, 302);
        DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.ResizeDisabled, true);
        ToggleDock();
    }
    public override void _Process(double delta)
    {
        _clock += delta;
        if (_clock >= _nextRead && !_reading) { _nextRead = _clock + .35; ReadInventory(); }
        if (!_connected && _clock >= _nextTick)
        {
            var before = _preview.EnemyHealth;
            _preview.Step(); _nextTick += 1;
            AnimateTick(before);
        }
        var attackAge = _clock - _attackAt;
        for (var i = 0; i < 3; i++)
        {
            var model = _preview.Party[i].Model;
            var healing = i == 2 && _clock - _healAt < .9;
            var attacking = i < 2 && attackAge < .78 && _clock > _walkingUntil;
            var pose = healing || attacking ? 2 : _clock < _walkingUntil ? 1 : 0;
            var age = healing ? _clock - _healAt : attackAge;
            var frame = pose == 2 ? Math.Min(3, (int)(age * 4 / .78)) : (int)(_clock * 6) % 4;
            _portraits[i].Texture = SpritePose(model, 2, pose, Math.Max(0, frame));
            var lunge = i == 0 && attacking ? (float)Math.Sin(attackAge / .78 * Math.PI) * 178 : 0;
            _portraits[i].Position = new Vector2(30 + (2 - i) * 151 + lunge, _dock ? 68 : 102);
        }
        _status.Text = _preview.LastEvent + "   ·   " + _preview.Encounters + " encontros";
        _enemies.Text = "SENTINELA   " + _preview.EnemyHealth + " / " + CombatPreview.EnemyMaxHealth;
        QueueRedraw();
    }
    private void AnimateTick(float before)
    {
        if (before <= 0) _walkingUntil = _clock + .7;
        else { _attackAt = _clock; _damage = Math.Max(0, before - _preview.EnemyHealth); }
        if (_preview.LastEvent == "Infusão Etérea") _healAt = _clock;
    }
    private async void ReadInventory()
    {
        _reading = true;
        try
        {
            var state = await _integration.Read(true);
            if (!IsInsideTree()) return;
            _connected = true; InventoryState = state;
            if (state.Battle.Tick != _serverTick)
            {
                var before = _preview.EnemyHealth;
                _preview.ApplySnapshot(state.Battle); _serverTick = state.Battle.Tick;
                AnimateTick(before);
            }
            var item = state.Items.FirstOrDefault();
            _loot.Text = item == null ? "Primeira vitória: Couraça de Latão · DEF +3" : $"{item.Name} · DEF +{item.DefenseBonus} · {item.Rarity} · #{item.Id[..8]}";
            _transfer.Disabled = _sending || item?.Scope != EquipmentScope.Idle;
            _transfer.Text = item?.Scope == EquipmentScope.Equipped ? "Equipado no MMO" : item?.Scope == EquipmentScope.Shared ? "Enviado ao MMO" : "Enviar para o MMO";
            if (item != null && !_sending)
                _lootMessage.Text = item.Scope == EquipmentScope.Idle ? "DROP! Armadura obtida na vitória. Envie pelo botão ao lado." : item.Scope == EquipmentScope.Shared ? "Na mochila compartilhada. Abra o MMO e clique em Equipar." : "Mesmo equipamento equipado no MMO. Defesa 10 → 13.";
            else if (item == null) _lootMessage.Text = "Expedição conectada. O servidor confirma o combate e o drop.";
            if (AutoTransfer && item?.Scope == EquipmentScope.Idle && !_autoStarted)
            {
                _autoStarted = true; _transfer.EmitSignal(Button.SignalName.Pressed);
            }
        }
        catch (Exception) { if (IsInsideTree()) { _connected = false; _transfer.Disabled = true; _lootMessage.Text = "Serviço indisponível. Abra Testar Integração.cmd; aqui só há animação local."; _nextTick = _clock + 1; } }
        finally { _reading = false; }
    }
    private async void Transfer()
    {
        var item = InventoryState?.Items.FirstOrDefault(i => i.Scope == EquipmentScope.Idle);
        if (_sending || item == null) return;
        _sending = true; _transfer.Disabled = true;
        try
        {
            var receipt = await _integration.Command("transfer", item.Id);
            SentItemId = receipt.ItemId;
            if (IsInsideTree()) _lootMessage.Text = receipt.Message;
            GD.Print("TRANSFER PASS item=" + SentItemId);
        }
        catch (Exception error) { if (IsInsideTree()) _lootMessage.Text = "Não foi possível enviar: " + error.Message; }
        finally { _sending = false; }
    }
    public override void _Draw()
    {
        DrawStyleBox(PrototypeUi.Box(new Color("18272bee"), new Color("746045")), new Rect2(Vector2.Zero, Size));
        var floor = _dock ? 172 : 245;
        DrawLine(new Vector2(22, floor), new Vector2(790, floor), new Color("78684a"), 1);
        DrawLine(new Vector2(22, _dock ? 210 : 302), new Vector2(790, _dock ? 210 : 302), new Color("78684a"), 1);
        for (var i = 0; i < 3; i++)
        {
            var bar = new Rect2(38 + (2-i)*151, _dock ? 162 : 235, 104, 6);
            DrawRect(bar,new Color("283234"));
            DrawRect(new Rect2(bar.Position,new Vector2(bar.Size.X*_preview.Party[i].Health/_preview.Party[i].MaxHealth,6)),new Color(i==2 ? "84bb99" : "bd8c52"));
        }
        var age = (float)(_clock - _attackAt);
        var baseline = _dock ? 152f : 225f;
        var defeated = _preview.EnemyHealth <= 0;
        var flash = age is > .38f and < .62f;
        var robotX = 657 + (flash ? (float)Math.Sin(age * 70) * 5 : 0);
        var bob = defeated ? 12 : (float)Math.Sin(_clock * 5) * 2;
        var body = flash ? new Color("f6bc77") : new Color("98734a");
        DrawCircle(new Vector2(robotX, baseline - 2), 28, new Color("36423e"));
        if (!defeated)
        {
            DrawLine(new Vector2(robotX-12,baseline-9+bob),new Vector2(robotX-19,baseline+1),body,7);
            DrawLine(new Vector2(robotX+12,baseline-9+bob),new Vector2(robotX+19,baseline+1),body,7);
        }
        DrawRect(new Rect2(robotX-20,baseline-44+bob,40,34),body);
        DrawCircle(new Vector2(robotX,baseline-41+bob),20,body);
        DrawRect(new Rect2(robotX-17,baseline-44+bob,34,10),new Color("223336"));
        DrawCircle(new Vector2(robotX-8,baseline-39+bob),3,new Color("f0c576"));
        DrawCircle(new Vector2(robotX+8,baseline-39+bob),3,new Color("f0c576"));
        DrawCircle(new Vector2(robotX,baseline-15+bob),8,new Color("324343"));
        DrawCircle(new Vector2(robotX,baseline-15+bob),4,new Color("f0c576"));
        var enemyBar = new Rect2(robotX-35,baseline+10,70,5);
        DrawRect(enemyBar,new Color("384443"));
        DrawRect(new Rect2(enemyBar.Position,new Vector2(enemyBar.Size.X*_preview.EnemyHealth/CombatPreview.EnemyMaxHealth,5)),new Color("c68755"));
        // Ranged shot, a readable impact, and a brief floating damage number.
        if (age is > .12f and < .44f)
        {
            var t = (age-.12f)/.32f;
            var shot = new Vector2(Mathf.Lerp(260,robotX-20,t),baseline-39-8*Mathf.Sin(t*Mathf.Pi));
            DrawLine(shot-new Vector2(15,0),shot,new Color("dca760"),3);
            DrawCircle(shot,3,new Color("fff1ba"));
        }
        if (age is > .4f and < .9f && _damage>0)
        {
            var center = new Vector2(robotX-22,baseline-31);
            for(var i=0;i<7;i++)
            {
                var angle=i*Mathf.Tau/7;
                var dir=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));
                DrawLine(center+dir*8,center+dir*(13+age*12),PrototypeUi.Gold,2);
            }
            DrawString(ThemeDB.FallbackFont,new Vector2(robotX-16,baseline-65-age*10),"-"+_damage,HorizontalAlignment.Left,-1,17,PrototypeUi.Gold);
        }
        if (!defeated && age is > .72f and < .97f)
        {
            var target = _portraits[0].Position + new Vector2(58,_dock ? 64 : 94);
            var t = (age-.72f)/.25f;
            var shot = new Vector2(robotX-20,baseline-25).Lerp(target,t);
            DrawLine(shot+new Vector2(12,0),shot,new Color("e48b62"),3);
            DrawCircle(shot,4,new Color("efb983"));
        }
        var healAge=(float)(_clock-_healAt);
        if (healAge is >= 0 and < .9f)
        {
            var tank = _portraits[0].Position + new Vector2(58,_dock ? 66 : 95);
            var start = new Vector2(92,baseline-45);
            var t=Mathf.Clamp(healAge/.6f,0,1);
            var orb=start.Lerp(tank,t); orb.Y-=Mathf.Sin(t*Mathf.Pi)*24;
            DrawCircle(orb,5,new Color("a3e4b1"));
            DrawArc(tank,20+healAge*14,0,Mathf.Tau,24,new Color("83d9a2"),2);
            DrawString(ThemeDB.FallbackFont,tank+new Vector2(-12,-25-healAge*15),"+30",HorizontalAlignment.Left,-1,17,new Color("a3e4b1"));
        }
    }
    private void ToggleDock()
    {
        _dock = !_dock;
        _route.Visible = !_dock;
        var targetSize = new Vector2I(812, _dock ? 290 : 382);
        for (var i = 0; i < 3; i++)
        {
            _roles[i].Position = new Vector2(_roles[i].Position.X, _dock ? 49 : 82);
            _portraits[i].Position = new Vector2(_portraits[i].Position.X, _dock ? 68 : 102);
            _portraits[i].Size = new Vector2(116, _dock ? 91 : 130);
        }
        _status.Position = new Vector2(22, _dock ? 182 : 260);
        _footer.Position = new Vector2(552, _dock ? 185 : 264);
        _enemies.Position = new Vector2(567, _dock ? 56 : 95);
        var lootY = _dock ? 222 : 314;
        _loot.Position = new Vector2(22,lootY);
        _lootMessage.Position = new Vector2(22,lootY+36);
        _transfer.Position = new Vector2(563,lootY-4);
        GetViewport().TransparentBg = _dock;
        DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.Borderless, _dock);
        DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.Transparent, _dock);
        DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.AlwaysOnTop, _dock);
        DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.NoFocus, _dock);
        GetWindow().Size = targetSize;
        var w = (float)GetWindow().Size.X; var h = (float)GetWindow().Size.Y;
        DisplayServer.WindowSetMousePassthrough(_dock ? new[] { new Vector2(10,0), new Vector2(w-10,0), new Vector2(w,10), new Vector2(w,h-10), new Vector2(w-10,h), new Vector2(10,h), new Vector2(0,h-10), new Vector2(0,10) } : Array.Empty<Vector2>());
        if (_dock)
        {
            var area = DisplayServer.ScreenGetUsableRect();
            GetWindow().Position = new Vector2I(Math.Max(area.Position.X, area.Position.X + area.Size.X - 830), Math.Max(area.Position.Y, area.Position.Y + area.Size.Y - GetWindow().Size.Y - 8));
        }
        GD.Print($"IDLE layout={(_dock ? "dock" : "window")}, size={GetWindow().Size}, transparency={DisplayServer.IsWindowTransparencyAvailable()}, no_focus={DisplayServer.WindowGetFlag(DisplayServer.WindowFlags.NoFocus)}");
    }
    public async Task VerifyLayouts()
    {
        ToggleDock();
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        if (DisplayServer.WindowGetFlag(DisplayServer.WindowFlags.Borderless) || DisplayServer.WindowGetFlag(DisplayServer.WindowFlags.NoFocus))
            throw new InvalidOperationException("Janela convencional mantém flags da barra.");
        ToggleDock();
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        if (!DisplayServer.IsWindowTransparencyAvailable() || !DisplayServer.WindowGetFlag(DisplayServer.WindowFlags.Transparent) || !DisplayServer.WindowGetFlag(DisplayServer.WindowFlags.NoFocus))
            throw new InvalidOperationException("Modo barra sem transparência ou proteção de foco.");
        GD.Print("LAYOUT PASS: window/dock transition, transparency available, no-focus flag, " + GetWindow().Size);
    }
    public static AtlasTexture SpriteFrame(string model, int direction, bool walk, int frame)
    {
        return SpritePose(model, direction, walk ? 1 : 0, frame);
    }
    private static AtlasTexture SpritePose(string model, int direction, int pose, int frame)
    {
        var key = (model, direction, pose, frame);
        if (!Frames.TryGetValue(key, out var texture))
        {
            var atlas = GD.Load<Texture2D>($"res://Assets/Generated/{model}.png");
            texture = new AtlasTexture { Atlas = atlas, Region = new Rect2((pose * 4 + frame) * 128, direction * 144, 128, 144) };
            Frames.Add(key, texture);
        }
        return texture;
    }
}

public static class ControlPosition
{
    public static T At<T>(this T control, float x, float y) where T : Control { control.Position = new Vector2(x, y); return control; }
}
