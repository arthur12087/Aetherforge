using Godot;
using Aetherforge.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>Local expedition instruments. This view alone advances the simulation.</summary>
public partial class ExpeditionView : Control
{
    public ExpeditionSimulation Simulation { get; } = new();
    public ExpeditionStage Stage { get; private set; } = null!;
    /// <summary>Set before Ready. Uses the same three Window contents, embedded for review.</summary>
    public bool Overview { get; set; }
    public int SelectedMemberIndex => _selected;
    public bool DetailedHud
    {
        get => Stage != null && Stage.DetailedHud;
        set { if (Stage != null) Stage.DetailedHud = value; if (_detailsButton != null) _detailsButton.SetPressedNoSignal(value); }
    }
    public int InventoryColumns => _bag.Columns;
    public int MemberSelectorCount => _selectors.Length;
    public int RouteNodeCount => _route.StageCount;

    private readonly Window[] _menus = new Window[3];
    private readonly Button[] _launchers = new Button[3], _selectors = new Button[3];
    private readonly BrassPartyRow[] _statusMembers = new BrassPartyRow[3];
    private readonly Label[] _stats = new Label[3];
    private bool _snapVitals = true;
    private readonly List<Button> _pauseButtons = new(), _retreatButtons = new();
    private readonly List<string> _recent = new();
    private GridContainer _bag = null!;
    private ExpeditionMenuRoute _route = null!;
    private Label _selectedName = null!, _heroName = null!, _heroRole = null!, _shieldState = null!,
        _healState = null!, _phase = null!, _ticket = null!, _journal = null!, _stageTitle = null!, _stageEvent = null!, _maxHealth = null!;
    private Button _shield = null!, _dockButton = null!, _detailsButton = null!;
    private TextureRect _portrait = null!;
    private readonly AtlasTexture[] _portraits = new AtlasTexture[3];
    private Control _menuSpace = null!;
    private int _selected, _lastEventId = -1;
    private double _refreshIn;
    private bool _headless, _docked = true, _ready;
    private readonly List<int> _openOrder = new();
    private readonly Control?[] _returnFocus = new Control?[3];
    private readonly Button[] _closeButtons = new Button[3];
    private readonly ScrollContainer[] _menuScrolls = new ScrollContainer[3];
    private readonly VBoxContainer[] _menuFooters = new VBoxContainer[3];
    private readonly VBoxContainer[] _menuHeaders = new VBoxContainer[3];
    private Control _toolbar = null!, _rootFooter = null!, _stagePanel = null!;
    private Label _overviewCaption = null!;
    private static int S(float value) => BrassUi.S(value);

    public override void _Ready()
    {
        _headless = DisplayServer.GetName() == "headless";
        Theme = BrassUi.CreateTheme();
        BrassUi.FullRect(this);
        GetViewport().GuiEmbedSubwindows = Overview || _headless;
        BuildRoot();
        BuildMenu(0, "STATUS", "Mesa de regulagem", BuildStatus);
        BuildMenu(1, "HERÓI", "Estojo de expedição", BuildHero);
        BuildMenu(2, "EXPEDIÇÃO", "Telégrafo de rotas", BuildRoute);
        _ready = true;
        if (!_headless) ConfigureRoot(true);
        else GetWindow().Size = Overview ? new Vector2I(1160, 880) : new Vector2I(1000, 320);
        GetWindow().SizeChanged += ArrangeMenus;
        ArrangeMenus();
        // Native windows start closed so launching the idle never takes focus three times.
        // Review mode deliberately shows all three instruments in one captured viewport.
        if (Overview) for (var i = 0; i < 3; i++) SetMenuVisible(i, true, false);
        SelectMember(0);
        RefreshState();
        if (!_headless && !Overview) _launchers[0].GrabFocus();
    }

