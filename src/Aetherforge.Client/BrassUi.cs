using Godot;
using System;
using System.Collections.Generic;

/// <summary>Original instrument UI: tobacco chassis, linen plates and a broken arch.</summary>
public static class BrassUi
{
    public static readonly Color Paper = new("#BFB49E"), Soot = new("#241F1A"),
        Tobacco = new("#40332B"), Wine = new("#6C3A32"), Brass = new("#B99B63"),
        Linen = new("#E4D8B7"), Ether = new("#97A581");

    private static float _scale = 1;
    private static readonly Dictionary<float, Theme> Themes = new();
    private static readonly Dictionary<(float Scale, bool Primary), StyleBoxFlat[]> ButtonStyles = new();
    private static readonly string[] ButtonStates = { "normal", "hover", "pressed", "hover_pressed", "disabled", "focus" };

    /// <summary>Set before constructing controls. Existing controls are not live-rescaled.</summary>
    public static float Scale
    {
        get => _scale;
        set => _scale = float.IsFinite(value) ? Mathf.Clamp(value, 1, 1.5f) : 1;
    }
    public static int S(float value) => (int)MathF.Round(value * Scale, MidpointRounding.AwayFromZero);
    public static Font BodyFont => BrassTypography.Body;
    public static Font StrongFont => BrassTypography.Strong;
    public static Font DisplayFont => BrassTypography.Display;
    public static Texture2D Icon(string key) => BrassIcons.Get(key);

    public static Theme CreateTheme()
    {
        if (Themes.TryGetValue(Scale, out var cached)) return cached;
        var theme = new Theme
        {
            DefaultFont = BodyFont,
            DefaultFontSize = S(BrassTypography.BodySize)
        };
        theme.SetColor("font_color", "Label", Linen);
        theme.SetFont("font", "Button", StrongFont);
        theme.SetFontSize("font_size", "Button", S(BrassTypography.BodySize));
        foreach (var state in new[] { "font_color", "font_hover_color", "font_pressed_color", "font_hover_pressed_color", "font_focus_color" })
            theme.SetColor(state, "Button", Linen);
        theme.SetColor("font_disabled_color", "Button", Paper.Darkened(.28f));
        var styles = Styles(false);
        for (var i = 0; i < ButtonStates.Length; i++) theme.SetStylebox(ButtonStates[i], "Button", styles[i]);
        theme.SetConstant("h_separation", "Button", S(8));
        theme.SetConstant("icon_max_width", "Button", S(20));
        theme.SetStylebox("background", "ProgressBar", Box(Soot, Soot, 0));
        theme.SetStylebox("fill", "ProgressBar", Box(Ether, Ether, 0));
        theme.SetConstant("separation", "VBoxContainer", S(8));
        theme.SetConstant("separation", "HBoxContainer", S(8));
        theme.SetConstant("h_separation", "GridContainer", S(6));
        theme.SetConstant("v_separation", "GridContainer", S(6));
        theme.SetConstant("line_spacing", "Label", S(4));
        theme.SetStylebox("panel", "PanelContainer", Box(Tobacco, Tobacco.Lightened(.12f), 0, 12, 10));
        theme.SetStylebox("panel", "TooltipPanel", Box(Soot, Paper, 1, 12, 8));
        theme.SetColor("font_color", "TooltipLabel", Linen);
        theme.SetFont("font", "TooltipLabel", BodyFont);
        theme.SetFontSize("font_size", "TooltipLabel", S(BrassTypography.BodySize));
        Themes.Add(Scale, theme);
        return theme;
    }

    public static StyleBoxFlat Box(Color fill, Color border, int width = 1, int horizontal = 0, int vertical = 0) => new()
    {
        BgColor = fill, BorderColor = border, BorderWidthLeft = S(width), BorderWidthRight = S(width),
        BorderWidthTop = S(width), BorderWidthBottom = S(width), ContentMarginLeft = S(horizontal),
        ContentMarginRight = S(horizontal), ContentMarginTop = S(vertical), ContentMarginBottom = S(vertical)
    };

