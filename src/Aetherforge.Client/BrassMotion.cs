using Godot;
using System;

/// <summary>Presentation-only preference. No simulation time scale or saved game state.</summary>
public static class BrassMotion
{
    private static bool _reduced;
    public static event Action? Changed;
    public static bool Reduced
    {
        get => _reduced;
        set { if (_reduced == value) return; _reduced = value; Changed?.Invoke(); }
    }
}

/// <summary>A single bounded, interruptible accent; never moves text, layout or hit rectangles.</summary>
public partial class BrassButtonFeedback : Control
{
    private Button _button = null!;
    private Tween? _tween;
    private float _amount;
    private bool _hover, _focus, _down;
    public int TransitionCount { get; private set; }
    public float AccentAmount => _amount;
    public bool Animating => _tween?.IsRunning() == true;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        _button = (Button)GetParent();
        BrassUi.FullRect(this);
        _button.MouseEntered += Enter; _button.MouseExited += Leave;
        _button.FocusEntered += FocusIn; _button.FocusExited += FocusOut;
        _button.ButtonDown += Down; _button.ButtonUp += Up;
        _button.Resized += QueueRedraw;
        BrassMotion.Changed += UpdateAccent;
    }
    private void Enter() { _hover = true; UpdateAccent(); }
    private void Leave() { _hover = false; _down = false; UpdateAccent(); }
    private void FocusIn() { _focus = true; UpdateAccent(); }
    private void FocusOut() { _focus = false; _down = false; UpdateAccent(); }
    private void Down() { _down = true; UpdateAccent(); }
    private void Up() { _down = false; UpdateAccent(); }
    private void UpdateAccent()
    {
        if (!IsInsideTree()) return;
        _tween?.Kill(); _tween = null;
        float target = _button.Disabled ? 0 : _down ? 1 : _focus || _hover ? .65f : 0;
        TransitionCount++;
        if (BrassMotion.Reduced) { _amount = target; QueueRedraw(); return; }
        _tween = CreateTween().SetIgnoreTimeScale(true);
        _tween.SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
        _tween.TweenMethod(Callable.From<float>(value => { _amount = value; QueueRedraw(); }), _amount, target, _down ? .08 : .14);
    }
    public override void _Draw()
    {
        if (_button == null || _button.Disabled || _amount < .01f) return;
        float s = BrassUi.Scale, length = Mathf.Max(0, Size.X - 16*s) * _amount;
        DrawLine(new Vector2(8*s, Size.Y-3*s), new Vector2(8*s+length, Size.Y-3*s), BrassUi.Brass, s);
        if (_down) DrawLine(new Vector2(4*s, 4*s), new Vector2(4*s, Size.Y-4*s), BrassUi.Linen, 2*s);
    }
    public override void _ExitTree()
    {
        _tween?.Kill(); BrassMotion.Changed -= UpdateAccent;
        _button.MouseEntered -= Enter; _button.MouseExited -= Leave;
        _button.FocusEntered -= FocusIn; _button.FocusExited -= FocusOut;
        _button.ButtonDown -= Down; _button.ButtonUp -= Up;
        _button.Resized -= QueueRedraw;
    }
}