    private void BuildRoot()
    {
        var background = new ColorRect { Color = Overview ? BrassUi.Paper : BrassUi.Soot, MouseFilter = MouseFilterEnum.Ignore };
        AddChild(background); BrassUi.FullRect(background);
        var margin = BrassUi.Margin(Overview ? 20 : 10, Overview ? 14 : 7);
        AddChild(margin); BrassUi.FullRect(margin);
        var column = new VBoxContainer(); margin.AddChild(column);
        // Flow only the toolbar on narrow windows; never inflate the compact stage to fit text.
        var toolbar = new HFlowContainer(); _toolbar = toolbar; column.AddChild(toolbar);
        var brand = new HBoxContainer(); toolbar.AddChild(brand);
        brand.AddChild(new BrassArch { CustomMinimumSize = new Vector2(S(32), S(25)) });
        brand.AddChild(BrassUi.Text("AETHERFORGE", 20, Overview ? BrassUi.Soot : BrassUi.Linen, true));
        string[] titles = { "Status", "Herói", "Expedição" };
        for (var i = 0; i < 3; i++)
        {
            int index = i;
            _launchers[i] = BrassUi.Button(titles[i], () => SetMenuVisible(index, !_menus[index].Visible));
            _launchers[i].Name = "OpenInstrument" + i;
            _launchers[i].ToggleMode = true;
            BrassUi.Quiet(_launchers[i]);
            toolbar.AddChild(_launchers[i]);
        }
        _detailsButton = BrassUi.Button("Detalhes", () =>
        {
            DetailedHud = !DetailedHud;
        });
        _detailsButton.Name = "DetailedHudToggle";
        _detailsButton.ToggleMode = true;
        _detailsButton.TooltipText = "Exibir nomes e detalhes no palco. O integrante selecionado também pode ser inspecionado em Herói.";
        BrassUi.Quiet(_detailsButton); toolbar.AddChild(_detailsButton);
        _dockButton = BrassUi.Button("Barra / janela", ToggleDock); BrassUi.Quiet(_dockButton); toolbar.AddChild(_dockButton);
        if (Overview)
        {
            _overviewCaption = BrassUi.Text("A PROMESSA DO LATÃO   /   GALERIAS DA CALDEIRA", 13, BrassUi.Tobacco);
            column.AddChild(_overviewCaption);
            _menuSpace = new Control { SizeFlagsVertical = SizeFlags.ExpandFill };
            column.AddChild(_menuSpace);
            _menuSpace.Resized += ArrangeMenus;
        }
        var stagePanel = new PanelContainer { SizeFlagsVertical = Overview ? SizeFlags.Fill : SizeFlags.ExpandFill };
        _stagePanel = stagePanel;
        stagePanel.AddThemeStyleboxOverride("panel", BrassUi.Box(BrassUi.Soot, BrassUi.Brass, 1, 3, 3));
        column.AddChild(stagePanel);
        var stageColumn = new VBoxContainer(); stageColumn.AddThemeConstantOverride("separation", 2); stagePanel.AddChild(stageColumn);
        _stageTitle = BrassUi.Text("GALERIAS DA CALDEIRA", 13);
        _stageTitle.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
        stageColumn.AddChild(_stageTitle);
        // Stage geometry is deliberately independent of text scale.
        Stage = new ExpeditionStage { Name = "ExpeditionStage", CustomMinimumSize = new Vector2(0, Overview ? 180 : 100), SizeFlagsVertical = SizeFlags.ExpandFill, SizeFlagsHorizontal = SizeFlags.ExpandFill };
        Stage.Bind(Simulation); stageColumn.AddChild(Stage);
        _stageEvent = BrassUi.Text("Equipe em marcha", 13, BrassUi.Paper);
        _stageEvent.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
        stageColumn.AddChild(_stageEvent);
        var footer = new HFlowContainer(); _rootFooter = footer; column.AddChild(footer);
        footer.AddChild(BrassUi.Text("Espaço: pausa   ·   P: escudo   ·   R: reiniciar", 13, Overview ? BrassUi.Tobacco : BrassUi.Paper));
        footer.AddChild(new Control { SizeFlagsHorizontal = SizeFlags.ExpandFill });
        footer.AddChild(BrassUi.Text("LOCAL · sem API", 13, Overview ? BrassUi.Tobacco : BrassUi.Ether));
    }

    private static VBoxContainer Column(int gap = 8)
    {
        var column = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        column.AddThemeConstantOverride("separation", S(gap));
        return column;
    }

    private static Label Heading(string text, int size = 18, Color? color = null)
    {
        var label = BrassUi.Emphasis(text, size, color);
        label.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        label.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        return label;
    }

    private static void SetIcon(Button button, string key)
    {
        button.Icon = BrassUi.Icon(key);
        button.ExpandIcon = true;
        button.AddThemeConstantOverride("icon_max_width", S(20));
    }

