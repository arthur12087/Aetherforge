using Godot;

/// <summary>
/// Native, editable A/B specimen for a 1120x680 viewport. Owns only its controls;
/// Main owns mode selection, viewport, duration, capture and application lifetime.
/// </summary>
public partial class BrassTypeProof : Control
{
    public override void _Ready()
    {
        Theme = BrassUi.CreateTheme();
        BrassUi.FullRect(this);
        var background = new ColorRect { Color = BrassUi.Soot, MouseFilter = MouseFilterEnum.Ignore };
        AddChild(background);
        BrassUi.FullRect(background);

        // Fixed optical sizes are deliberate: this is an A/B specimen in screen pixels,
        // not a miniature game scene or an attempt to change the application's UI scale.
        var margin = new MarginContainer();
        foreach (var edge in new[] { "left", "right", "top", "bottom" })
            margin.AddThemeConstantOverride($"margin_{edge}", 16);
        AddChild(margin);
        BrassUi.FullRect(margin);
        var scroll = new ScrollContainer
        {
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
            SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsVertical = SizeFlags.ExpandFill
        };
        margin.AddChild(scroll);
        var page = Column(8);
        page.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        scroll.AddChild(page);
        page.AddChild(Sample("Prova de leitura", BrassUi.DisplayFont, 26));
        page.AddChild(Sample("ESPÉCIME TIPOGRÁFICO · Jost / Source Sans 3 · pesos reais · sem sessão de jogo", BrassUi.BodyFont, 14, BrassUi.Paper));

        var comparison = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        comparison.AddThemeConstantOverride("separation", 16);
        page.AddChild(comparison);
        comparison.AddChild(Family("Jost", "Jost"));
        comparison.AddChild(Family("SourceSans3", "Source Sans 3"));

        var footer = new HBoxContainer();
        footer.AddThemeConstantOverride("separation", 10);
        page.AddChild(footer);
        foreach (var key in new[] { "shield", "rifle", "heal", "pause", "return", "route" })
            footer.AddChild(new TextureRect
            {
                Texture = BrassUi.Icon(key), CustomMinimumSize = new Vector2(24, 24),
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered, MouseFilter = MouseFilterEnum.Ignore
            });
        footer.AddChild(Sample("13 / 14 / 18 / 26 px · Tab / Enter: experimentar o escudo", BrassUi.BodyFont, 13, BrassUi.Paper));
    }

    private static PanelContainer Family(string family, string title)
    {
        var regular = BrassTypography.Weight(family, 400);
        var strong = BrassTypography.Weight(family, 600);
        var panel = new PanelContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        panel.AddThemeStyleboxOverride("panel", Plate(BrassUi.Tobacco, 14, 12));
        var column = Column(6);
        panel.AddChild(column);
        column.AddChild(Sample(title, strong, 18));
        var grid = new GridContainer { Columns = 3, SizeFlagsHorizontal = SizeFlags.ExpandFill };
        grid.AddThemeConstantOverride("h_separation", 12);
        grid.AddThemeConstantOverride("v_separation", 6);
        column.AddChild(grid);
        grid.AddChild(Sample("px", regular, 13, BrassUi.Paper));
        grid.AddChild(Sample("400 · Regular", regular, 13, BrassUi.Paper));
        grid.AddChild(Sample("600 · Semibold", strong, 13, BrassUi.Paper));
        foreach (var size in new[] { 13, 14, 18, 26 })
        {
            grid.AddChild(Sample(size.ToString(), regular, 13, BrassUi.Paper));
            grid.AddChild(Sample("Éter · Pressão", regular, size));
            grid.AddChild(Sample("Éter · Pressão", strong, size));
        }
        column.AddChild(Sample("Alquimista · proteção · infusão · ç ã é ê ó", regular, 14));
        column.AddChild(Sample("0123456789   0 O   1 I l   100%   01:54", regular, 14));
        column.AddChild(Sample("0123456789   −28   +12   75%", strong, 18));

        var plate = new PanelContainer();
        plate.AddThemeStyleboxOverride("panel", Plate(BrassUi.Soot, 12, 6));
        column.AddChild(plate);
        var plateColumn = Column(4);
        plate.AddChild(plateColumn);
        plateColumn.AddChild(Sample("Guarda da Caldeira", strong, 18));
        plateColumn.AddChild(Sample("Placa e valores de demonstração · 138 / 180", regular, 13, BrassUi.Paper));
        var meter = new ProgressBar
        {
            MaxValue = 180, Value = 138, ShowPercentage = false,
            CustomMinimumSize = new Vector2(0, 6), MouseFilter = MouseFilterEnum.Ignore
        };
        meter.AddThemeStyleboxOverride("background", Plate(BrassUi.Tobacco, 0, 0));
        meter.AddThemeStyleboxOverride("fill", Plate(BrassUi.Ether, 0, 0));
        plateColumn.AddChild(meter);

        var shield = BrassUi.Button("Escudo de Pressão · Pronto", () => { });
        BrassUi.Primary(shield);
        shield.Name = $"{family}SpecimenShield";
        shield.ToggleMode = true;
        shield.Icon = BrassUi.Icon("shield");
        shield.AddThemeFontOverride("font", strong);
        shield.AddThemeFontSizeOverride("font_size", 14);
        shield.TooltipText = "Somente um espécime de botão. Alterna a apresentação, sem regras ou combate.";
        shield.Toggled += active => shield.Text = active ? "Escudo de Pressão · Protegido" : "Escudo de Pressão · Pronto";
        column.AddChild(shield);
        column.AddChild(Sample("Escudo demonstrativo · não inicia combate nem recarga", regular, 13, BrassUi.Paper));
        return panel;
    }

    private static VBoxContainer Column(int gap)
    {
        var column = new VBoxContainer();
        column.AddThemeConstantOverride("separation", gap);
        return column;
    }

    private static Label Sample(string text, Font font, int size, Color? color = null)
    {
        var label = new Label { Text = text, MouseFilter = MouseFilterEnum.Ignore };
        label.AddThemeFontOverride("font", font);
        label.AddThemeFontSizeOverride("font_size", size);
        label.AddThemeColorOverride("font_color", color ?? BrassUi.Linen);
        return label;
    }

    private static StyleBoxFlat Plate(Color fill, int horizontal, int vertical) => new()
    {
        BgColor = fill, ContentMarginLeft = horizontal, ContentMarginRight = horizontal,
        ContentMarginTop = vertical, ContentMarginBottom = vertical
    };
}
