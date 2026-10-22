using Godot;
using Aetherforge.Domain;

/// <summary>Five real route stops. Shape and text carry state independently of color.</summary>
public partial class ExpeditionMenuRoute : VBoxContainer
{
    public int StageCount => 5;
    private readonly Label[] _states = new Label[5];
    private readonly ExpeditionMenuRouteMark[] _marks = new ExpeditionMenuRouteMark[5];
    private static readonly string[] Names = { "Entrada", "Trilhos", "Pressão", "Ninho", "Fornalha" };

    public override void _Ready()
    {
        AddThemeConstantOverride("separation", BrassUi.S(6));
        for (var i = 0; i < StageCount; i++)
        {
            var paper = new PanelContainer();
            paper.AddThemeStyleboxOverride("panel", BrassUi.Box(BrassUi.Linen, BrassUi.Linen, 0, 8, 5));
            AddChild(paper);
            var row = new HBoxContainer(); row.AddThemeConstantOverride("separation", BrassUi.S(10)); paper.AddChild(row);
            _marks[i] = new ExpeditionMenuRouteMark { CustomMinimumSize = new Vector2(BrassUi.S(28), BrassUi.S(32)) };
            row.AddChild(_marks[i]);
            var labels = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            labels.AddThemeConstantOverride("separation", 0); row.AddChild(labels);
            var name = BrassUi.Emphasis($"{i + 1:00}  {Names[i]}", 16, BrassUi.Soot);
            name.AutowrapMode = TextServer.AutowrapMode.WordSmart; labels.AddChild(name);
            _states[i] = BrassUi.Wrap("Por alcançar", 13, BrassUi.Soot); labels.AddChild(_states[i]);
        }
    }

    public void UpdateRoute(ExpeditionSimulation simulation)
    {
        if (!IsNodeReady()) return;
        for (var i = 0; i < StageCount; i++)
        {
            bool completed = i < simulation.Victories;
            bool current = i == simulation.StageIndex && !completed;
            bool stopped = current && (simulation.Phase is ExpeditionPhase.Defeat or ExpeditionPhase.Retreated);
            string state = completed ? "Concluído" : !current ? "Por alcançar" : simulation.Phase switch
            {
                ExpeditionPhase.March => "Atual · em marcha",
                ExpeditionPhase.Combat => "Atual · em combate",
                ExpeditionPhase.Defeat => "Interrompido · equipe derrotada",
                ExpeditionPhase.Retreated => "Interrompido · recuo concluído",
                ExpeditionPhase.Loot => "Encontro vencido · descanso",
                _ => "Atual"
            };
            if (current && simulation.Paused && !stopped) state += " · pausa";
            if (_states[i].Text != state) _states[i].Text = state;
            _marks[i].SetState(completed ? 1 : stopped ? 3 : current ? 2 : 0);
        }
    }
}

/// <summary>Ring = future, checked square = complete, diamond = current, cross = stopped.</summary>
public partial class ExpeditionMenuRouteMark : Control
{
    private int _state = -1;
    public override void _Ready() { MouseFilter = MouseFilterEnum.Ignore; Resized += QueueRedraw; }
    public void SetState(int state)
    {
        if (_state == state) return;
        _state = state; QueueRedraw();
    }
    public override void _Draw()
    {
        var center = Size / 2;
        float radius = BrassUi.S(10), stroke = BrassUi.S(2);
        if (_state == 1)
        {
            DrawRect(new Rect2(center - Vector2.One * radius, Vector2.One * radius * 2), BrassUi.Soot);
            DrawPolyline(new[] { center + new Vector2(-radius * .6f, 0), center + new Vector2(-radius * .1f, radius * .5f),
                center + new Vector2(radius * .65f, -radius * .5f) }, BrassUi.Linen, stroke, true);
        }
        else if (_state == 2)
            DrawColoredPolygon(new[] { center + new Vector2(0, -radius), center + new Vector2(radius, 0),
                center + new Vector2(0, radius), center + new Vector2(-radius, 0) }, BrassUi.Wine);
        else if (_state == 3)
        {
            DrawLine(center - Vector2.One * radius, center + Vector2.One * radius, BrassUi.Soot, stroke, true);
            DrawLine(center + new Vector2(-radius, radius), center + new Vector2(radius, -radius), BrassUi.Soot, stroke, true);
        }
        else DrawCircle(center, radius, BrassUi.Soot, false, stroke, true);
    }
}