    private void BuildMenu(int index, string title, string subtitle, Action<VBoxContainer, VBoxContainer> build)
    {
        var window = new Window
        {
            Name = "Instrument" + index, Title = "Aetherforge · " + title,
            Size = new Vector2I(S(380), S(620)), MinSize = new Vector2I(300, S(280)),
            Visible = false, Borderless = true, Transient = false, Exclusive = false,
            AlwaysOnTop = !Overview && !_headless && _docked,
            Theme = Theme, WrapControls = false, GuiDisableInput = false, HandleInputLocally = true
        };
        _menus[index] = window; AddChild(window);
        window.CloseRequested += () => SetMenuVisible(index, false);
        window.AddChild(new ExpeditionMenuInput { Handle = input => HandleKey(input, window, index) });
        // Chassis fills the viewport. Its minimum comes only from fixed header/footer;
        // the ScrollContainer deliberately contributes no vertical content minimum.
        var chassis = new BrassChassis { Name = "InstrumentChassis", Instrument = index };
        window.AddChild(chassis); BrassUi.FullRect(chassis);
        var shell = Column(8); chassis.AddChild(shell);
        var top = Column(index == 1 ? 4 : 2); top.Name = "FixedHeader"; shell.AddChild(top);
        _menuHeaders[index] = top;
        var crest = new HBoxContainer(); top.AddChild(crest);
        if (index == 0)
        {
            crest.AddChild(BrassUi.Text("01 / STATUS", 13, BrassUi.Paper));
            crest.AddChild(new BrassArch { CustomMinimumSize = new Vector2(S(34), S(18)), SizeFlagsHorizontal = SizeFlags.ExpandFill });
        }
        else if (index == 1)
            crest.AddChild(new BrassArch { CustomMinimumSize = new Vector2(0, S(28)), SizeFlagsHorizontal = SizeFlags.ExpandFill });
        else
        {
            var routeIcon = new TextureRect { Texture = BrassUi.Icon("route"), ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered, CustomMinimumSize = new Vector2(S(22), S(22)), MouseFilter = MouseFilterEnum.Ignore };
            crest.AddChild(routeIcon);
            crest.AddChild(BrassUi.Text("03 / ROTA DE CAÇA", 13, BrassUi.Paper));
        }
        var header = new HBoxContainer(); top.AddChild(header);
        var plate = new BrassInstrumentPlate { Name = "EngravedTitle", SizeFlagsHorizontal = SizeFlags.ExpandFill, MouseFilter = MouseFilterEnum.Stop };
        header.AddChild(plate);
        var heading = BrassUi.Text(title, 24, BrassUi.Soot, true);
        heading.HorizontalAlignment = index == 1 ? HorizontalAlignment.Center : HorizontalAlignment.Left;
        plate.AddChild(heading);
        var close = BrassUi.Button("×", () => SetMenuVisible(index, false));
        close.Name = "CloseInstrument" + index; _closeButtons[index] = close;
        close.TooltipText = "Fechar menu · Escape"; BrassUi.Quiet(close);
        // Keep the complete instrument title on its own plate at large text scales.
        if (index == 2) crest.AddChild(new Control { SizeFlagsHorizontal = SizeFlags.ExpandFill });
        crest.AddChild(close);
        bool dragging = false; Vector2I origin = default, mouseOrigin = default;
        plate.GuiInput += input =>
        {
            if (input is InputEventMouseButton click && click.ButtonIndex == MouseButton.Left)
            {
                dragging = click.Pressed; origin = window.Position;
                mouseOrigin = (Vector2I)window.GetMousePosition() + window.Position;
            }
            if (input is InputEventMouseMotion && dragging)
            {
                var mouse = (Vector2I)window.GetMousePosition() + window.Position;
                window.Position = ClampPosition(origin + mouse - mouseOrigin, window.Size, MenuBounds());
            }
        };
        var caption = BrassUi.Text(subtitle, 13, BrassUi.Paper);
        caption.HorizontalAlignment = index == 1 ? HorizontalAlignment.Center : HorizontalAlignment.Left; top.AddChild(caption);
        var scroll = new ScrollContainer
        {
            Name = "ScrollableBody", HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
            VerticalScrollMode = ScrollContainer.ScrollMode.Auto, FollowFocus = true,
            FocusMode = FocusModeEnum.All,
            SizeFlagsVertical = SizeFlags.ExpandFill, SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        shell.AddChild(scroll); _menuScrolls[index] = scroll;
        scroll.TooltipText = "Setas e Page Up / Page Down: rolar conteúdo. Tab: ações fixas.";
        scroll.GuiInput += input =>
        {
            if (!scroll.HasFocus() || input is not InputEventKey key || !key.Pressed || key.CtrlPressed || key.AltPressed || key.MetaPressed) return;
            int step = key.Keycode switch
            {
                Key.Up => -S(36), Key.Down => S(36),
                Key.Pageup => -(int)scroll.Size.Y, Key.Pagedown => (int)scroll.Size.Y,
                _ => 0
            };
            if (step == 0) return;
            scroll.ScrollVertical = Math.Max(0, scroll.ScrollVertical + step); scroll.AcceptEvent();
        };
        var content = Column(10); content.Name = "MenuContent"; scroll.AddChild(content);
        var footer = Column(6); footer.Name = "FixedFooter"; shell.AddChild(footer); _menuFooters[index] = footer;
        build(content, footer);
        WireFocus(window);
    }

    private void BuildStatus(VBoxContainer column, VBoxContainer footer)
    {
        column.AddThemeConstantOverride("separation", S(6));
        _selectedName = Heading(""); _selectedName.TooltipText = "Integrante selecionado · clique nas fichas da equipe ou nos seletores de Herói"; column.AddChild(_selectedName);
        var stats = new HBoxContainer(); column.AddChild(stats);
        string[] names = { "Vida", "Ataque", "Defesa" };
        for (var i = 0; i < 3; i++)
        {
            var plate = new BrassStatSocket { Kind = i, Name = "StatSocket" + i, SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsStretchRatio = 1 };
            stats.AddChild(plate);
            var box = Column(0); plate.AddChild(box);
            var legend = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center }; legend.AddThemeConstantOverride("separation", S(4)); box.AddChild(legend);
            var icon = new TextureRect { Texture = BrassUi.Icon(i == 0 ? "vital" : i == 1 ? "rifle" : "shield"), Modulate = BrassUi.Soot,
                CustomMinimumSize = new Vector2(S(14), S(14)), ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered, MouseFilter = MouseFilterEnum.Ignore };
            legend.AddChild(icon);
            var label = BrassUi.Text(names[i], 13, BrassUi.Soot); label.HorizontalAlignment = HorizontalAlignment.Center; legend.AddChild(label);
            _stats[i] = BrassUi.Emphasis("", 28, BrassUi.Soot);
            _stats[i].HorizontalAlignment = HorizontalAlignment.Center; box.AddChild(_stats[i]);
            if (i == 0)
            {
                _maxHealth = BrassUi.Text("", 13, BrassUi.Soot);
                _maxHealth.HorizontalAlignment = HorizontalAlignment.Center; box.AddChild(_maxHealth);
            }
        }
        column.AddChild(Heading("Equipe", 16));
        for (var i = 0; i < 3; i++)
        {
            int member = i;
            var healthRow = new BrassPartyRow { Name = "SelectStatusMember" + i, MemberIndex = i, FocusMode = FocusModeEnum.All };
            healthRow.Pressed += () => SelectMember(member);
            _statusMembers[i] = healthRow; column.AddChild(healthRow);
        }
        column.AddChild(Heading("Apoio automático", 16));
        _healState = BrassUi.Wrap("", 14, BrassUi.Paper); column.AddChild(_healState);
        column.AddChild(BrassUi.Wrap("Aliados vivos atacam automaticamente. O Guarda recebe a maioria dos golpes comuns.", 14, BrassUi.Paper));
        var reduce = new CheckButton { Name = "ReduceUiMotion", Text = "Reduzir movimento da UI", ButtonPressed = BrassMotion.Reduced, FocusMode = FocusModeEnum.All };
        reduce.TooltipText = "Remove as transições decorativas de foco/clique e o rastro de vida. Estados, valores e foco continuam imediatos. Não altera combate, animação 3D ou áudio.";
        reduce.Toggled += value => BrassMotion.Reduced = value; column.AddChild(reduce);
        var protection = new PanelContainer();
        protection.AddThemeStyleboxOverride("panel", BrassUi.Box(BrassUi.Soot, BrassUi.Soot, 0, 6, 6)); footer.AddChild(protection);
        var protectionColumn = Column(2); protection.AddChild(protectionColumn);
        _shield = BrassUi.Button("Escudo de Pressão", ActivateShield); _shield.Name = "PressureShield";
        _shield.TooltipText = "P · Protege o Guarda durante o combate, reduzindo o dano recebido. Não concede imunidade. A pausa congela a duração e a recarga.";
        SetIcon(_shield, "shield"); BrassUi.Primary(_shield); protectionColumn.AddChild(_shield);
        _shieldState = BrassUi.Wrap("", 14); protectionColumn.AddChild(_shieldState);
        AddActions(footer, false);
    }

