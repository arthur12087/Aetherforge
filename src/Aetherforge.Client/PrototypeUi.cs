using Godot;

public static class PrototypeUi
{
    public static readonly Color Ink = new("eee1c4"), Gold = new("daa667"), Muted = new("9eafa9");
    public static Theme Theme()
    {
        var theme = new Theme { DefaultFontSize = 15 };
        theme.SetColor("font_color", "Label", Ink);
        theme.SetColor("font_color", "Button", Ink);
        theme.SetColor("font_hover_color", "Button", Colors.White);
        foreach (var state in new[] { "normal", "hover", "pressed", "focus" })
        {
            var style = Box(state == "normal" ? new Color("253538") : new Color("3d4c49"), Gold.Darkened(.55f));
            style.ContentMarginLeft = 15; style.ContentMarginRight = 15;
            style.ContentMarginTop = 8; style.ContentMarginBottom = 8;
            theme.SetStylebox(state, "Button", style);
        }
        return theme;
    }
    public static StyleBoxFlat Box(Color fill, Color border)
    {
        return new StyleBoxFlat { BgColor = fill, BorderColor = border, BorderWidthBottom = 1, BorderWidthTop = 1, BorderWidthLeft = 1, BorderWidthRight = 1, CornerRadiusTopLeft = 10, CornerRadiusTopRight = 10, CornerRadiusBottomLeft = 10, CornerRadiusBottomRight = 10 };
    }
    public static Label Label(string text, int size = 15, Color? color = null)
    {
        var label = new Label { Text = text, MouseFilter = Control.MouseFilterEnum.Ignore };
        label.AddThemeFontSizeOverride("font_size", size);
        label.AddThemeColorOverride("font_color", color ?? Ink);
        return label;
    }
}
