using Godot;
using System;

/// <summary>Original engraved plate, with a shadow seat, chamfered enamel and sparse fixings.</summary>
public partial class BrassInstrumentPlate : PanelContainer
{
    public override void _Ready()
    {
        AddThemeStyleboxOverride("panel", new StyleBoxEmpty { ContentMarginLeft = BrassUi.S(14), ContentMarginRight = BrassUi.S(18), ContentMarginTop = BrassUi.S(7), ContentMarginBottom = BrassUi.S(7) });
        Resized += QueueRedraw;
    }
    public override void _Draw()
    {
        float w = Size.X, h = Size.Y, s = BrassUi.Scale, c = 7*s;
        DrawRect(new Rect2(0, 2*s, w, h-2*s), BrassUi.Soot);
        var edge = new[] { new Vector2(c, 0), new Vector2(w-c, 0), new Vector2(w, c), new Vector2(w, h-c), new Vector2(w-c, h-2*s), new Vector2(c, h-2*s), new Vector2(0, h-c), new Vector2(0, c), new Vector2(c, 0) };
        DrawColoredPolygon(edge, BrassUi.Paper);
        DrawPolyline(edge, BrassUi.Brass.Darkened(.18f), s, true);
        DrawRect(new Rect2(7*s, 4*s, Math.Max(0, w-14*s), Math.Max(0, h-11*s)), BrassUi.Linen);
        DrawLine(new Vector2(8*s, 4*s), new Vector2(w-8*s, 4*s), BrassUi.Linen.Lightened(.22f), s);
        // The right contact is ornamental, not a new counter or progression gauge.
        for (int i = 0; i < 3; i++) DrawLine(new Vector2(w-(12+i*4)*s, h-8*s), new Vector2(w-(12+i*4)*s, h-14*s), BrassUi.Tobacco, s);
    }
}

/// <summary>Three same-size socket housings; values remain native Label children.</summary>
public partial class BrassStatSocket : PanelContainer
{
    public int Kind { get; set; }
    public override void _Ready()
    {
        AddThemeStyleboxOverride("panel", new StyleBoxEmpty { ContentMarginLeft = BrassUi.S(5), ContentMarginRight = BrassUi.S(5), ContentMarginTop = BrassUi.S(9), ContentMarginBottom = BrassUi.S(9) });
        Resized += QueueRedraw;
    }
    public override void _Draw()
    {
        float w = Size.X, h = Size.Y, s = BrassUi.Scale;
        DrawRect(new Rect2(0, 0, w, h), BrassUi.Soot);
        var p = new[] { new Vector2(6*s, 3*s), new Vector2(w-6*s, 3*s), new Vector2(w-2*s, 7*s), new Vector2(w-2*s, h-6*s), new Vector2(w-6*s, h-2*s), new Vector2(6*s, h-2*s), new Vector2(2*s, h-6*s), new Vector2(2*s, 7*s), new Vector2(6*s, 3*s) };
        DrawColoredPolygon(p, BrassUi.Paper);
        DrawPolyline(p, BrassUi.Brass.Darkened(.3f), s, true);
        DrawRect(new Rect2(6*s, 7*s, Math.Max(0,w-12*s), Math.Max(0,h-15*s)), BrassUi.Linen);
        DrawLine(new Vector2(7*s, h-9*s), new Vector2(w-7*s, h-9*s), BrassUi.Paper.Darkened(.2f), s);
        // Sparse side notches define the socket; they do not encode attack/defense levels.
        foreach (float x in new[] { 2*s, w-3*s }) DrawLine(new Vector2(x,h*.48f), new Vector2(x,h*.64f), BrassUi.Brass, s);
    }
}

/// <summary>Current HP is immediate; a short lost-health trail is decorative and bounded.</summary>
public partial class BrassVitalMeter : Control
{
    private float _ratio = 1, _trail = 1, _age = 1;
    public bool Paused { get; set; }
    public float HealthRatio => _ratio;
    public float TrailRatio => _trail;
    public bool HasTrail => _trail - _ratio > .001f;
    public int DamageChanges { get; private set; }
    public int HealChanges { get; private set; }
    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        CustomMinimumSize = new Vector2(0, BrassUi.S(9));
        Resized += QueueRedraw; BrassMotion.Changed += SnapTrail;
        SetProcess(false);
    }
    public void SetHealth(double value, double maximum, bool snap = false)
    {
        float next = maximum > 0 ? Mathf.Clamp((float)(value/maximum), 0, 1) : 0;
        bool changed = Math.Abs(next-_ratio) >= .0001f;
        if (!snap && changed) { if (next < _ratio) DamageChanges++; else HealChanges++; }
        if (snap || BrassMotion.Reduced || !IsVisibleInTree()) { _ratio = next; SnapTrail(); return; }
        if (!changed) return;
        if (next < _ratio) _trail = Math.Max(_trail, _ratio);
        else _trail = next;
        _ratio = next; _age = 0;
        SetProcess(HasTrail); QueueRedraw();
    }
    private void SnapTrail() { _trail = _ratio; _age = 1; SetProcess(false); QueueRedraw(); }
    public override void _Process(double delta)
    {
        if (Paused) return;
        _age += (float)(delta / Math.Max(Engine.TimeScale, .001)); // fixed real-time accent; gameplay still owns HP
        if (_age > .08f) _trail = Mathf.MoveToward(_trail, _ratio, (float)delta/(float)Math.Max(Engine.TimeScale,.001)*3.4f);
        if (_age >= .36f || !HasTrail) SnapTrail(); else QueueRedraw();
    }
    public override void _Draw()
    {
        float s = BrassUi.Scale, w = Math.Max(0, Size.X-2*s), h = Math.Max(1, Size.Y-2*s);
        DrawRect(new Rect2(0,0,Size.X,Size.Y), BrassUi.Soot);
        if (HasTrail) DrawRect(new Rect2(s,s,w*_trail,h), BrassUi.Wine.Lightened(.12f));
        DrawRect(new Rect2(s,s,w*_ratio,h), _ratio <= .25f ? BrassUi.Paper : BrassUi.Ether);
        for (int i=1; i<4; i++) DrawLine(new Vector2(s+w*i/4,s),new Vector2(s+w*i/4,s+h),BrassUi.Soot with { A = .4f },s);
        DrawLine(new Vector2(s,s),new Vector2(s+w,s),BrassUi.Paper.Darkened(.35f),s);
    }
    public override void _ExitTree() => BrassMotion.Changed -= SnapTrail;
}