    private void BuildHero(VBoxContainer column, VBoxContainer footer)
    {
        var source = GD.Load<Texture2D>("res://Assets/Brass/Portraits/equipe-v12.png");
        _portraits[0] = new AtlasTexture { Atlas = source, Region = new Rect2(555, 0, 565, 941), FilterClip = true };
        _portraits[1] = new AtlasTexture { Atlas = source, Region = new Rect2(0, 0, 555, 941), FilterClip = true };
        _portraits[2] = new AtlasTexture { Atlas = source, Region = new Rect2(1120, 0, 552, 941), FilterClip = true };
        var frame = new BrassPortraitFrame { CustomMinimumSize = new Vector2(0, S(220)) };
        column.AddChild(frame);
        _portrait = new TextureRect { ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered, MouseFilter = MouseFilterEnum.Ignore };
        frame.AddChild(_portrait);
        _heroName = Heading("", 20); _heroName.HorizontalAlignment = HorizontalAlignment.Center; column.AddChild(_heroName);
        _heroRole = BrassUi.Wrap("", 14, BrassUi.Paper); _heroRole.HorizontalAlignment = HorizontalAlignment.Center; column.AddChild(_heroRole);
        column.AddChild(Heading("Bolsa", 16));
        column.AddChild(BrassUi.Wrap("21 espaços indisponíveis. Equipamento fora desta fatia.", 14, BrassUi.Paper));
        _bag = new GridContainer { Name = "EquipmentBag", Columns = 7 }; column.AddChild(_bag);
        for (var i = 0; i < 21; i++)
        {
            var socket = new Button { Name = "UnavailableSlot" + i, Disabled = true, CustomMinimumSize = new Vector2(0, S(28)),
                SizeFlagsHorizontal = SizeFlags.ExpandFill, TooltipText = "Indisponível · equipamento fora desta fatia", FocusMode = FocusModeEnum.None };
            // Empty sockets must not inherit text-button padding: seven columns still
            // fit an embedded instrument at 150%, without shrinking any text.
            socket.AddThemeStyleboxOverride("disabled", BrassUi.Box(BrassUi.Soot, BrassUi.Tobacco, 0));
            _bag.AddChild(socket);
            var engraving = new BrassEmptySlot(); socket.AddChild(engraving); BrassUi.FullRect(engraving);
        }
        // Use actual available width (already in screen space), keeping all seven columns
        // square without scaling the geometry a second time or imposing a wide minimum.
        _bag.Resized += () =>
        {
            float side = Math.Max(1, (_bag.Size.X - 6 * _bag.GetThemeConstant("h_separation")) / 7);
            foreach (Button socket in _bag.GetChildren())
                if (Math.Abs(socket.CustomMinimumSize.Y - side) > 1) socket.CustomMinimumSize = new Vector2(0, side);
        };
        footer.AddChild(BrassUi.Wrap("Selecionar integrante · inspecionar no palco", 13, BrassUi.Paper));
        var selectors = new HBoxContainer(); footer.AddChild(selectors);
        var group = new ButtonGroup();
        int[] order = { 2, 1, 0 }; string[] labels = { "CURA", "DPS", "TANK" }, icons = { "heal", "rifle", "shield" };
        for (var i = 0; i < 3; i++)
        {
            int member = order[i];
            var button = BrassUi.Button(labels[i], () => SelectMember(member));
            button.Name = "SelectMember" + member;
            button.ToggleMode = true; button.ButtonGroup = group; button.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            button.TooltipText = Simulation.Party[member].Name + " · atualizar retrato, Status e identificação no palco";
            SetIcon(button, icons[i]); BrassUi.Quiet(button);
            button.IconAlignment = HorizontalAlignment.Center;
            button.VerticalIconAlignment = VerticalAlignment.Top;
            button.CustomMinimumSize = new Vector2(0, S(64));
            _selectors[member] = button; selectors.AddChild(button);
        }
    }

