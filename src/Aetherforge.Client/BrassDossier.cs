using Godot;

/// <summary>Original V16 paper instrument. Containers own content; ornament never owns input.</summary>
public partial class BrassDossier : PanelContainer
{
    public override void _Ready()
    {
        AddThemeStyleboxOverride("panel", new StyleBoxEmpty
        {
            ContentMarginLeft = BrassUi.S(16), ContentMarginRight = BrassUi.S(16),
            ContentMarginTop = BrassUi.S(10), ContentMarginBottom = BrassUi.S(12)
        });
        Resized += QueueRedraw;
    }
    public override void _Draw()
    {
        float s = BrassUi.Scale;
        if (Size.X < 8*s || Size.Y < 8*s) return;
        DrawRect(new Rect2(4*s, 4*s, Size.X-4*s, Size.Y-4*s), BrassUi.Tobacco);
        var sheet = new Rect2(0, 0, Size.X-4*s, Size.Y-4*s);
        DrawRect(sheet, BrassUi.Linen);
        DrawRect(sheet, BrassUi.Tobacco, false, s);
        // No texture under data. A single fold, not a brass frame around every component.
        DrawLine(new Vector2(2*s, Size.Y-7*s), new Vector2(Size.X-8*s, Size.Y-7*s), BrassUi.Paper, s);
    }
}

/// <summary>One shared attribute ruler; three native text columns, no fake scales or socket gauges.</summary>
public partial class BrassDossierRuler : PanelContainer
{
    public override void _Ready()
    {
        AddThemeStyleboxOverride("panel", BrassUi.Box(BrassUi.Paper, BrassUi.Paper, 0, 8, 6));
        Resized += QueueRedraw;
    }
    public override void _Draw()
    {
        float s = BrassUi.Scale;
        for (int i = 1; i < 3; i++)
            DrawLine(new Vector2(Size.X*i/3, 9*s), new Vector2(Size.X*i/3, Size.Y-9*s), BrassUi.Tobacco with { A=.5f }, s);
    }
}