/// <summary>An existing member selector presented as an occupational instrument slip.</summary>
public partial class BrassPartyRow : Button
{
    private Label _name = null!, _value = null!;
    public BrassVitalMeter Meter { get; private set; } = null!;
    public int MemberIndex { get; set; }
    public override void _Ready()
    {
        ToggleMode = true;
        CustomMinimumSize = new Vector2(0,BrassUi.S(45));
        SizeFlagsHorizontal = SizeFlags.ExpandFill;
        BrassUi.Quiet(this);
        foreach (var state in new[] {"normal","hover","pressed","hover_pressed"}) AddThemeStyleboxOverride(state, new StyleBoxEmpty());
        var margin = BrassUi.Margin(9,7); AddChild(margin); BrassUi.FullRect(margin); margin.MouseFilter = MouseFilterEnum.Ignore;
        var row = new HBoxContainer { MouseFilter = MouseFilterEnum.Ignore }; margin.AddChild(row);
        var badge = new Control { CustomMinimumSize = new Vector2(BrassUi.S(30),0), MouseFilter = MouseFilterEnum.Ignore }; row.AddChild(badge);
        var icon = new TextureRect { Texture = BrassUi.Icon(MemberIndex==0?"shield":MemberIndex==1?"rifle":"heal"), CustomMinimumSize=new Vector2(BrassUi.S(20),BrassUi.S(20)), ExpandMode=TextureRect.ExpandModeEnum.IgnoreSize, StretchMode=TextureRect.StretchModeEnum.KeepAspectCentered, MouseFilter=MouseFilterEnum.Ignore };
        badge.AddChild(icon); BrassUi.FullRect(icon); icon.OffsetLeft=BrassUi.S(4); icon.OffsetRight=-BrassUi.S(4);
        var data = new VBoxContainer { SizeFlagsHorizontal=SizeFlags.ExpandFill, MouseFilter=MouseFilterEnum.Ignore }; data.AddThemeConstantOverride("separation",BrassUi.S(3)); row.AddChild(data);
        var top = new HBoxContainer { MouseFilter=MouseFilterEnum.Ignore }; top.AddThemeConstantOverride("separation",BrassUi.S(4)); data.AddChild(top);
        _name=BrassUi.Emphasis("",14); _name.SizeFlagsHorizontal=SizeFlags.ExpandFill; _name.TextOverrunBehavior=TextServer.OverrunBehavior.TrimEllipsis; top.AddChild(_name);
        _value=BrassUi.Text("",13,BrassUi.Paper); top.AddChild(_value);
        Meter=new BrassVitalMeter(); data.AddChild(Meter);
        AddChild(new BrassButtonFeedback());
        MouseEntered+=QueueRedraw; MouseExited+=QueueRedraw; Toggled+=_=>QueueRedraw(); ButtonDown+=QueueRedraw; ButtonUp+=QueueRedraw; Resized+=QueueRedraw;
    }
    public void UpdateMember(string name, double hp, double max, bool selected, bool paused, bool snap)
    {
        _name.Text=hp<=0 ? name+" · Caído" : name; _value.Text=$"{Math.Ceiling(hp):0}/{max:0}";
        TooltipText=$"{name} · {Math.Ceiling(hp):0}/{max:0} · "+(hp<=0?"Caído · ":"")+"Selecionar no Status, Herói e palco";
        SetPressedNoSignal(selected); Meter.Paused=paused; Meter.SetHealth(hp,max,snap);
        QueueRedraw();
    }
    public override void _Draw()
    {
        float s=BrassUi.Scale;
        DrawRect(new Rect2(0,0,Size.X,Size.Y),BrassUi.Soot);
        DrawRect(new Rect2(1*s,1*s,Size.X-2*s,Size.Y-3*s),ButtonPressed?BrassUi.Tobacco:BrassUi.Soot.Lightened(.035f));
        DrawLine(new Vector2(6*s,1*s),new Vector2(Size.X-6*s,1*s),BrassUi.Paper.Darkened(.55f),s);
        if (ButtonPressed) { DrawRect(new Rect2(0,6*s,3*s,Size.Y-12*s),BrassUi.Brass); DrawLine(new Vector2(4*s,7*s),new Vector2(7*s,10*s),BrassUi.Linen,s); }
        var center = new Vector2(23*s,Size.Y*.5f);
        DrawArc(center,14*s,Mathf.Pi*.68f,Mathf.Pi*2.32f,32,ButtonPressed?BrassUi.Brass:BrassUi.Paper.Darkened(.4f),s,true);
    }
}