    private void BuildRoute(VBoxContainer column, VBoxContainer footer)
    {
        column.AddChild(Heading("Galerias da Caldeira", 18));
        _route = new ExpeditionMenuRoute { Name = "FiveStageRoute" }; column.AddChild(_route);
        var ticket = new PanelContainer(); ticket.AddThemeStyleboxOverride("panel", BrassUi.Box(BrassUi.Linen, BrassUi.Brass, 0, 10, 8)); column.AddChild(ticket);
        var ticketColumn = Column(4); ticket.AddChild(ticketColumn);
        _ticket = Heading("", 20, BrassUi.Soot); ticketColumn.AddChild(_ticket);
        _phase = BrassUi.Wrap("", 14, BrassUi.Soot); ticketColumn.AddChild(_phase);
        column.AddChild(Heading("Últimos acontecimentos", 16));
        _journal = BrassUi.Wrap("A equipe inicia a marcha.", 14, BrassUi.Linen); column.AddChild(_journal);
        AddActions(footer, true);
    }

    private void AddActions(VBoxContainer column, bool primary)
    {
        var row = new HBoxContainer(); column.AddChild(row);
        var pause = BrassUi.Button(primary ? "Pausar expedição" : "Pausar", ContinueOrPause);
        pause.Name = primary ? "RoutePause" : "StatusPause";
        SetIcon(pause, "pause"); BrassUi.Quiet(pause);
        pause.SizeFlagsHorizontal = SizeFlags.ExpandFill; row.AddChild(pause); _pauseButtons.Add(pause);
        if (!primary)
        {
            var reset = BrassUi.Button("Reiniciar", ResetSimulation); reset.Name = "ResetRoute";
            reset.TooltipText = "Reiniciar rota · R"; BrassUi.Quiet(reset); row.AddChild(reset);
        }
        // Recuar is physically separated from the frequent pause action.
        var retreat = BrassUi.Button("Recuar", () => { Simulation.Retreat(); RefreshState(); });
        retreat.Name = primary ? "RouteRetreat" : "StatusRetreat";
        retreat.TooltipText = "Encerrar esta tentativa e retirar a equipe da rota.";
        SetIcon(retreat, "return"); BrassUi.Quiet(retreat);
        column.AddChild(retreat); _retreatButtons.Add(retreat);
    }

    private static IEnumerable<Control> MenuFocusControls(Node parent)
    {
        foreach (var child in parent.GetChildren())
        {
            if (child is Control control && control.FocusMode == FocusModeEnum.All && (child is Button || child is ScrollContainer)) yield return control;
            foreach (var descendant in MenuFocusControls(child)) yield return descendant;
        }
    }

    private static void WireFocus(Window window)
    {
        var buttons = MenuFocusControls(window).ToArray();
        for (var i = 0; i < buttons.Length; i++)
        {
            buttons[i].FocusNext = buttons[(i + 1) % buttons.Length].GetPath();
            buttons[i].FocusPrevious = buttons[(i + buttons.Length - 1) % buttons.Length].GetPath();
        }
    }

    public override void _Process(double delta)
    {
        Simulation.Advance(delta);
        _refreshIn -= delta;
        if (_refreshIn > 0) return;
        _refreshIn = .1;
        RefreshState();
    }

    private bool Terminal => Simulation.Phase is ExpeditionPhase.Complete or ExpeditionPhase.Defeat or ExpeditionPhase.Retreated;

    private void SelectMember(int index)
    {
        _selected = index;
        Stage.SelectedMemberIndex = index;
        for (var i = 0; i < 3; i++) _selectors[i].SetPressedNoSignal(i == index);
        _portrait.Texture = _portraits[index];
        RefreshState();
    }

    private void ActivateShield() { Simulation.ActivateShield(); RefreshState(); }
    private void ContinueOrPause()
    {
        if (Terminal) ResetSimulation();
        else { Simulation.Paused = !Simulation.Paused; RefreshState(); }
    }
    private void ResetSimulation()
    {
        Simulation.Reset(); _snapVitals = true; _lastEventId = -1; _recent.Clear(); RefreshState();
    }

