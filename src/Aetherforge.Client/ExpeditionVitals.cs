using Aetherforge.Domain;
using Godot;
using System;
using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Read-only screen-space HUD. Stage refreshes after posing actors and tracking the camera.
/// Native font sizes never inherit camera zoom; drawing uses fixed storage and no child nodes.
/// </summary>
public partial class ExpeditionVitals : Control
{
    private const int ActorLimit = 5, NumberLimit = 10;
    private const double NumberLife = .65;
    private ExpeditionSimulation _simulation = null!;
    private Camera3D _camera = null!;
    private SubViewport _viewport = null!;
    private Control _stage = null!;
    private IReadOnlyList<ExpeditionActor> _party = null!, _enemies = null!;
    private Font _body = null!, _strong = null!;
    private readonly Texture2D[] _roles = new Texture2D[3];
    private readonly ActorPlate[] _actors = new ActorPlate[ActorLimit];
    private readonly CombatNumber[] _numbers = new CombatNumber[NumberLimit];
    private readonly Rect2[] _occupied = new Rect2[ActorLimit * 2 + NumberLimit + 1];
    private int _occupiedCount, _lastEvent, _hovered = -1, _boss = -1;
    private int _nameSize, _dataSize, _numberSize;
    private float _scale;
    private Transform2D _toLocal;
    private Rect2 _safe, _bossRect;
    public bool DetailedHud { get; set; }
    public int SelectedMemberIndex { get; set; }
    public int ActiveFeedbackCount { get; private set; }
    public int VisibleFeedbackCount { get; private set; }
    public int MaxFeedbackPerActor { get; private set; }
    public int VisibleVitalCount { get; private set; }
    public int VisibleAllyCount { get; private set; }
    public int VisibleNameCount { get; private set; }
    public bool VisibleDamage { get; private set; }
    public bool VisibleHealing { get; private set; }
    private readonly int[] _feedbackPerActor = new int[ActorLimit];

    private sealed class ActorPlate
    {
        internal object Data = null!;
        internal string Name = "", ShortName = "", Stats = "", ShownName = "";
        internal float Health = -1, MaxHealth = -1;
        internal bool Projected, Visible, Enemy, Boss, Inspected, Shielded, ShowStats;
        internal Vector2 Anchor, Foot;
        internal Rect2 Head, Hit, Rect, NameRect;
        internal Vector2 NameMeasure, ShortMeasure, StatsMeasure;
    }

    private sealed class CombatNumber
    {
        internal bool Active, Heal, Visible;
        internal int Actor;
        internal object Data = null!;
        internal double At;
        internal string Text = "";
        internal Vector2 Measure;
        internal Rect2 Rect;
    }

