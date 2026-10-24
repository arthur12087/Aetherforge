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
    public bool HeroUsesSpread => _heroSpread.Columns == 2;
    public ExpeditionStatusPanel StatusPanel { get; private set; } = null!;
    public ExpeditionLootView LootPanel { get; private set; } = null!;

    private readonly Window[] _menus = new Window[3];
    private readonly Button[] _launchers = new Button[3], _selectors = new Button[3];
    private readonly BrassPartyRow[] _statusMembers = new BrassPartyRow[3];
    private bool _snapVitals = true;
    private readonly List<Button> _pauseButtons = new(), _retreatButtons = new();
    private readonly List<string> _recent = new();
    private GridContainer _bag = null!;
    private ExpeditionMenuRoute _route = null!;
    private Label _heroName = null!, _heroRole = null!, _shieldState = null!, _rewardNotice = null!, _routeRewards = null!,
        _phase = null!, _ticket = null!, _journal = null!, _stageTitle = null!, _stageEvent = null!;
    private Button _shield = null!, _dockButton = null!, _detailsButton = null!;
    private TextureRect _portrait = null!;
    private readonly AtlasTexture[] _portraits = new AtlasTexture[3], _faces = new AtlasTexture[3];
    private GridContainer _heroSpread = null!;
    private PanelContainer _heroFrame = null!;
    private VBoxContainer _heroDetails = null!;
    private Control _menuSpace = null!;
    private int _selected, _lastEventId = -1;
    private long _lastRewardId;
    private double _rewardUntil;
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
        InitializePortraits();
        BuildMenu(0, "STATUS", "01 / INTEGRANTE EM CAMPO", BuildStatus);
        BuildMenu(1, "HERÓI", "02 / ESTOJO", BuildHero);
        BuildMenu(2, "EXPEDIÇÃO", "03 / BILHETE DE ROTA", BuildRoute);
        InitializeEquipment();
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
        _rewardNotice=BrassUi.Text("",13,BrassUi.Ether); _rewardNotice.Name="VictoryRewardNotice";
        _rewardNotice.TextOverrunBehavior=TextServer.OverrunBehavior.TrimEllipsis; _rewardNotice.Visible=false; stageColumn.AddChild(_rewardNotice);
        var footer = new HFlowContainer(); _rootFooter = footer; column.AddChild(footer);
        footer.AddChild(BrassUi.Text("Espaço: ação / pausa   ·   P: escudo   ·   R: reiniciar", 13, Overview ? BrassUi.Tobacco : BrassUi.Paper));
        footer.AddChild(new Control { SizeFlagsHorizontal = SizeFlags.ExpandFill });
        footer.AddChild(BrassUi.Text("SESSÃO LOCAL", 13, Overview ? BrassUi.Tobacco : BrassUi.Ether));
    }

    private static VBoxContainer Column(int gap = 8)
    {
        var column = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        column.AddThemeConstantOverride("separation", S(gap));
        return column;
    }

    private static Label Heading(string text, int size = 18, Color? color = null)
    {
        var label = BrassUi.Emphasis(text, size, color ?? BrassUi.Soot);
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
            Theme = BrassUi.CreateDossierTheme(), WrapControls = false, GuiDisableInput = false, HandleInputLocally = true
        };
        _menus[index] = window; AddChild(window);
        window.CloseRequested += () => SetMenuVisible(index, false);
        window.AddChild(new ExpeditionMenuInput { Handle = input => HandleKey(input, window, index) });
        // Chassis fills the viewport. Its minimum comes only from fixed header/footer;
        // the ScrollContainer deliberately contributes no vertical content minimum.
        var chassis = new BrassDossier { Name = "InstrumentChassis" };
        window.AddChild(chassis); BrassUi.FullRect(chassis);
        var shell = Column(8); chassis.AddChild(shell);
        var top = Column(2); top.Name = "FixedHeader"; shell.AddChild(top);
        _menuHeaders[index] = top;
        top.AddChild(BrassUi.Text(subtitle, 13, BrassUi.Tobacco));
        var plate = new PanelContainer { Name = "DossierHeading", MouseFilter = MouseFilterEnum.Stop };
        var band = BrassUi.Box(index == 1 ? BrassUi.Linen : BrassUi.Wine, BrassUi.Tobacco, 0, 8, 0);
        if (index == 1) band.BorderWidthBottom = S(1);
        plate.AddThemeStyleboxOverride("panel", band); top.AddChild(plate);
        var header = new HBoxContainer { MouseFilter = MouseFilterEnum.Pass }; plate.AddChild(header);
        var heading = BrassUi.Text(index == 1 ? "Herói" : title, index == 1 ? 14 : 24, index == 1 ? BrassUi.Soot : BrassUi.Linen, index != 1);
        heading.SizeFlagsHorizontal = SizeFlags.ExpandFill; header.AddChild(heading);
        var close = BrassUi.Button("×", () => SetMenuVisible(index, false));
        close.Name = "CloseInstrument" + index; _closeButtons[index] = close;
        close.TooltipText = "Fechar menu · Escape"; BrassUi.DossierButton(close, index != 1); header.AddChild(close);
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
        var scroll = new ScrollContainer
        {
            Name = "ScrollableBody", HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
            VerticalScrollMode = ScrollContainer.ScrollMode.Auto, FollowFocus = true,
            FocusMode = FocusModeEnum.All,
            SizeFlagsVertical = SizeFlags.ExpandFill, SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        shell.AddChild(scroll); _menuScrolls[index] = scroll;
        scroll.CustomMinimumSize=new Vector2(0,S(40));
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
        footer.MinimumSizeChanged+=()=>Callable.From(ArrangeMenus).CallDeferred();
        top.MinimumSizeChanged+=()=>Callable.From(ArrangeMenus).CallDeferred();
        WireFocus(window);
    }

    private void InitializePortraits()
    {
        var source = GD.Load<Texture2D>("res://Assets/Brass/Portraits/equipe-v12.png");
        Rect2[] full = { new(555, 0, 565, 941), new(0, 0, 555, 941), new(1120, 0, 552, 941) };
        Rect2[] faces = { new(650, 0, 410, 455), new(130, 0, 350, 455), new(1195, 0, 350, 455) };
        for (int i = 0; i < 3; i++)
        {
            _portraits[i] = new AtlasTexture { Atlas = source, Region = full[i], FilterClip = true };
            _faces[i] = new AtlasTexture { Atlas = source, Region = faces[i], FilterClip = true };
        }
    }

    private void BuildStatus(VBoxContainer column, VBoxContainer footer)
    {
        StatusPanel = new ExpeditionStatusPanel();
        StatusPanel.Bind(Simulation, _faces, SelectMember);
        column.AddChild(StatusPanel);
        for (int i=0; i<3; i++) _statusMembers[i] = StatusPanel.MemberRows[i];
        StatusPanel.NavigationChanged += () => WireFocus(_menus[0]);
        var protection = new PanelContainer();
        protection.AddThemeStyleboxOverride("panel", BrassUi.Box(BrassUi.Linen, BrassUi.Paper, 0, 0, 2)); footer.AddChild(protection);
        var protectionColumn = Column(2); protection.AddChild(protectionColumn);
        var shortcuts=new HBoxContainer(); protectionColumn.AddChild(shortcuts);
        _shield = BrassUi.Button("Escudo · P", ActivateShield); _shield.Name = "PressureShield";
        _shield.TooltipText = "P · Protege o Guarda durante o combate, reduzindo o dano recebido. Não concede imunidade. A pausa congela a duração e a recarga.";
        SetIcon(_shield, "shield"); BrassUi.DossierButton(_shield, true); _shield.SizeFlagsHorizontal=SizeFlags.ExpandFill; shortcuts.AddChild(_shield);
        var routeShortcut=BrassUi.Button("Expedição",()=>SetMenuVisible(2,true)); routeShortcut.Name="StatusRouteActions";
        routeShortcut.TooltipText="Pausar, recuar ou reiniciar a rota"; BrassUi.DossierButton(routeShortcut); shortcuts.AddChild(routeShortcut);
        _shieldState = BrassUi.Wrap("", 14, BrassUi.Soot); protectionColumn.AddChild(_shieldState);
    }

    private void BuildHero(VBoxContainer column, VBoxContainer footer)
    {
        _heroSpread = new GridContainer { Name = "HeroDossierSpread", Columns = 1, SizeFlagsHorizontal = SizeFlags.ExpandFill };
        _heroSpread.AddThemeConstantOverride("h_separation", S(16)); column.AddChild(_heroSpread);
        var portraitColumn = Column(6); _heroSpread.AddChild(portraitColumn);
        var frame = new PanelContainer { Name = "HeroPortraitFrame", CustomMinimumSize = new Vector2(0, S(260)), MouseFilter = MouseFilterEnum.Ignore };
        frame.AddThemeStyleboxOverride("panel", BrassUi.Box(BrassUi.Paper, BrassUi.Paper, 0)); _heroFrame = frame;
        portraitColumn.AddChild(frame);
        portraitColumn.AddChild(BrassUi.Wrap("Retrato v12 · arte de referência", 13, BrassUi.Tobacco));
        var details = Column(8); details.Name = "HeroDossierDetails"; _heroDetails = details; _heroSpread.AddChild(details);
        void AdaptSpread()
        {
            bool sideBySide = column.Size.X >= S(480);
            _heroSpread.Columns = sideBySide ? 2 : 1;
            // Resize artwork, not text: preserve the whole figure where body height permits.
            float artHeight = sideBySide ? Math.Clamp(_menuScrolls[1].Size.Y-S(32), S(200), S(380)) : S(260);
            frame.CustomMinimumSize = new Vector2(sideBySide ? S(160) : 0, artHeight);
            details.CustomMinimumSize = new Vector2(sideBySide ? S(210) : 0, 0);
        }
        column.Resized += AdaptSpread;
        _menuScrolls[1].Resized += AdaptSpread;
        _portrait = new TextureRect { ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered, MouseFilter = MouseFilterEnum.Ignore };
        frame.AddChild(_portrait);
        _heroName = Heading("", 20); details.AddChild(_heroName);
        _heroRole = BrassUi.Wrap("", 14, BrassUi.Tobacco); details.AddChild(_heroRole);
        details.AddChild(new HSeparator());
        details.AddChild(Heading("Ganhos da sessão",16));
        LootPanel=new ExpeditionLootView(); LootPanel.Bind(Simulation); details.AddChild(LootPanel); _bag=LootPanel.Bag;
        LootPanel.InventoryChanged+=()=>WireFocus(_menus[1]);
        footer.AddChild(BrassUi.Wrap("Selecionar integrante · inspecionar no palco", 13, BrassUi.Tobacco));
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
            SetIcon(button, icons[i]); BrassUi.DossierButton(button);
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
        var ticket = new PanelContainer(); ticket.AddThemeStyleboxOverride("panel", BrassUi.Box(BrassUi.Linen, BrassUi.Tobacco, 0, 0, 8)); column.AddChild(ticket);
        var ticketColumn = Column(4); ticket.AddChild(ticketColumn);
        _ticket = Heading("", 20, BrassUi.Soot); ticketColumn.AddChild(_ticket);
        _phase = BrassUi.Wrap("", 14, BrassUi.Soot); ticketColumn.AddChild(_phase);
        column.AddChild(Heading("Últimos acontecimentos", 16));
        _journal = BrassUi.Wrap("A equipe inicia a marcha.", 14, BrassUi.Tobacco); column.AddChild(_journal);
        _routeRewards=BrassUi.Wrap("",14,BrassUi.Soot); _routeRewards.Name="RouteRewardSummary"; column.AddChild(_routeRewards);
        AddActions(footer, true);
    }

    private void AddActions(VBoxContainer column, bool primary)
    {
        var row = new HBoxContainer(); column.AddChild(row);
        var pause = BrassUi.Button("Pausar", ContinueOrPause);
        pause.Name = primary ? "RoutePause" : "StatusPause";
        SetIcon(pause, "pause"); BrassUi.DossierButton(pause, !primary);
        pause.SizeFlagsHorizontal = SizeFlags.ExpandFill; row.AddChild(pause); _pauseButtons.Add(pause);
        if (primary)
        {
            var reset = BrassUi.Button("Reiniciar", ResetSimulation); reset.Name = "ResetRoute";
            reset.TooltipText = "Nova tentativa · mantém nível, XP e ganhos desta sessão · R"; BrassUi.DossierButton(reset); row.AddChild(reset);
        }
        // Recuar is physically separated from the frequent pause action.
        var retreat = BrassUi.Button("Recuar", () => ExecuteRoute(RouteAction.Retreat));
        retreat.Name = primary ? "RouteRetreat" : "StatusRetreat";
        retreat.TooltipText = "Encerrar esta tentativa e retirar a equipe da rota.";
        SetIcon(retreat, "return"); BrassUi.DossierButton(retreat);
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
        var buttons = MenuFocusControls(window).Where(control => control.IsVisibleInTree() && !(control is BaseButton button && button.Disabled)).ToArray();
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
        EquipmentSelection.Invalidate();
        if (_equipmentPage>=0) _equipmentPages[_equipmentPage].Result=false;
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
        else if (Simulation.ReadView().Management != ExpeditionManagement.Active)
            ExecuteRoute(RouteAction.SetPaused, !Simulation.ReadView().Paused);
    }
    private void ResetSimulation()
    {
        ExecuteRoute(RouteAction.RestartRoute); _snapVitals = true; _lastEventId = -1; _recent.Clear();
        _lastRewardId=Simulation.Rewards.LastReward?.Sequence??0; _rewardNotice.Visible=false; _rewardUntil=0; RefreshState();
    }

    private static void SetText(Label label, string text) { if (label.Text != text) label.Text = text; }
    private void RefreshState(ExpeditionReadModel? readModel=null)
    {
        if (!_ready) return;
        var view=readModel??Simulation.ReadView();
        var member = view.Members[_selected];
        StatusPanel.UpdateState(_selected, _snapVitals,view);
        _snapVitals = false;
        SetText(_heroName, member.Name+$" · Nv. {member.Level}");
        LootPanel.UpdateState(view);
        RefreshEquipment(view);
        SetText(_heroRole, "Selecionado · "+(_selected == 0 ? "TANK · prioridade de proteção" : _selected == 1 ? "DPS · ataques automáticos" : "CURA · apoio automático condicional"));
        _shield.Disabled = Simulation.Phase != ExpeditionPhase.Combat || Simulation.Paused || Simulation.Party[0].Health <= 0 || Simulation.ShieldCooldown > 0;
        SetText(_shieldState, Terminal ? "Indisponível · encerrada" : Simulation.Party[0].Health <= 0 ? "Indisponível · Guarda caído"
            : Simulation.ShieldRemaining > 0 ? $"Guarda protegido · {Simulation.ShieldRemaining:0.0} s"
            : Simulation.ShieldCooldown > 0 ? $"Recarga · {Simulation.ShieldCooldown:0.0} s"
            : Simulation.Paused ? "Pronto · retome para ativar"
            : Simulation.Phase == ExpeditionPhase.Combat ? "Pronto · ativar com P ou clique" : "Pronto · aguarda combate");
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
        _pauseButtons[0].Text = Terminal ? "Reiniciar rota" : Simulation.Paused ? "Continuar" : "Pausar";
        foreach (var button in _retreatButtons) button.Disabled = Terminal;
        foreach (var e in Simulation.Events)
        {
            if (e.Id <= _lastEventId) continue;
            _lastEventId = e.Id;
            // Combat impacts remain visible on stage; keep the journal readable instead of flooding attacks.
            if (e.Kind is not ("phase" or "heal" or "shield" or "kill" or "reward" or "level" or "drop")) continue;
            var stamp = TimeSpan.FromSeconds(e.At);
            _recent.Add($"{(int)stamp.TotalMinutes:00}:{stamp.Seconds:00}  {e.Message}");
            if (_recent.Count > 3) _recent.RemoveAt(0);
        }
        SetText(_journal, _recent.Count == 0 ? "A equipe inicia a marcha." : string.Join("\n", _recent));
        var rewards=Simulation.Rewards;
        var latest=rewards.LastReward;
        if(latest!=null && latest.Sequence>_lastRewardId)
        {
            _lastRewardId=latest.Sequence; _rewardUntil=Simulation.Elapsed+4;
            string experience=latest.ExperienceGranted.All(x=>x==latest.ExperienceGranted[0])?$"+{latest.ExperienceGranted[0]} XP por integrante":"XP distribuída";
            string levels=latest.LevelsGained.Any(x=>x>0)?" · NÍVEL AUMENTOU":"";
            string item=latest.Drop!=null?" · "+latest.Drop.Name:"";
            SetText(_rewardNotice,$"VITÓRIA · {experience} · +{latest.Coins} moedas · +{latest.Scrap} sucatas{levels}{item}");
            _rewardNotice.TooltipText=_rewardNotice.Text;
        }
        _rewardNotice.Visible=latest!=null && Simulation.Elapsed<_rewardUntil;
        SetText(_routeRewards,$"Ganhos da sessão\n{rewards.Coins} moedas · {rewards.Scrap} sucatas · {rewards.Components} componentes\n{rewards.Inventory.Count} itens na bolsa"+(rewards.PendingDrops.Count>0?$" · {rewards.PendingDrops.Count} pendentes":""));
    }

    private void SetMenuVisible(int index, bool visible, bool focus = true, Control? opener=null)
    {
        var window = _menus[index];
        if (window.Visible == visible) return;
        if (visible && focus)
            _returnFocus[index] = opener ?? GetViewport().GuiGetFocusOwner() ?? _launchers[index];
        window.Visible = visible; _openOrder.Remove(index);
        if (!visible)
        {
            EquipmentSelection.Invalidate();
            if (_equipmentPage==index) ShowEquipmentPage(index,false);
            if (index==1) _bagReturn=-1;
        }
        if (visible)
        {
            _openOrder.Add(index);
            window.Position = ClampPosition(window.Position, window.Size, MenuBounds());
            if (index == 0) { StatusPanel.ResetNavigation(); _menuScrolls[0].ScrollVertical = 0; }
            WireFocus(window);
            if (focus)
            {
                // Explicit user-open only. Dock movement never raises this window or the stage.
                if (!_headless) window.GrabFocus();
                var initial = index == 1 ? _selectors[_selected] : index == 0 ? StatusPanel.InitialFocus : _pauseButtons[0];
                if (initial.Disabled) initial = _closeButtons[index];
                initial.GrabFocus();
            }
        }
        else if (focus)
        {
            var previous = _returnFocus[index];
            if (previous == null || !GodotObject.IsInstanceValid(previous) || !previous.IsInsideTree() || !previous.IsVisibleInTree() || previous is BaseButton { Disabled:true })
                previous = _launchers[index];
            if (!_headless && previous.GetViewport() is Window owner) owner.GrabFocus();
            previous.GrabFocus();
            _returnFocus[index] = null;
        }
        _launchers[index].SetPressedNoSignal(visible);
    }

    // Focused controls retain Space as ui_accept; otherwise Space pauses the expedition.
    public override void _Input(InputEvent input) => HandleKey(input, GetViewport(), -1);
    private void HandleKey(InputEvent input, Viewport viewport, int menu)
    {
        if (input is not InputEventKey key || !key.Pressed || key.Echo || key.CtrlPressed || key.AltPressed || key.MetaPressed) return;
        switch (key.Keycode)
        {
            case Key.Space:
                if (viewport.GuiGetFocusOwner() is BaseButton focused && focused.IsVisibleInTree() && !focused.Disabled) return;
                ContinueOrPause(); break;
            case Key.P: ActivateShield(); break;
            case Key.R: ResetSimulation(); break;
            case Key.Escape:
                int close = menu >= 0 ? menu : _openOrder.Count > 0 ? _openOrder[^1] : -1;
                if (close < 0) return; // Root quit belongs to Main.
                if (close < 2 && BackEquipment(close)) break;
                if (close == 0 && StatusPanel.TryGoBack()) break;
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
                required += S(27) + S(16) + S(180); // reserve identity and useful data, not just one line of body
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
            int budget = bounds.Size.X - 64;
            int statusWidth = Math.Max(300, (int)(budget * 380f/1366));
            int routeWidth = Math.Max(300, (int)(budget * 398f/1366));
            int[] widths = { statusWidth, Math.Max(300, budget-statusWidth-routeWidth), routeWidth };
            // Overview's reserved space is the actual gap between toolbar and stage,
            // rather than a guessed y/height which fails when the toolbar wraps.
            int top = Overview ? (int)_menuSpace.GlobalPosition.Y : 0;
            int available = Overview ? (int)_menuSpace.Size.Y : bounds.Size.Y - top;
            int fixedRequired=0;
            for(int i=0;i<3;i++) fixedRequired=Math.Max(fixedRequired,(int)Math.Ceiling(_menuHeaders[i].GetCombinedMinimumSize().Y+_menuFooters[i].GetCombinedMinimumSize().Y)+S(100));
            int height = Math.Min(bounds.Size.Y-top, Math.Max(fixedRequired,Math.Max(280, Math.Min(S(620), available))));
            int gap = Math.Max(8, (bounds.Size.X-widths.Sum())/4);
            int left = gap;
            for (var i = 0; i < 3; i++)
            {
                // Embedded review keeps its three-column width at all text scales.
                // Lowering the *window* minimum never scales down its fonts.
                _menus[i].MinSize = new Vector2I(300, Math.Min(S(280), height));
                _menus[i].Size = new Vector2I(widths[i], height);
                _menus[i].Position = ClampPosition(new Vector2I(left, top), _menus[i].Size, bounds);
                left += widths[i]+gap;
            }
            _dockButton.Disabled = true; _dockButton.TooltipText = "Overview · instrumentos embutidos";
        }
        else ArrangeNativeMenus(bounds);
    }

    private void ArrangeNativeMenus(Rect2I bounds)
    {
        // Share usable width before overlapping windows. Excess body content scrolls.
        int[] preferred = { S(380), S(588), S(398) }, minimum = { 350, 450, 350 };
        float fit = Math.Min(1, (bounds.Size.X-24f)/preferred.Sum());
        int[] widths = preferred.Select((w, i) => Math.Min(bounds.Size.X, Math.Max(minimum[i], (int)(w*fit)))).ToArray();
        int height = Math.Min(S(620), bounds.Size.Y-40);
        int required = 0;
        for (int i = 0; i < 3; i++)
            required = Math.Max(required, (int)Math.Ceiling(_menuHeaders[i].GetCombinedMinimumSize().Y + _menuFooters[i].GetCombinedMinimumSize().Y) + S(83));
        // Reserve the dock/toolbar when the desktop can accommodate it. Otherwise
        // windows remain independent/overlapping, with their fixed close/actions accessible.
        int dockHeight = !Overview && !_headless ? GetWindow().Size.Y : 320;
        int aboveDock = bounds.Size.Y - dockHeight - 48;
        if (_docked && aboveDock >= required) height = Math.Min(height, aboveDock);
        int total = widths.Sum()+24;
        int left = bounds.Position.X+(bounds.Size.X-total)/2;
        for (var i = 0; i < 3; i++)
        {
            int readableHeight = (int)Math.Ceiling(_menuHeaders[i].GetCombinedMinimumSize().Y + _menuFooters[i].GetCombinedMinimumSize().Y) + S(83);
            _menus[i].MinSize = new Vector2I(Math.Min(widths[i], Math.Max(300, S(250))), Math.Min(height, Math.Max(S(280), readableHeight)));
            _menus[i].Size = new Vector2I(widths[i], height);
            _menus[i].Position = ClampPosition(new Vector2I(left, bounds.Position.Y+24), _menus[i].Size, bounds);
            left += widths[i]+12;
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
        var statusBody = _menuScrolls[0].GetGlobalRect();
        float statusRight = statusBody.End.X-(_menuScrolls[0].GetVScrollBar().Visible ? _menuScrolls[0].GetVScrollBar().Size.X : 0);
        if (_statusMembers.Any(row => row.GetGlobalRect().Position.X < statusBody.Position.X-.5f || row.GetGlobalRect().End.X > statusRight+.5f))
            throw new InvalidOperationException($"UI bounds {usableSize} scale {BrassUi.Scale}: Status team clipped horizontally");
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
        Require(_bag.Columns is >=1 and <=7 && _bag.GetChildCount() == 21, "bolsa deve preservar 21 posições e refluir em área curta");
        Require(_bag.GetChildren().Select((child,index)=>child is Button button && button.Disabled==(index>=Simulation.Rewards.Inventory.Count)).All(valid=>valid), "slots devem corresponder aos itens da bolsa");
        Require(_selectors.Length == 3 && _selectors.All(button => button != null && button.IsInsideTree()), "três seletores obrigatórios");
        Require(_selectors.Count(button => button.ButtonPressed) == 1 && _selectors[_selected].ButtonPressed, "seleção incoerente");
        Require(_route.StageCount == 5, "rota deve ter cinco etapas");
        Require(Stage!.SelectedMemberIndex == _selected, "seleção deve identificar o mesmo integrante no palco");
        Require(_detailsButton.ToggleMode && _detailsButton.ButtonPressed == Stage.DetailedHud, "toggle Detalhes incoerente com o palco");
        for (var i = 0; i < 3; i++)
            Require(!_menuScrolls[i].IsAncestorOf(_menuHeaders[i]) && !_menuScrolls[i].IsAncestorOf(_menuFooters[i]), "cabeçalho e ações devem ficar fora da rolagem");
        Require(_menus.All(window => window != null && window.IsInsideTree() && !window.Exclusive), "três janelas independentes obrigatórias");
        Require(_statusMembers.All(row => row != null && row.Meter.IsInsideTree() && row.PortraitView.Texture is AtlasTexture), "fichas/retratos/barras de vida ausentes");
        Require(_heroFrame.IsInsideTree() && _heroDetails.IsInsideTree(), "dossiê do Herói ausente");
        Require(_heroSpread.Columns is 1 or 2, "dossiê deve empilhar ou mostrar duas colunas");
        Require(_portrait.Texture is AtlasTexture atlas && atlas.Region.Size.Y == 941, "retrato deve usar a pintura v12");
        Require(_pauseButtons.Count == 1 && _retreatButtons.Count == 1 && _shield != null, "ações de rota e escudo ausentes");
        Require(_menus.All(window => window.Size.X >= 300 && window.Size.Y >= 280), "instrumento abaixo do tamanho mínimo");
        if (!_headless)
        {
            var bounds = MenuBounds();
            Require(_menus.All(window => bounds.Encloses(new Rect2I(window.Position, window.Size))), "instrumento fora da área utilizável");
        }
        GD.Print("EXPEDITION LAYOUT OK: 3 windows, 3 selectors, 7x3 session bag, 5 route nodes, v12 atlas portraits");
    }
}