    private static void SetText(Label label, string text) { if (label.Text != text) label.Text = text; }
    private void RefreshState()
    {
        if (!_ready) return;
        var member = Simulation.Party[_selected];
        SetText(_selectedName, member.Name);
        SetText(_stats[0], Math.Ceiling(member.Health).ToString("0"));
        SetText(_maxHealth, $"/ {member.MaxHealth:0}");
        SetText(_stats[1], member.Attack.ToString("0.#"));
        SetText(_stats[2], member.Defense.ToString("0.#"));
        for (var i = 0; i < 3; i++)
        {
            var ally = Simulation.Party[i];
            _statusMembers[i].UpdateMember(ally.Name, ally.Health, ally.MaxHealth, i == _selected, Simulation.Paused, _snapVitals);
        }
        _snapVitals = false;
        SetText(_heroName, member.Name);
        SetText(_heroRole, _selected == 0 ? "TANK · prioridade de proteção" : _selected == 1 ? "DPS · ataques automáticos" : "CURA · apoio automático condicional");
        _shield.Disabled = Simulation.Phase != ExpeditionPhase.Combat || Simulation.Paused || Simulation.Party[0].Health <= 0 || Simulation.ShieldCooldown > 0;
        SetText(_shieldState, Terminal ? "Indisponível · encerrada" : Simulation.Party[0].Health <= 0 ? "Indisponível · Guarda caído"
            : Simulation.ShieldRemaining > 0 ? $"Protegido · {Simulation.ShieldRemaining:0.0} s"
            : Simulation.ShieldCooldown > 0 ? $"Recarga · {Simulation.ShieldCooldown:0.0} s"
            : Simulation.Paused ? "Pronto · retome para ativar"
            : Simulation.Phase == ExpeditionPhase.Combat ? "Pronto · ativar com P ou clique" : "Pronto · aguarda combate");
        SetText(_healState, Terminal ? "Cura suspensa · tentativa encerrada." : Simulation.Party[2].Health <= 0 ? "Cura indisponível: Alquimista caído." : Simulation.HealCooldown > 0 ? $"Infusão automática · recarga {Simulation.HealCooldown:0.0} s" : "Infusão automática: cura feridos com o Alquimista vivo.");
        var phase = Simulation.Phase switch
        {
            ExpeditionPhase.March => "Em marcha", ExpeditionPhase.Combat => "Em combate",
            ExpeditionPhase.Loot => "Encontro vencido · descanso", ExpeditionPhase.Complete => "Rota concluída",
            ExpeditionPhase.Defeat => "Equipe derrotada", ExpeditionPhase.Retreated => "Recuo concluído", _ => ""
        };
        if (Simulation.Paused && !Terminal) phase += " · PAUSA";
        SetText(_ticket, Simulation.StageName);
        SetText(_phase, $"TRECHO {Simulation.StageIndex+1:00} / 05 · {phase}\n{Simulation.Victories} / 5 encontros vencidos");
        _route.UpdateRoute(Simulation);
        SetText(_stageTitle, $"GALERIAS DA CALDEIRA   /   {Simulation.StageName.ToUpperInvariant()}   ·   {phase.ToUpperInvariant()}");
        SetText(_stageEvent, Simulation.LastEvent);
        _pauseButtons[0].Text = Terminal ? "Encerrada" : Simulation.Paused ? "Continuar" : "Pausar";
        _pauseButtons[0].Disabled = Terminal;
        _pauseButtons[1].Text = Terminal ? "Reiniciar expedição" : Simulation.Paused ? "Continuar expedição" : "Pausar expedição";
        foreach (var button in _retreatButtons) button.Disabled = Terminal;
        foreach (var e in Simulation.Events)
        {
            if (e.Id <= _lastEventId) continue;
            _lastEventId = e.Id;
            // Combat impacts remain visible on stage; keep the journal readable instead of flooding attacks.
            if (e.Kind is not ("phase" or "heal" or "shield" or "kill")) continue;
            var stamp = TimeSpan.FromSeconds(e.At);
            _recent.Add($"{(int)stamp.TotalMinutes:00}:{stamp.Seconds:00}  {e.Message}");
            if (_recent.Count > 3) _recent.RemoveAt(0);
        }
        SetText(_journal, _recent.Count == 0 ? "A equipe inicia a marcha." : string.Join("\n", _recent));
    }

    private void SetMenuVisible(int index, bool visible, bool focus = true)
    {
        var window = _menus[index];
        if (window.Visible == visible) return;
        if (visible && focus)
            _returnFocus[index] = GetViewport().GuiGetFocusOwner() ?? _launchers[index];
        window.Visible = visible; _openOrder.Remove(index);
        if (visible)
        {
            _openOrder.Add(index);
            window.Position = ClampPosition(window.Position, window.Size, MenuBounds());
            if (focus && !_headless)
            {
                // Explicit user-open only. Dock movement never raises this window or the stage.
                window.GrabFocus();
                var initial = index == 1 ? _selectors[_selected] : index == 0 && !_shield.Disabled ? _shield : _pauseButtons[index == 0 ? 0 : 1];
                if (initial.Disabled) initial = _closeButtons[index];
                initial.GrabFocus();
            }
        }
        else if (focus && !_headless)
        {
            var previous = _returnFocus[index];
            if (previous == null || !GodotObject.IsInstanceValid(previous) || !previous.IsInsideTree() || !previous.IsVisibleInTree())
                previous = _launchers[index];
            if (previous.GetViewport() is Window owner) owner.GrabFocus();
            previous.GrabFocus();
            _returnFocus[index] = null;
        }
        _launchers[index].SetPressedNoSignal(visible);
    }

