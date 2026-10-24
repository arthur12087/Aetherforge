using Aetherforge.Domain;
using Godot;
using System;
using System.Collections.Generic;

/// <summary>Character context, summary and progressive detail. Receives updates from the view; never advances combat.</summary>
public partial class ExpeditionStatusPanel : VBoxContainer
{
    public BrassPartyRow[] MemberRows { get; } = new BrassPartyRow[3];
    public bool DetailsVisible => _details.Visible;
    public string SelectedAttribute { get; private set; } = "";
    public int SelectedMember { get; private set; }
    public Button InitialFocus => _previous;
    public event Action? NavigationChanged;
    private ExpeditionSimulation _simulation = null!;
    private Texture2D[] _faces = null!;
    private Action<int> _selectMember = null!;
    private Label _name = null!, _role = null!, _condition = null!, _healthText = null!, _experienceText = null!;
    private ProgressBar _experience = null!;
    private Label _explanationTitle = null!, _explanationText = null!;
    private TextureRect _portrait = null!;
    private BrassVitalMeter _health = null!;
    private VBoxContainer _summary = null!, _details = null!;
    private PanelContainer _explanation = null!;
    private Button _openDetails = null!, _back = null!, _previous = null!, _backAttribute = null!;
    private readonly Dictionary<string, Label> _values = new();
    private readonly Dictionary<string, Button> _detailButtons = new();
    private string _lastExplanation = "";
    private static int S(float value) => BrassUi.S(value);

    public void Bind(ExpeditionSimulation simulation, Texture2D[] faces, Action<int> selectMember)
    {
        _simulation = simulation; _faces = faces; _selectMember = selectMember;
    }

