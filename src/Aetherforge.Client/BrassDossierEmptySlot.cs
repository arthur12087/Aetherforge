using Godot;

/// <summary>A blank, unavailable paper square, not an equipment item or a rarity frame.</summary>
public partial class BrassDossierEmptySlot : Control
{
    public override void _Ready() { MouseFilter = MouseFilterEnum.Ignore; Resized += QueueRedraw; }
    public override void _Draw()
    {
        float s = BrassUi.Scale;
        if (Size.X <= 2*s || Size.Y <= 2*s) return;
        var r = new Rect2(Vector2.One*s, Size-Vector2.One*2*s);
        DrawRect(r, BrassUi.Paper);
        DrawRect(r, BrassUi.Tobacco with { A=.55f }, false, s);
        DrawLine(Size/2-new Vector2(2,-2)*s, Size/2+new Vector2(2,-2)*s, BrassUi.Tobacco, s, true);
    }
}