    // Capture shortcuts before focused Buttons consume Space as ui_accept.
    public override void _Input(InputEvent input) => HandleKey(input, GetViewport(), -1);
    private void HandleKey(InputEvent input, Viewport viewport, int menu)
    {
        if (input is not InputEventKey key || !key.Pressed || key.Echo || key.CtrlPressed || key.AltPressed || key.MetaPressed) return;
        switch (key.Keycode)
        {
            case Key.Space: ContinueOrPause(); break;
            case Key.P: ActivateShield(); break;
            case Key.R: ResetSimulation(); break;
            case Key.Escape:
                int close = menu >= 0 ? menu : _openOrder.Count > 0 ? _openOrder[^1] : -1;
                if (close < 0) return; // Root quit belongs to Main.
                SetMenuVisible(close, false); break;
            default: return;
        }
        viewport.SetInputAsHandled();
    }

    public void ToggleDock()
    {
        if (!_ready || Overview || _headless) return;
        _docked = !_docked; ConfigureRoot(false);
    }

    private void ConfigureRoot(bool initial)
    {
        var window = GetWindow();
        var usable = DisplayServer.ScreenGetUsableRect(window.CurrentScreen);
        if (usable.Size.X <= 0 || usable.Size.Y <= 0) return;
        window.Mode = Window.ModeEnum.Windowed;
        window.Borderless = !Overview && _docked;
        window.AlwaysOnTop = !Overview && _docked;
        foreach (var menu in _menus) if (menu != null) menu.AlwaysOnTop = !Overview && _docked;
        // Keep keyboard input available. Never use Unfocusable: native menus must retain input.
        window.Unfocusable = false;
        window.Size = new Vector2I(Math.Min(Overview ? 1160 : 1000, usable.Size.X-16), Math.Min(Overview ? 880 : _docked ? 320 : 430, usable.Size.Y-32));
        window.Position = Overview || !_docked
            ? usable.Position + (usable.Size-window.Size)/2
            : new Vector2I(usable.Position.X+(usable.Size.X-window.Size.X)/2, usable.End.Y-window.Size.Y-6);
        if (initial) ArrangeMenus();
    }

    private Rect2I MenuBounds()
    {
        if (Overview || _headless) return new Rect2I(Vector2I.Zero, GetWindow().Size);
        return DisplayServer.ScreenGetUsableRect(GetWindow().CurrentScreen);
    }
    private static Vector2I ClampPosition(Vector2I position, Vector2I size, Rect2I bounds) => new(
        Math.Clamp(position.X, bounds.Position.X, Math.Max(bounds.Position.X, bounds.End.X-size.X)),
        Math.Clamp(position.Y, bounds.Position.Y, Math.Max(bounds.Position.Y, bounds.End.Y-size.Y)));

    private void ArrangeMenus()
    {
        if (!_ready) return;
        var bounds = MenuBounds();
        if (bounds.Size.X <= 0 || bounds.Size.Y <= 0) return;
        if (Overview || _headless)
        {
            if (Overview)
            {
                float required = 0;
                for (var i = 0; i < 3; i++)
                    required = Math.Max(required, _menuHeaders[i].GetCombinedMinimumSize().Y + _menuFooters[i].GetCombinedMinimumSize().Y);
                required += S(27) + S(16) + S(40); // chassis margins, shell gaps, a readable body slice
                // Estimate the regular composition even if its secondary copy is hidden.
                // This avoids a resize/hide/show oscillation as the menu space grows.
                float regularStage = _stagePanel.GetCombinedMinimumSize().Y + 180 - Stage.CustomMinimumSize.Y;
                float regularSpace = bounds.Size.Y - (2 * S(14) + _toolbar.Size.Y + _overviewCaption.GetCombinedMinimumSize().Y
                    + regularStage + _rootFooter.GetCombinedMinimumSize().Y + 4 * S(8));
                bool tight = regularSpace < required;
                _overviewCaption.Visible = !tight;
                _rootFooter.Visible = !tight;
                Stage.CustomMinimumSize = new Vector2(0, tight ? 100 : 180);
            }
            int width = Math.Max(300, Math.Min(350, (bounds.Size.X-64)/3));
            // Overview's reserved space is the actual gap between toolbar and stage,
            // rather than a guessed y/height which fails when the toolbar wraps.
            int top = Overview ? (int)_menuSpace.GlobalPosition.Y : 0;
            int available = Overview ? (int)_menuSpace.Size.Y : bounds.Size.Y - top;
            int height = Math.Max(280, Math.Min(S(620), available));
            int gap = Math.Max(8, (bounds.Size.X-3*width)/4);
            for (var i = 0; i < 3; i++)
            {
                // Embedded review keeps its three-column width at all text scales.
                // Lowering the *window* minimum never scales down its fonts.
                _menus[i].MinSize = new Vector2I(300, Math.Min(S(280), height));
                _menus[i].Size = new Vector2I(width, height);
                _menus[i].Position = ClampPosition(new Vector2I(gap+i*(width+gap), top), _menus[i].Size, bounds);
            }
            _dockButton.Disabled = true; _dockButton.TooltipText = "Overview · instrumentos embutidos";
        }
        else ArrangeNativeMenus(bounds);
    }