    public override void _Ready()
    {
        Name = "CharacterStatus";
        SizeFlagsHorizontal = SizeFlags.ExpandFill;
        AddThemeConstantOverride("separation", S(6));
        var identity = new HBoxContainer(); identity.AddThemeConstantOverride("separation", S(10)); AddChild(identity);
        _portrait = new TextureRect { Name = "StatusPortrait", CustomMinimumSize = new Vector2(S(62), S(72)),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            MouseFilter = MouseFilterEnum.Ignore };
        identity.AddChild(_portrait);
        var heading = Stack(1); identity.AddChild(heading);
        _name = BrassUi.Emphasis("", 19, BrassUi.Soot); _name.Name = "StatusMemberName";
        _name.AutowrapMode = TextServer.AutowrapMode.WordSmart; heading.AddChild(_name);
        _role = BrassUi.Text("", 13, BrassUi.Tobacco); heading.AddChild(_role);
        _condition = BrassUi.Text("", 13, BrassUi.Soot); _condition.Name = "StatusMemberCondition"; heading.AddChild(_condition);
        _experienceText=BrassUi.Wrap("", 13, BrassUi.Tobacco); _experienceText.Name="StatusExperienceText"; heading.AddChild(_experienceText);
        var arrows = Stack(2); arrows.SizeFlagsHorizontal = SizeFlags.ShrinkEnd; identity.AddChild(arrows);
        var previous = Action("‹", "StatusPreviousMember", () => _selectMember((SelectedMember+2)%3));
        _previous = previous;
        previous.TooltipText = "Integrante anterior"; arrows.AddChild(previous);
        var next = Action("›", "StatusNextMember", () => _selectMember((SelectedMember+1)%3));
        next.TooltipText = "Próximo integrante"; arrows.AddChild(next);
        foreach (var arrow in new[] { previous, next })
        {
            arrow.CustomMinimumSize = new Vector2(S(28), S(30));
            foreach (var state in new[] { "normal", "hover", "pressed", "hover_pressed" })
                arrow.AddThemeStyleboxOverride(state, BrassUi.Box(state=="normal" ? BrassUi.Linen : BrassUi.Paper, BrassUi.Paper, 0, 4, 2));
        }
        _experience=new ProgressBar { Name="StatusExperienceBar", ShowPercentage=false, CustomMinimumSize=new Vector2(0,S(6)), MouseFilter=MouseFilterEnum.Ignore };
        _experience.AddThemeStyleboxOverride("background",BrassUi.Box(BrassUi.Paper,BrassUi.Tobacco,1));
        _experience.AddThemeStyleboxOverride("fill",BrassUi.Box(BrassUi.Brass,BrassUi.Brass,0)); AddChild(_experience);

        var healthLine = new HBoxContainer(); AddChild(healthLine);
        var lifeLabel = BrassUi.Emphasis("Vida", 14, BrassUi.Soot); lifeLabel.SizeFlagsHorizontal = SizeFlags.ExpandFill; healthLine.AddChild(lifeLabel);
        _healthText = BrassUi.Emphasis("", 16, BrassUi.Soot); _healthText.Name = "StatusHealthValue"; healthLine.AddChild(_healthText);
        _health = new BrassVitalMeter { Name = "StatusHealthMeter" }; AddChild(_health);
        _summary = Stack(0); _summary.Name = "StatusSummary"; AddChild(_summary);
        Metric(_summary, "summary-attack", "Ataque", () => OpenAttribute("attack"));
        Metric(_summary, "summary-defense", "Defesa", () => OpenAttribute("defense"));
        Metric(_summary, "summary-interval", "Intervalo de ataque", () => OpenAttribute("interval"));
        _openDetails = Action("Estatísticas detalhadas", "OpenStatusDetails", () => ShowDetails(true)); AddChild(_openDetails);

        _details = Stack(3); _details.Name = "StatusDetails"; _details.Visible = false; AddChild(_details);
        _back = Action("‹ Voltar ao resumo", "BackStatusSummary", () => ShowDetails(false)); _details.AddChild(_back);
        Section("Progressão");
        Metric(_details,"experience","Nível e experiência",()=>OpenAttribute("experience"));
        Section("Ataque");
        Metric(_details, "attack", "Ataque", () => OpenAttribute("attack"));
        Metric(_details, "interval", "Intervalo de ataque", () => OpenAttribute("interval"));
        Metric(_details, "dps", "DPS básico teórico", () => OpenAttribute("dps"));
        Section("Defesa");
        Metric(_details, "max-health", "Vida máxima", () => OpenAttribute("max-health"));
        Metric(_details, "defense", "Defesa", () => OpenAttribute("defense"));
        Metric(_details, "protection", "Escudo de Pressão", () => OpenAttribute("protection"));
        Section("Apoio");
        Metric(_details, "infusion", "Infusão Etérea", () => OpenAttribute("infusion"));
        Metric(_details, "rest", "Cura no descanso", () => OpenAttribute("rest"));

        _explanation = new PanelContainer { Name = "StatusAttributeExplanation", Visible = false };
        _explanation.AddThemeStyleboxOverride("panel", BrassUi.Box(BrassUi.Paper, BrassUi.Tobacco, 0, 10, 8));
        // Before the detail list so explanations remain near the identity on small windows.
        AddChild(_explanation); MoveChild(_explanation, GetChildren().IndexOf(_details));
        var explanation = Stack(4); _explanation.AddChild(explanation);
        _explanationTitle = BrassUi.Emphasis("", 16, BrassUi.Soot); _explanationTitle.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        explanation.AddChild(_explanationTitle);
        _explanationText = BrassUi.Wrap("", 14, BrassUi.Soot); explanation.AddChild(_explanationText);
        _backAttribute = Action("Voltar aos atributos", "BackStatusAttributes", ClearAttribute); explanation.AddChild(_backAttribute);

        AddChild(new HSeparator());
        AddChild(BrassUi.Emphasis("Equipe em campo", 15, BrassUi.Soot));
        var members = new HBoxContainer(); members.AddThemeConstantOverride("separation", S(4)); AddChild(members);
        for (int i=0; i<3; i++)
        {
            int member = i;
            var row = new BrassPartyRow { Name = "SelectStatusMember"+i, MemberIndex = i, Portrait = _faces[i], Compact = true, FocusMode = FocusModeEnum.All };
            row.Pressed += () => _selectMember(member); MemberRows[i] = row; members.AddChild(row);
        }
        var reduce = new CheckButton { Name = "ReduceUiMotion", Text = "Reduzir movimento da UI", ButtonPressed = BrassMotion.Reduced, FocusMode = FocusModeEnum.All };
        reduce.AddThemeColorOverride("font_color", BrassUi.Soot);
        reduce.TooltipText = "Remove transições decorativas e rastro de vida. Valores e foco continuam imediatos.";
        reduce.Toggled += value => BrassMotion.Reduced = value; AddChild(reduce);
    }