    public static Label Text(string text, int size = 14, Color? color = null, bool title = false)
    {
        var label = new Label { Text = text, MouseFilter = Control.MouseFilterEnum.Ignore };
        label.AddThemeFontSizeOverride("font_size", S(size));
        label.AddThemeColorOverride("font_color", color ?? Linen);
        label.AddThemeFontOverride("font", title ? DisplayFont : BodyFont);
        return label;
    }

    public static Label Emphasis(string text, int size = 18, Color? color = null)
    {
        var label = Text(text, size, color);
        label.AddThemeFontOverride("font", StrongFont);
        return label;
    }

    public static Label Wrap(string text, int size = 13, Color? color = null)
    {
        var label = Text(text, size, color);
        label.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        label.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        return label;
    }

    public static Button Button(string text, Action action)
    {
        var button = new Button { Text = text, FocusMode = Control.FocusModeEnum.All };
        Quiet(button);
        button.Pressed += action;
        button.AddChild(new BrassButtonFeedback { Name = "MicroFeedback" });
        return button;
    }

    public static void Primary(Button button) => ApplyButton(button, true);
    public static void Quiet(Button button) => ApplyButton(button, false);

    private static void ApplyButton(Button button, bool primary)
    {
        var styles = Styles(primary);
        for (var i = 0; i < ButtonStates.Length; i++) button.AddThemeStyleboxOverride(ButtonStates[i], styles[i]);
        button.AddThemeFontOverride("font", StrongFont);
        button.AddThemeFontSizeOverride("font_size", S(BrassTypography.BodySize));
        foreach (var state in new[] { "font_color", "font_hover_color", "font_pressed_color", "font_hover_pressed_color", "font_focus_color" })
            button.AddThemeColorOverride(state, Linen);
        button.AddThemeColorOverride("font_disabled_color", Paper.Darkened(.28f));
        button.AddThemeConstantOverride("h_separation", S(8));
        button.AddThemeConstantOverride("icon_max_width", S(20));
        button.ExpandIcon = true;
        button.CustomMinimumSize = new Vector2(button.CustomMinimumSize.X,
            Mathf.Max(button.CustomMinimumSize.Y, S(BrassTypography.ButtonHeight)));
        // Style only: preserve callbacks, ToggleMode, ButtonPressed, Disabled and caller focus policy.
    }

    private static StyleBoxFlat[] Styles(bool primary)
    {
        var key = (Scale, primary);
        if (ButtonStyles.TryGetValue(key, out var cached)) return cached;
        var normal = Box(primary ? Wine : Soot, primary ? Brass : Tobacco.Lightened(.16f), 0, 12, 7);
        normal.BorderWidthLeft = primary ? S(3) : 0;
        normal.BorderWidthBottom = S(1);
        var hover = Box(primary ? Wine.Lightened(.1f) : Tobacco.Lightened(.07f), Paper, 0, 12, 7);
        hover.BorderWidthBottom = S(2);
        hover.BorderWidthLeft = primary ? S(3) : 0;
        var pressed = Box(Wine.Darkened(.12f), Linen, 0, 12, 7);
        pressed.BorderWidthLeft = S(4); // A latched selector has a visible physical key.
        pressed.BorderWidthTop = S(2);
        var hoverPressed = (StyleBoxFlat)pressed.Duplicate();
        hoverPressed.BgColor = Wine;
        var disabled = Box(Soot.Lightened(.06f), Tobacco, 0, 12, 7);
        disabled.BorderWidthBottom = S(1);
        var focus = Box(Colors.Transparent, Linen, 2);
        focus.DrawCenter = false; // A complete outline appears independently of hover/selection.
        cached = new[] { normal, hover, pressed, hoverPressed, disabled, focus };
        ButtonStyles.Add(key, cached);
        return cached;
    }