    public void Bind(ExpeditionSimulation simulation, Camera3D camera, SubViewport viewport,
        IReadOnlyList<ExpeditionActor> party, IReadOnlyList<ExpeditionActor> enemies)
    {
        _simulation = simulation; _camera = camera; _viewport = viewport; _party = party; _enemies = enemies;
        ResetFeedback();
    }

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        ClipContents = true;
        _stage = (Control)_viewport.GetParent();
        _body = BrassUi.BodyFont; _strong = BrassUi.StrongFont;
        _scale = BrassUi.Scale;
        _nameSize = BrassUi.S(14); _dataSize = BrassUi.S(13); _numberSize = BrassUi.S(18);
        _roles[0] = BrassUi.Icon("shield"); _roles[1] = BrassUi.Icon("rifle"); _roles[2] = BrassUi.Icon("heal");
        for (int i = 0; i < ActorLimit; i++) _actors[i] = new ActorPlate();
        for (int i = 0; i < NumberLimit; i++) _numbers[i] = new CombatNumber();
    }

    /// <summary>Also skips retained history when binding a running simulation or resetting.</summary>
    public void ResetFeedback()
    {
        foreach (var number in _numbers) if (number != null) number.Active = number.Visible = false;
        _lastEvent = _simulation == null || _simulation.Events.Count == 0 ? 0 : _simulation.Events[^1].Id;
    }

    /// <summary>No Advance and no engine-delta clock: all transients are aged by real Elapsed.</summary>
    public void Refresh()
    {
        if (_body == null || _simulation == null || Size.X < 2 || Size.Y < 2) return;
        _safe = new Rect2(Vector2.One * U(6), Size - Vector2.One * U(12));
        _toLocal = GetGlobalTransformWithCanvas().AffineInverse() * _stage.GetGlobalTransformWithCanvas();
        _occupiedCount = 0; _hovered = -1; _boss = -1; _bossRect = default;
        for (int i = 0; i < ActorLimit; i++) SampleActor(i);
        ConsumeEvents();
        PlaceBoss();
        // Every health/role row gets a chance before a long optional name can occupy its space.
        int selected = Math.Clamp(SelectedMemberIndex, 0, 2);
        PlaceVital(selected);
        // Give the higher rear silhouette a row before a lower neighbour takes its only headroom.
        for (int i = ActorLimit - 1; i >= 0; i--) if (i != selected) PlaceVital(i);
        bool missingAlly = false;
        for (int i = 0; i < 3; i++) missingAlly |= _actors[i].Projected && _actors[i].Health > 0 && !_actors[i].Visible;
        if (missingAlly)
        {
            // A short shared rail is the compact fallback: do not drop essential ally HP
            // just because large text/nearby silhouettes consume overhead space.
            _occupiedCount = 0;
            if (_bossRect.HasArea()) Reserve(_bossRect);
            foreach (var actor in _actors) actor.Visible = false;
            PlacePartyRail();
            for (int i = 0; i < ActorLimit; i++) if (!_actors[i].Visible) PlaceVital(i);
        }
        // Inspection/selection have priority over optional names of neighbours.
        if (_hovered >= 0) PlacePlate(_hovered);
        if (selected != _hovered) PlacePlate(selected);
        PlaceNumbers();
        // Optional neighbour names yield their space to real combat feedback.
        for (int i = 0; i < ActorLimit; i++) if (i != selected && i != _hovered) PlacePlate(i);
        ActiveFeedbackCount = VisibleFeedbackCount = MaxFeedbackPerActor = VisibleVitalCount = VisibleAllyCount = VisibleNameCount = 0;
        VisibleDamage = VisibleHealing = false; Array.Clear(_feedbackPerActor);
        foreach (var number in _numbers)
        {
            if (number.Active) { ActiveFeedbackCount++; MaxFeedbackPerActor = Math.Max(MaxFeedbackPerActor, ++_feedbackPerActor[number.Actor]); }
            if (number.Visible) { VisibleFeedbackCount++; VisibleHealing |= number.Heal; VisibleDamage |= !number.Heal; }
        }
        foreach (var actor in _actors) { if (actor.Visible) { VisibleVitalCount++; if (!actor.Enemy) VisibleAllyCount++; } if (actor.ShownName.Length > 0) VisibleNameCount++; }
        QueueRedraw();
    }

    private float U(float value) => value * _scale;
    private Vector2 Measure(string text, Font font, int size) => font.GetStringSize(text, HorizontalAlignment.Left, -1, size);
    private float Line(Font font, int size) => font.GetHeight(size);

    private Vector2 Project(Vector3 point)
    {
        var screen = _camera.UnprojectPosition(point);
        screen *= _stage.Size / new Vector2(Math.Max(2, _viewport.Size.X), Math.Max(2, _viewport.Size.Y));
        return _toLocal * screen;
    }

    private void SampleActor(int index)
    {
        var plate = _actors[index];
        bool wasVisible = plate.Visible;
        var previousNameRect = plate.NameRect;
        plate.Visible = plate.Projected = false;
        plate.NameRect = default; plate.ShownName = ""; plate.ShowStats = false;
        plate.Enemy = index >= 3;
        int enemy = index - 3;
        if (index < 3 ? index >= _party.Count : enemy >= Math.Min(_enemies.Count, _simulation.Enemies.Count)) return;
        var actor = index < 3 ? _party[index] : _enemies[enemy];
        if (!GodotObject.IsInstanceValid(actor)) return;
        object data = index < 3 ? _simulation.Party[index] : _simulation.Enemies[enemy];
        string name = index < 3 ? _simulation.Party[index].Name : _simulation.Enemies[enemy].Name;
        float hp = index < 3 ? _simulation.Party[index].Health : _simulation.Enemies[enemy].Health;
        float max = index < 3 ? _simulation.Party[index].MaxHealth : _simulation.Enemies[enemy].MaxHealth;
        bool shielded = index == 0 && _simulation.ShieldRemaining > 0 && hp > 0;
        if (!ReferenceEquals(plate.Data, data) || plate.Name != name)
        {
            plate.Data = data; plate.Name = name;
            plate.ShortName = actor.Kind switch
            {
                "guard" => "Guarda", "gunner" => "Pistoleiro", "healer" => "Alquimista",
                "foreman" => "Capataz", "scavenger" => "Catador", "sentry" => "Sentinela",
                "watcher" => "Vigia", "drone" => "Drone", _ => "Rato"
            };
            plate.NameMeasure = Measure(name, _strong, _nameSize);
            plate.ShortMeasure = Measure(plate.ShortName, _strong, _nameSize);
            plate.Health = -1; wasVisible = false;
        }
        if (plate.Health != hp || plate.MaxHealth != max || plate.Shielded != shielded)
        {
            plate.Health = hp; plate.MaxHealth = max; plate.Shielded = shielded;
            plate.Stats = HealthText(hp, max) + (shielded ? " · Protegido" : "");
            plate.StatsMeasure = Measure(plate.Stats, _body, _dataSize);
        }
        plate.Boss = actor.Kind == "foreman";
        if (plate.Boss && hp > 0 && _simulation.Phase == ExpeditionPhase.Combat) _boss = index;
        // Hide enemies while marching toward them; the real actors remain untouched.
        if (plate.Enemy && _simulation.Phase == ExpeditionPhase.March) return;
        var point = actor.GlobalPosition + Vector3.Up * (actor.Height + .20f);
        if (_camera.IsPositionBehind(point)) return;
        plate.Anchor = Project(point);
        plate.Foot = Project(actor.GlobalPosition);
        float radius = actor.Kind is "rat" or "drone" ? .85f : .48f;
        float half = Math.Max(U(14), Math.Abs(Project(point + Vector3.Right * radius).X - plate.Anchor.X));
        // Protect the face/rotor as well as the anchor; labels/numbers may never cross this region.
        float lowerHead = Project(actor.GlobalPosition + Vector3.Up * (actor.Height - (actor.Kind == "drone" ? 1.3f : .75f))).Y;
        plate.Head = new Rect2(plate.Anchor + new Vector2(-half, -U(2)), new Vector2(half * 2, Math.Max(U(24), lowerHead - plate.Anchor.Y + U(4))));
        plate.Hit = new Rect2(plate.Anchor + new Vector2(-half - U(4), -U(24)), new Vector2(half * 2 + U(8),
            Math.Max(U(48), Project(actor.GlobalPosition).Y - plate.Anchor.Y + U(24))));
        plate.Projected = _safe.Intersects(plate.Head);
        if (!plate.Projected) return;
        var mouse = GetLocalMousePosition();
        if (hp > 0 && _safe.HasPoint(mouse) && (plate.Hit.HasPoint(mouse) ||
            (wasVisible && (plate.Rect.HasPoint(mouse) || previousNameRect.HasPoint(mouse))))) _hovered = index;
        plate.Inspected = index == Math.Clamp(SelectedMemberIndex, 0, 2);
    }

    private static string HealthText(float hp, float max) =>
        Math.Ceiling(hp).ToString("0", CultureInfo.InvariantCulture) + " / " + Math.Ceiling(max).ToString("0", CultureInfo.InvariantCulture);

    private void ConsumeEvents()
    {
        bool terminal = _simulation.Phase is ExpeditionPhase.Complete or ExpeditionPhase.Defeat or ExpeditionPhase.Retreated;
        foreach (var number in _numbers)
        {
            number.Visible = false;
            if (number.Active && (terminal || _simulation.Elapsed - number.At >= NumberLife ||
                !ReferenceEquals(number.Data, _actors[number.Actor].Data))) number.Active = false;
        }
        foreach (var e in _simulation.Events)
        {
            if (e.Id <= _lastEvent) continue;
            _lastEvent = e.Id;
            if (terminal || e.Kind is not ("hurt" or "heal") || e.Amount <= 0 || !float.IsFinite(e.Amount) ||
                e.Target < 0 || e.Target >= ActorLimit || e.At > _simulation.Elapsed || _simulation.Elapsed - e.At >= NumberLife) continue;
            // Old-encounter enemy feedback must not attach to a newly spawned actor with the same index.
            if (e.Target >= 3 && e.At < _simulation.Elapsed - _simulation.PhaseTime) continue;
            AddNumber(e);
        }
    }

    private void AddNumber(ExpeditionEvent e)
    {
        bool heal = e.Kind == "heal";
        int count = 0;
        CombatNumber? free = null, oldest = null, sameActor = null, sameKind = null;
        foreach (var number in _numbers)
        {
            if (!number.Active) { free ??= number; continue; }
            if (oldest == null || number.At < oldest.At) oldest = number;
            if (number.Actor != e.Target) continue;
            count++;
            if (sameActor == null || number.At < sameActor.At) sameActor = number;
            if (number.Heal == heal && (sameKind == null || number.At < sameKind.At)) sameKind = number;
        }
        // Keep damage and healing separate; saturation replaces a same-kind item first.
        var slot = count >= 2 ? sameKind ?? sameActor! : free ?? oldest!;
        slot.Active = true; slot.Actor = e.Target; slot.Data = _actors[e.Target].Data;
        slot.Heal = heal; slot.At = e.At;
        slot.Text = (heal ? "+" : "−") + Math.Ceiling(e.Amount).ToString("0", CultureInfo.InvariantCulture);
        slot.Measure = Measure(slot.Text, _strong, _numberSize);
    }

    private bool Fits(Rect2 rect)
    {
        if (!_safe.Encloses(rect)) return false;
        for (int i = 0; i < ActorLimit; i++)
            if (_actors[i].Projected && rect.Intersects(_actors[i].Head)) return false;
        for (int i = 0; i < _occupiedCount; i++) if (rect.Intersects(_occupied[i])) return false;
        return true;
    }

    private void Reserve(Rect2 rect) => _occupied[_occupiedCount++] = rect.Grow(U(3));

    private void PlaceBoss()
    {
        if (_boss < 0) return;
        var plate = _actors[_boss];
        float width = Math.Max(U(238), plate.NameMeasure.X + plate.StatsMeasure.X + U(28));
        float height = Line(_strong, _nameSize) + U(21);
        for (int side = 0; side < 3; side++)
        {
            float x = side == 0 ? _safe.End.X - width : side == 1 ? (Size.X - width) / 2 : _safe.Position.X;
            var rect = new Rect2(x, _safe.Position.Y, width, height);
            if (!Fits(rect)) continue;
            _bossRect = rect; Reserve(rect); break;
        }
    }

    private void PlaceVital(int index)
    {
        var plate = _actors[index];
        if (!plate.Projected || plate.Health <= 0 || plate.Boss) return;
        var size = new Vector2(U(plate.Enemy ? 56 : 76), U(18));
        for (int lift = 0; lift < 3; lift++)
        {
            var rect = new Rect2(plate.Anchor.X - size.X / 2, plate.Anchor.Y - U(5) - size.Y - lift * U(22), size.X, size.Y);
            if (!Fits(rect)) continue;
            plate.Rect = rect; plate.Visible = true; Reserve(rect); return;
        }
    }

    private void PlacePartyRail()
    {
        var size = new Vector2(U(76), U(18));
        float width = size.X * 3 + U(12);
        float center = (_actors[0].Anchor.X + _actors[1].Anchor.X + _actors[2].Anchor.X) / 3;
        float x = Math.Clamp(center - width / 2, _safe.Position.X, Math.Max(_safe.Position.X, _safe.End.X - width));
        var rail = new Rect2(x, _safe.End.Y - size.Y, width, size.Y);
        if (!Fits(rail)) rail.Position = new Vector2(_safe.Position.X, rail.Position.Y);
        if (!Fits(rail)) rail.Position = new Vector2(_safe.End.X - width, rail.Position.Y);
        if (!Fits(rail)) return;
        for (int slot = 0; slot < 3; slot++)
        {
            var actor = _actors[2 - slot];
            if (!actor.Projected) continue;
            actor.Rect = new Rect2(rail.Position + Vector2.Right * slot * (size.X + U(6)), size);
            actor.Visible = true; Reserve(actor.Rect);
        }
    }

    private void PlacePlate(int index)
    {
        var plate = _actors[index];
        if (!plate.Visible) return;
        bool inspected = plate.Inspected || index == _hovered;
        bool named = inspected || DetailedHud || plate.Enemy;
        if (!named) return;
        // Drop secondary text before dropping identification; never shrink native glyphs.
        for (int detail = 0; detail < 3; detail++)
        {
            bool stats = (inspected || DetailedHud) && detail == 0;
            string text = detail <= 1 ? (inspected || DetailedHud ? plate.Name : plate.ShortName) : plate.ShortName;
            float nameWidth = detail <= 1 && (inspected || DetailedHud) ? plate.NameMeasure.X : plate.ShortMeasure.X;
            float width = Math.Max(nameWidth, stats ? plate.StatsMeasure.X : 0) + U(16);
            float height = Line(_strong, _nameSize) + U(6) + (stats ? Line(_body, _dataSize) + U(2) : 0);
            // Only vertical lifts: a plate must not drift sideways onto a different actor.
            for (int lift = 0; lift < 3; lift++)
            {
                var rect = new Rect2(plate.Anchor.X - width / 2, plate.Rect.Position.Y - U(4) - height - lift * U(16), width, height);
                if (!Fits(rect)) continue;
                plate.NameRect = rect; plate.ShownName = text; plate.ShowStats = stats;
                Reserve(rect); return;
            }
            // At compact height there may be no room above the heads. Keep identification
            // attached to the same actor below its feet, instead of hiding every selected name.
            var underfoot = new Rect2(plate.Anchor.X - width / 2, Math.Max(plate.Foot.Y + U(5), plate.Rect.End.Y + U(4)), width, height);
            if (Fits(underfoot))
            {
                plate.NameRect = underfoot; plate.ShownName = text; plate.ShowStats = stats;
                Reserve(underfoot); return;
            }
        }
    }

    private void PlaceNumbers()
    {
        foreach (var number in _numbers)
        {
            if (!number.Active) continue;
            var actor = _actors[number.Actor];
            if (!actor.Projected) continue;
            var size = new Vector2(number.Measure.X + U(23), Line(_strong, _numberSize) + U(4));
            float rise = U(14);
            // Reserve the whole .65 s travel channel, so rising amounts never cross a plate/face.
            for (int lane = 0; lane < 8; lane++)
            {
                int side = ((lane % 2 == 0) == number.Heal) ? 1 : -1;
                float x = side > 0 ? actor.Head.End.X + U(6) : actor.Head.Position.X - size.X - U(6);
                float y = actor.Anchor.Y + U(5) + (lane / 2) * (size.Y + U(5));
                var travel = new Rect2(x, y - rise, size.X, size.Y + rise);
                if (!Fits(travel)) continue;
                number.Rect = new Rect2(x, y - rise * (float)((_simulation.Elapsed - number.At) / NumberLife), size.X, size.Y);
                number.Visible = true; Reserve(travel); break;
            }
        }
    }

    public override void _Draw()
    {
        if (_strong == null || _simulation == null) return;
        if (_bossRect.HasArea()) DrawBoss();
        for (int i = 0; i < ActorLimit; i++) if (_actors[i].Visible) DrawPlate(i);
        foreach (var number in _numbers) if (number.Visible) DrawNumber(number);
    }

    private void DrawPlate(int index)
    {
        var plate = _actors[index]; var rect = plate.Rect;
        DrawRect(rect, BrassUi.Soot);
        float y = plate.NameRect.Position.Y + U(3);
        if (plate.ShownName.Length > 0)
        {
            DrawRect(plate.NameRect, BrassUi.Soot);
            DrawString(_strong, new Vector2(plate.NameRect.Position.X + U(8), y + _strong.GetAscent(_nameSize)), plate.ShownName,
                HorizontalAlignment.Left, -1, _nameSize, BrassUi.Linen);
            y += Line(_strong, _nameSize) + U(2);
        }
        if (plate.ShowStats)
        {
            DrawString(_body, new Vector2(plate.NameRect.Position.X + U(8), y + _body.GetAscent(_dataSize)), plate.Stats,
                HorizontalAlignment.Left, -1, _dataSize, BrassUi.Paper);
            y += Line(_body, _dataSize) + U(2);
        }
        float vitalWidth = U(plate.Enemy ? 52 : 72);
        var origin = new Vector2(rect.GetCenter().X - vitalWidth / 2, rect.Position.Y + U(3));
        if (!plate.Enemy)
        {
            DrawTextureRect(_roles[index], new Rect2(origin, Vector2.One * U(14)), false);
            origin.X += U(20);
        }
        DrawMeter(new Rect2(origin + new Vector2(0, U(4)), new Vector2(U(52), U(7))), plate.Health, plate.MaxHealth, plate.Enemy);
        if (plate.Inspected || index == _hovered)
        {
            // A short, open bracket identifies selection without surrounding every plate in gold.
            DrawLine(rect.Position, rect.Position + Vector2.Right * U(12), BrassUi.Linen, U(2));
            DrawLine(rect.Position, rect.Position + Vector2.Down * U(8), BrassUi.Linen, U(2));
        }
        if (plate.Shielded)
            DrawLine(origin + new Vector2(0, U(13)), origin + new Vector2(U(52), U(13)), BrassUi.Linen, U(2));
    }

    private void DrawMeter(Rect2 rect, float hp, float max, bool enemy)
    {
        DrawRect(rect.Grow(U(1)), BrassUi.Paper.Darkened(.45f));
        DrawRect(rect, BrassUi.Tobacco);
        float fill = max > 0 ? Math.Clamp(hp / max, 0, 1) : 0;
        DrawRect(new Rect2(rect.Position, new Vector2(rect.Size.X * fill, rect.Size.Y)), enemy ? BrassUi.Linen : BrassUi.Ether);
        // The filled edge and empty track convey health even without hue discrimination.
        if (fill > 0 && fill < 1)
            DrawLine(rect.Position + Vector2.Right * rect.Size.X * fill, rect.Position + new Vector2(rect.Size.X * fill, rect.Size.Y), BrassUi.Soot, U(1));
    }

    private void DrawBoss()
    {
        var plate = _actors[_boss]; var rect = _bossRect;
        DrawRect(rect, BrassUi.Soot);
        var origin = rect.Position + new Vector2(U(9), U(4));
        DrawString(_strong, origin + Vector2.Down * _strong.GetAscent(_nameSize), plate.Name, HorizontalAlignment.Left, -1, _nameSize, BrassUi.Linen);
        DrawString(_body, new Vector2(rect.End.X - U(9) - plate.StatsMeasure.X, origin.Y + _body.GetAscent(_dataSize)), plate.Stats,
            HorizontalAlignment.Left, -1, _dataSize, BrassUi.Paper);
        DrawMeter(new Rect2(rect.Position + new Vector2(U(9), Line(_strong, _nameSize) + U(9)), new Vector2(rect.Size.X - U(18), U(7))), plate.Health, plate.MaxHealth, true);
    }

    private void DrawNumber(CombatNumber number)
    {
        var rect = number.Rect;
        float progress = (float)((_simulation.Elapsed - number.At) / NumberLife);
        float alpha = 1 - Math.Clamp((progress - .75f) / .25f, 0, 1);
        var color = number.Heal ? BrassUi.Ether.Lightened(.25f) : BrassUi.Linen;
        color.A = alpha;
        var background = BrassUi.Soot; background.A = alpha;
        DrawRect(rect, background);
        var marker = rect.Position + new Vector2(U(8), rect.Size.Y / 2);
        if (number.Heal)
        {
            DrawLine(marker - Vector2.Right * U(3), marker + Vector2.Right * U(3), color, U(2));
            DrawLine(marker - Vector2.Down * U(3), marker + Vector2.Down * U(3), color, U(2));
        }
        else
        {
            DrawLine(marker + new Vector2(-U(3), U(3)), marker + new Vector2(U(3), -U(3)), color, U(2));
        }
        DrawString(_strong, rect.Position + new Vector2(U(17), U(2) + _strong.GetAscent(_numberSize)), number.Text,
            HorizontalAlignment.Left, -1, _numberSize, color);
    }
}