    private static VBoxContainer Stack(int gap)
    {
        var column = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        column.AddThemeConstantOverride("separation", S(gap)); return column;
    }
    private static Button Action(string text, string name, Action action)
    {
        var button = BrassUi.Button(text, action); button.Name = name;
        BrassUi.DossierButton(button); return button;
    }
    private void Section(string title)
    {
        var label = BrassUi.Emphasis(title.ToUpperInvariant(), 13, BrassUi.Tobacco);
        _details.AddChild(label);
    }
    private void Metric(VBoxContainer parent, string id, string title, Action selected)
    {
        var button = Action("", "StatusAttribute-"+id, selected);
        button.TooltipText = title+" · selecionar para explicar";
        button.CustomMinimumSize = new Vector2(0, S(28)); parent.AddChild(button);
        var resting = BrassUi.Box(Colors.Transparent, BrassUi.Paper, 0, 0, 0); resting.BorderWidthBottom = S(1);
        button.AddThemeStyleboxOverride("normal", resting);
        foreach (var state in new[] { "hover", "pressed", "hover_pressed" })
            button.AddThemeStyleboxOverride(state, BrassUi.Box(BrassUi.Paper, BrassUi.Tobacco, 0, 0, 0));
        var margin = BrassUi.Margin(8, 3); margin.MouseFilter = MouseFilterEnum.Ignore; button.AddChild(margin); BrassUi.FullRect(margin);
        var row = new HBoxContainer { MouseFilter = MouseFilterEnum.Ignore }; margin.AddChild(row);
        var label = BrassUi.Text(title, 14, BrassUi.Soot); label.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        label.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis; row.AddChild(label);
        var value = BrassUi.Emphasis("", 14, BrassUi.Soot); value.Name = "AttributeValue"; row.AddChild(value);
        _values[id] = value;
        if (!id.StartsWith("summary-")) { button.ToggleMode = true; _detailButtons[id] = button; }
    }