    public static MarginContainer Margin(int horizontal = 14, int vertical = 12)
    {
        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", S(horizontal));
        margin.AddThemeConstantOverride("margin_right", S(horizontal));
        margin.AddThemeConstantOverride("margin_top", S(vertical));
        margin.AddThemeConstantOverride("margin_bottom", S(vertical));
        return margin;
    }

    public static ProgressBar Meter(float height = 5) => new()
    {
        ShowPercentage = false, CustomMinimumSize = new Vector2(0, S(height)),
        SizeFlagsHorizontal = Control.SizeFlags.ExpandFill, MouseFilter = Control.MouseFilterEnum.Ignore
    };

    public static void FullRect(Control control) => control.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
}

/// <summary>The silhouette and ornament are drawn here, not copied from concept pixels.</summary>
public partial class BrassChassis : PanelContainer
{
    private int _instrument;
    /// <summary>0: straight regulation desk; 1: arched expedition case; 2: stepped route telegraph.</summary>
    public int Instrument
    {
        get => _instrument;
        set { _instrument = Mathf.Clamp(value, 0, 2); QueueRedraw(); }
    }

    public override void _Ready()
    {
        AddThemeStyleboxOverride("panel", new StyleBoxEmpty
        {
            ContentMarginLeft = BrassUi.S(18), ContentMarginRight = BrassUi.S(18),
            ContentMarginTop = BrassUi.S(12), ContentMarginBottom = BrassUi.S(15)
        });
        Resized += QueueRedraw;
    }

    public override void _Draw()
    {
        var w = Size.X; var h = Size.Y;
        var s = BrassUi.Scale;
        if (w < 48 * s || h < 64 * s) return;
        var outline = Outline(w, h, s);
        DrawColoredPolygon(outline, BrassUi.Tobacco);
        DrawPolyline(outline, BrassUi.Paper.Darkened(.38f), s, true);
        switch (Instrument)
        {
            case 0:
                // Flat technical plate, narrow brass corner brackets and a grounded plinth.
                DrawRect(new Rect2(7*s, 4*s, w-14*s, 7*s), BrassUi.Soot);
                DrawLine(new Vector2(9*s, 4*s), new Vector2(39*s, 4*s), BrassUi.Brass, 2*s);
                DrawLine(new Vector2(w-39*s, 4*s), new Vector2(w-9*s, 4*s), BrassUi.Brass, 2*s);
                DrawRect(new Rect2(7*s, h-8*s, w-14*s, 5*s), BrassUi.Soot);
                break;
            case 1:
                // Curved crown is part of the silhouette, not an arch painted on the same box.
                var crown = new Vector2[25];
                for (var i = 0; i < crown.Length; i++)
                {
                    var t = i / 24f;
                    crown[i] = new Vector2(8*s + (w-16*s)*t, (7 + 22*Mathf.Pow(2*t-1, 2))*s);
                }
                DrawPolyline(crown, BrassUi.Brass.Darkened(.12f), s, true);
                DrawLine(new Vector2(8*s, 32*s), new Vector2(8*s, 59*s), BrassUi.Brass, 2*s);
                DrawLine(new Vector2(w-8*s, 32*s), new Vector2(w-8*s, 59*s), BrassUi.Brass, 2*s);
                break;
            case 2:
                // Raised central telegraph head and two stair shoulders, with restrained contacts.
                DrawLine(new Vector2(w*.34f, 4*s), new Vector2(w*.66f, 4*s), BrassUi.Brass, 2*s);
                foreach (var x in new[] { 8*s, w-8*s })
                    for (var i = 0; i < 3; i++)
                        DrawLine(new Vector2(x-2*s, (35+i*5)*s), new Vector2(x+2*s, (35+i*5)*s), BrassUi.Brass, s);
                DrawLine(new Vector2(12*s, h-6*s), new Vector2(w-12*s, h-6*s), BrassUi.Soot, 3*s);
                break;
        }
        // Fixings stay on the outer rail, outside readable surfaces.
        foreach (var x in new[] { 8*s, w-8*s })
        {
            foreach (var y in new[] { 69*s, h-20*s })
            {
                DrawCircle(new Vector2(x, y), 2*s, BrassUi.Brass.Darkened(.1f));
                DrawLine(new Vector2(x-s, y+s), new Vector2(x+s, y-s), BrassUi.Soot, s, true);
            }
        }
    }