    private void ArrangeNativeMenus(Rect2I bounds)
    {
        // Share usable width before overlapping windows. Excess body content scrolls.
        int width = Math.Min(Math.Min(S(380), Math.Max(350, (bounds.Size.X-24)/3)), bounds.Size.X);
        int height = Math.Min(S(620), bounds.Size.Y-40);
        int required = 0;
        for (int i = 0; i < 3; i++)
            required = Math.Max(required, (int)Math.Ceiling(_menuHeaders[i].GetCombinedMinimumSize().Y + _menuFooters[i].GetCombinedMinimumSize().Y) + S(83));
        // Reserve the dock/toolbar when the desktop can accommodate it. Otherwise
        // windows remain independent/overlapping, with their fixed close/actions accessible.
        int dockHeight = !Overview && !_headless ? GetWindow().Size.Y : 320;
        int aboveDock = bounds.Size.Y - dockHeight - 48;
        if (_docked && aboveDock >= required) height = Math.Min(height, aboveDock);
        int total = 3*width+24;
        for (var i = 0; i < 3; i++)
        {
            int readableHeight = (int)Math.Ceiling(_menuHeaders[i].GetCombinedMinimumSize().Y + _menuFooters[i].GetCombinedMinimumSize().Y) + S(83);
            _menus[i].MinSize = new Vector2I(Math.Min(width, Math.Max(300, S(250))), Math.Min(height, Math.Max(S(280), readableHeight)));
            _menus[i].Size = new Vector2I(width, height);
            _menus[i].Position = ClampPosition(new Vector2I(bounds.Position.X+(bounds.Size.X-total)/2+i*(width+12), bounds.Position.Y+24), _menus[i].Size, bounds);
        }
    }

    /// <summary>Opt-in virtual usable-desktop fixture, NOT a physical DPI/monitor test.</summary>
    public async Task VerifyMenuBounds(Vector2I usableSize)
    {
        var bounds = new Rect2I(Vector2I.Zero, usableSize);
        ArrangeNativeMenus(bounds);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        for (int i = 0; i < 3; i++)
        {
            var local = new Rect2(Vector2.Zero, _menus[i].Size);
            if (!bounds.Encloses(new Rect2I(_menus[i].Position, _menus[i].Size)) ||
                !local.Encloses(_menuHeaders[i].GetGlobalRect()) || !local.Encloses(_menuFooters[i].GetGlobalRect()) ||
                _menuScrolls[i].Size.Y < S(40))
                throw new InvalidOperationException($"UI bounds {usableSize} scale {BrassUi.Scale}: instrument {i} clipped");
        }
        ArrangeMenus();
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    }

    /// <summary>Structural and layout checks. No screen capture, quitting, API or simulation mutation.</summary>
    public async Task VerifyLayouts()
    {
        if (!IsNodeReady()) await ToSignal(this, SignalName.Ready);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException("Expedition layout: " + message); }
        Require(Stage != null && Stage.IsInsideTree(), "palco não instanciado");
        Require(Simulation.Party.Count == 3, "equipe deve ter três integrantes");
        Require(_bag.Columns == 7 && _bag.GetChildCount() == 21, "bolsa deve ter sete colunas e três linhas");
        Require(_bag.GetChildren().All(child => child is Button button && button.Disabled), "slots devem estar indisponíveis");
        Require(_selectors.Length == 3 && _selectors.All(button => button != null && button.IsInsideTree()), "três seletores obrigatórios");
        Require(_selectors.Count(button => button.ButtonPressed) == 1 && _selectors[_selected].ButtonPressed, "seleção incoerente");
        Require(_route.StageCount == 5, "rota deve ter cinco etapas");
        Require(Stage!.SelectedMemberIndex == _selected, "seleção deve identificar o mesmo integrante no palco");
        Require(_detailsButton.ToggleMode && _detailsButton.ButtonPressed == Stage.DetailedHud, "toggle Detalhes incoerente com o palco");
        for (var i = 0; i < 3; i++)
            Require(!_menuScrolls[i].IsAncestorOf(_menuHeaders[i]) && !_menuScrolls[i].IsAncestorOf(_menuFooters[i]), "cabeçalho e ações devem ficar fora da rolagem");
        Require(_menus.All(window => window != null && window.IsInsideTree() && !window.Exclusive), "três janelas independentes obrigatórias");
        Require(_statusMembers.All(row => row != null && row.Meter.IsInsideTree()), "fichas/barras de vida ausentes");
        Require(_portrait.Texture is AtlasTexture atlas && atlas.Region.Size.Y == 941, "retrato deve usar a pintura v12");
        Require(_pauseButtons.Count == 2 && _retreatButtons.Count == 2 && _shield != null, "ações ausentes");
        Require(_menus.All(window => window.Size.X >= 300 && window.Size.Y >= 280), "instrumento abaixo do tamanho mínimo");
        if (!_headless)
        {
            var bounds = MenuBounds();
            Require(_menus.All(window => bounds.Encloses(new Rect2I(window.Position, window.Size))), "instrumento fora da área utilizável");
        }
        GD.Print("EXPEDITION LAYOUT OK: 3 windows, 3 selectors, 7x3 unavailable slots, 5 route nodes, v12 atlas portraits");
    }
}