    public void UpdateState(int selected, bool snap, ExpeditionReadModel? view=null)
    {
        bool changed = SelectedMember != selected;
        SelectedMember = selected;
        view??=_simulation.ReadView();
        var member = view.Members[selected];
        var stats=member.Stats;
        Set(_name, member.Name);
        Set(_role, member.Role switch { PartyRole.Tank => "TANK · proteção", PartyRole.Dps => "DPS · ataque", _ => "CURA · apoio" });
        string condition = member.Health <= 0 ? "Caído" : _simulation.Paused ? "Em pausa" : _simulation.Phase switch
        { ExpeditionPhase.March => "Em marcha", ExpeditionPhase.Combat => "Em combate", ExpeditionPhase.Loot => "Em descanso", _ => "Expedição encerrada" };
        Set(_condition, "Selecionado · "+condition + $" · {selected+1} / 3");
        int required=member.Level==CharacterProgression.MaximumLevel?0:60+30*(member.Level-1);
        Set(_experienceText,required==0 ? $"NÍVEL {member.Level} · MÁXIMO" : $"NÍVEL {member.Level} · {member.Experience} / {required} XP");
        _experience.MaxValue=required>0?required:1;
        _experience.Value=required>0?member.Experience:1;
        _portrait.Texture = _faces[selected];
        Set(_healthText, $"{Math.Ceiling(member.Health):0} / {stats.MaxHealth:0} PV");
        _health.Paused = view.Paused || view.Management==ExpeditionManagement.Active; _health.SetHealth(member.Health, stats.MaxHealth, snap || changed);
        Value("attack", $"{stats.Attack:0.#}"); Value("defense", $"{stats.Defense:0.#}");
        Value("experience",$"Nv. {member.Level}");
        Value("interval", $"{_simulation.GetAttackInterval(selected):0.##} s");
        Value("dps", $"{stats.Attack/_simulation.GetAttackInterval(selected):0.##} / s");
        Value("max-health", $"{stats.MaxHealth:0} PV");
        Value("protection", selected==0 ? $"{(1-ExpeditionSimulation.ShieldDamageMultiplier)*100:0}%" : "Só o Guarda");
        Value("infusion", selected==2 ? $"até {stats.InfusionPotential:0} PV" : "Recebe apoio");
        Value("rest", $"até {stats.MaxHealth*ExpeditionSimulation.RestHealingFraction:0} PV");
        foreach (var pair in _detailButtons) pair.Value.SetPressedNoSignal(pair.Key == SelectedAttribute);
        if (SelectedAttribute.Length > 0) UpdateExplanation();
        for (int i=0; i<3; i++)
        {
            var ally = view.Members[i];
            MemberRows[i].UpdateMember(ally.Name, ally.Health, ally.Stats.MaxHealth, i==selected, view.Paused || view.Management==ExpeditionManagement.Active, snap);
        }
    }
    private static void Set(Label label, string text) { if (label.Text != text) label.Text = text; }
    private void Value(string id, string text)
    {
        Set(_values[id], text);
        if (_values.TryGetValue("summary-"+id, out var summary)) Set(summary, text);
    }
    private void ShowDetails(bool show, bool focus = true)
    {
        SelectedAttribute = ""; _explanation.Visible = false; _lastExplanation = "";
        _details.Visible = show; _summary.Visible = !show; _openDetails.Visible = !show;
        NavigationChanged?.Invoke();
        if (focus && IsVisibleInTree()) (show ? _back : _openDetails).GrabFocus();
    }
    private void OpenAttribute(string id)
    {
        _details.Visible = true; _summary.Visible = false; _openDetails.Visible = false;
        SelectedAttribute = id; _explanation.Visible = true; UpdateExplanation();
        foreach (var pair in _detailButtons) pair.Value.SetPressedNoSignal(pair.Key==id);
        NavigationChanged?.Invoke();
        _backAttribute.GrabFocus();
    }
    private void ClearAttribute()
    {
        var previous = SelectedAttribute; SelectedAttribute = ""; _explanation.Visible = false; _lastExplanation = "";
        foreach (var pair in _detailButtons) pair.Value.SetPressedNoSignal(false);
        NavigationChanged?.Invoke();
        if (_detailButtons.TryGetValue(previous, out var button)) button.GrabFocus();
    }
    public bool TryGoBack()
    {
        if (SelectedAttribute.Length>0) { ClearAttribute(); return true; }
        if (DetailsVisible) { ShowDetails(false); return true; }
        return false;
    }
    public void ResetNavigation() => ShowDetails(false, false);
    private void UpdateExplanation()
    {
        var member = _simulation.Party[SelectedMember];
        var stats=_simulation.ReadView().Members[SelectedMember].Stats;
        var (title, text) = SelectedAttribute switch
        {
            "experience" => ("Nível e experiência",$"Nível {member.Progression.Level} de {CharacterProgression.MaximumLevel}. Cada vitória concede XP aos três integrantes, inclusive caídos. Próximo nível: 60 + 30 × (nível − 1). XP excedente avança níveis; no nível 20 não acumula. Reiniciar a rota mantém o progresso desta sessão."),
            "attack" => ("Ataque", EquipmentPresentation.Composition(stats)+"\nAtaque menos defesa do alvo, com mínimo de 1 por golpe. Equipamento já incluído nos totais."),
            "defense" => ("Defesa", EquipmentPresentation.Composition(stats)+"\nSubtraída do ataque inimigo, mínimo de 1. Escudo reduz depois o dano do Guarda."),
            "interval" => ("Intervalo de ataque", $"Um ataque a cada {_simulation.GetAttackInterval(SelectedMember):0.##} segundos de combate. A abertura do encontro tem um atraso próprio. Integrantes caídos não atacam; a pausa congela o tempo."),
            "dps" => ("DPS básico teórico", $"{member.Attack:0.#} de ataque ÷ {_simulation.GetAttackInterval(SelectedMember):0.##} s = {member.Attack/_simulation.GetAttackInterval(SelectedMember):0.##} por segundo. Estimativa antes da defesa do alvo, sem atraso inicial, cura ou interrupções. Não é dano medido."),
            "max-health" => ("Vida máxima", EquipmentPresentation.Composition(stats)+"\nEquipar não cura; reduzir máximo limita HP absoluto. Subir de nível preserva vida faltante dos vivos; não ressuscita."),
            "protection" => ("Escudo de Pressão", $"Protege somente o Guarda vivo. Reduz {(1-ExpeditionSimulation.ShieldDamageMultiplier)*100:0}% do dano após a defesa durante {ExpeditionSimulation.ShieldDuration:0} s. Recarga de {ExpeditionSimulation.ShieldRecharge:0} s desde a ativação. Use P ou o botão no combate."),
            "infusion" => ("Infusão Etérea", $"Com o Alquimista vivo, cura até {_simulation.CurrentInfusionAmount:0} PV (+8% da base por nível) do aliado vivo com menor proporção de vida quando alguém chega a {ExpeditionSimulation.InfusionHealthThreshold*100:0}% ou menos. Recarga de {ExpeditionSimulation.InfusionRecharge:0} s. O ganho é limitado pela vida máxima."),
            _ => ("Cura no descanso", $"Após 2 s de descanso entre encontros, cada aliado vivo recupera até {ExpeditionSimulation.RestHealingFraction*100:0}% de sua vida máxima se o Alquimista estiver vivo. Para {member.Name}: até {member.MaxHealth*ExpeditionSimulation.RestHealingFraction:0} PV, limitado à vida faltante.")
        };
        Set(_explanationTitle, title);
        if (_lastExplanation != text) { _lastExplanation=text; Set(_explanationText, text); }
    }
}