    private Vector2[] Outline(float w, float h, float s)
    {
        var points = new List<Vector2>();
        switch (Instrument)
        {
            case 0:
                points.AddRange(new[] { new Vector2(7*s, 2*s), new Vector2(w-7*s, 2*s), new Vector2(w-2*s, 7*s) });
                break;
            case 1:
                for (var i = 0; i <= 32; i++)
                {
                    var t = i / 32f;
                    points.Add(new Vector2(2*s + (w-4*s)*t, (2 + 23*Mathf.Pow(2*t-1, 2))*s));
                }
                break;
            default:
                points.AddRange(new[] { new Vector2(2*s, 19*s), new Vector2(w*.18f, 19*s),
                    new Vector2(w*.18f, 10*s), new Vector2(w*.32f, 10*s), new Vector2(w*.32f, 2*s),
                    new Vector2(w*.68f, 2*s), new Vector2(w*.68f, 10*s), new Vector2(w*.82f, 10*s),
                    new Vector2(w*.82f, 19*s), new Vector2(w-2*s, 19*s) });
                break;
        }
        points.AddRange(new[] { new Vector2(w-2*s, h-8*s), new Vector2(w-8*s, h-2*s),
            new Vector2(8*s, h-2*s), new Vector2(2*s, h-8*s) });
        if (Instrument == 0) points.Add(new Vector2(2*s, 7*s));
        points.Add(points[0]);
        return points.ToArray();
    }
}

public partial class BrassArch : Control
{
    public override void _Ready() { MouseFilter = MouseFilterEnum.Ignore; Resized += QueueRedraw; }
    public override void _Draw()
    {
        var s = BrassUi.Scale;
        var center = new Vector2(Size.X / 2, Size.Y - 2*s);
        for (var i = 0; i < 3; i++)
        {
            var r = (8 + i * 5)*s;
            DrawArc(center, r, Mathf.Pi, Mathf.Pi*1.47f, 24, BrassUi.Brass, 1.5f*s, true);
            DrawArc(center, r, Mathf.Pi*1.53f, Mathf.Tau, 24, BrassUi.Brass, 1.5f*s, true);
        }
        DrawLine(center + new Vector2(0, 2*s), center + new Vector2(0, -21*s), BrassUi.Linen, 1.2f*s, true);
    }
}

public partial class BrassPortraitFrame : PanelContainer
{
    public override void _Ready()
    {
        AddThemeStyleboxOverride("panel", BrassUi.Box(BrassUi.Paper, BrassUi.Brass, 2, 7, 7));
        Resized += QueueRedraw;
    }
    public override void _Draw()
    {
        // Original double-arch bezel; the complete figure retains its source aspect ratio.
        var radius = Mathf.Min(Size.X * .36f, Size.Y * .32f);
        var s = BrassUi.Scale;
        var center = new Vector2(Size.X / 2, radius + 4*s);
        DrawArc(center, radius, Mathf.Pi, Mathf.Tau, 48, BrassUi.Brass, 2*s, true);
        DrawLine(center + new Vector2(-radius, 0), new Vector2(center.X-radius, Size.Y-4*s), BrassUi.Brass, 2*s, true);
        DrawLine(center + new Vector2(radius, 0), new Vector2(center.X+radius, Size.Y-4*s), BrassUi.Brass, 2*s, true);
    }
}

/// <summary>Unavailable equipment socket: an engraved empty ring, never a pretend item.</summary>
public partial class BrassEmptySlot : Control
{
    public override void _Ready() { MouseFilter = MouseFilterEnum.Ignore; Resized += QueueRedraw; }
    public override void _Draw()
    {
        var s = BrassUi.Scale;
        if (Size.X <= 2*s || Size.Y <= 2*s) return;
        DrawRect(new Rect2(Vector2.One*s, Size - Vector2.One*2*s), BrassUi.Soot);
        DrawRect(new Rect2(Vector2.One*s, Size - Vector2.One*2*s), BrassUi.Tobacco.Lightened(.15f), false, s);
        DrawCircle(Size / 2, 4*s, BrassUi.Paper.Darkened(.35f), false, s, true);
    }
}

public partial class BrassRoute : Control
{
    public int StageCount => 5;
    private int _current, _victories;
    private static readonly string[] Names = { "Entrada", "Trilhos", "Pressão", "Ninho", "Fornalha" };
    public override void _Ready() { MouseFilter = MouseFilterEnum.Ignore; Resized += QueueRedraw; }
    public void UpdateRoute(int current, int victories)
    {
        if (_current == current && _victories == victories) return;
        _current = current; _victories = victories; QueueRedraw();
    }
    public override void _Draw()
    {
        DrawRect(new Rect2(Vector2.Zero, Size), BrassUi.Linen);
        var font = BrassUi.BodyFont;
        var strong = BrassUi.StrongFont;
        var s = BrassUi.Scale;
        var fontSize = BrassUi.S(14);
        var metaSize = BrassUi.S(13);
        if (Size.X < 60*s || Size.Y < 70*s) return;
        var points = new Vector2[5];
        var row = (Size.Y-12*s)/5;
        for (var i = 0; i < 5; i++) points[i] = new Vector2(22*s, 6*s + row*(i+.5f));
        for (var i = 1; i < 5; i++) DrawLine(points[i-1], points[i], i <= _victories ? BrassUi.Wine : BrassUi.Paper.Darkened(.15f), 2*s, true);
        for (var i = 0; i < 5; i++)
        {
            var done = i < _victories;
            var current = !done && i == _current;
            var p = points[i];
            var state = done ? "Concluído" : current ? "Atual" : "Futuro";
            if (done)
            {
                DrawRect(new Rect2(p-Vector2.One*8*s, Vector2.One*16*s), BrassUi.Wine);
                DrawPolyline(new[] { p+new Vector2(-4,0)*s, p+new Vector2(-1,4)*s, p+new Vector2(5,-5)*s }, BrassUi.Linen, 2*s, true);
            }
            else if (current)
            {
                var diamond = new[] { p+Vector2.Up*11*s, p+Vector2.Right*11*s, p+Vector2.Down*11*s, p+Vector2.Left*11*s, p+Vector2.Up*11*s };
                DrawColoredPolygon(diamond, BrassUi.Wine);
                DrawCircle(p, 3*s, BrassUi.Linen);
            }
            else
            {
                DrawCircle(p, 8*s, BrassUi.Linen);
                DrawCircle(p, 8*s, BrassUi.Tobacco, false, s, true);
            }
            var stateWidth = font.GetStringSize(state, HorizontalAlignment.Left, -1, metaSize).X;
            var nameWidth = strong.GetStringSize(Names[i], HorizontalAlignment.Left, -1, fontSize).X;
            var twoLines = 48*s + nameWidth + 16*s + stateWidth > Size.X-12*s;
            var baseline = p.Y + strong.GetAscent(fontSize) - strong.GetHeight(fontSize)*.5f;
            if (twoLines) baseline -= 7*s;
            DrawString(strong, new Vector2(42*s, baseline), Names[i], HorizontalAlignment.Left, -1, fontSize, BrassUi.Soot);
            DrawString(font, new Vector2(twoLines ? 42*s : Size.X-12*s-stateWidth, twoLines ? baseline+15*s : baseline),
                state, HorizontalAlignment.Left, -1, metaSize, BrassUi.Soot);
        }
        DrawLine(new Vector2(0, s), new Vector2(Size.X, s), BrassUi.Brass, 2*s);
        DrawLine(new Vector2(0, Size.Y-s), new Vector2(Size.X, Size.Y-s), BrassUi.Paper, s);
    }
}
