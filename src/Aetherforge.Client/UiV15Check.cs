using Godot;
using Aetherforge.Domain;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

/// <summary>Opt-in component fixtures and observation of a live route. Never advances the simulation.</summary>
public partial class UiV15Check : Node
{
    public string EvidencePath { get; set; } = "";
    public string CaptureDirectory { get; set; } = "";
    private ExpeditionView _view = null!;
    private readonly List<string> _checks = new();
    public void Bind(ExpeditionView view) => _view = view;
    public override async void _Ready()
    {
        try
        {
            await Frames(4);
            bool reducedAtStart = BrassMotion.Reduced;
            // Explicit isolated fixtures: these values never enter the expedition or a save.
            var fixture = new Control { Position = new Vector2(-5000,-5000), Size = new Vector2(240,100) };
            _view.AddChild(fixture);
            var meter = new BrassVitalMeter { Size = new Vector2(200,9) }; fixture.AddChild(meter);
            BrassMotion.Reduced=false;
            meter.SetHealth(100,100,true); meter.SetHealth(65,100);
            Require(Math.Abs(meter.HealthRatio-.65)<.001 && meter.HasTrail, "vida deve atualizar antes da cauda");
            meter.Paused=true; float frozen=meter.TrailRatio; await RealSeconds(.42);
            Require(meter.TrailRatio==frozen, "cauda nao congelou com pausa");
            meter.Paused=false; await RealSeconds(.45);
            Require(!meter.HasTrail, "cauda nao terminou em 360 ms");
            meter.SetHealth(83,100); Require(meter.HealChanges==1 && Math.Abs(meter.HealthRatio-.83)<.001, "cura nao chegou imediatamente");
            meter.SetHealth(50,100); BrassMotion.Reduced=true;
            Require(!meter.HasTrail, "reduzir movimento nao limpou cauda");
            meter.SetHealth(100,100,true); Require(!meter.HasTrail && meter.HealthRatio==1, "reset visual criou cura antiga");
            _checks.Add("fixture HP: dado imediato, cauda limitada, pausa, cura, reduzir movimento e reset");

            var button=BrassUi.Button("Fixture de componente",()=>{}); button.Position=new Vector2(0,30); button.Size=new Vector2(210,40); fixture.AddChild(button);
            await Frames(2);
            var feedback=button.GetChildren().OfType<BrassButtonFeedback>().Single();
            var original=button.GetRect(); BrassMotion.Reduced=false;
            // Component signal fixtures, not a physical mouse claim. Full native input is tested separately.
            int before=GetTree().GetProcessedTweens().Count(tween=>tween.IsRunning());
            for(int i=0;i<24;i++)button.EmitSignal(i%2==0?Control.SignalName.MouseEntered:Control.SignalName.MouseExited);
            Require(GetTree().GetProcessedTweens().Count(tween=>tween.IsRunning())<=before+1,"tweens empilharam");
            await RealSeconds(.2); Require(!feedback.Animating && feedback.AccentAmount<.01,"accent nao voltou ao repouso");
            BrassMotion.Reduced=true; button.EmitSignal(Control.SignalName.MouseEntered);
            Require(!feedback.Animating && feedback.AccentAmount>.6,"modo reduzido deve responder sem tween");
            Require(button.GetRect()==original,"feedback alterou layout/hit rectangle");
            int transitions=feedback.TransitionCount;
            fixture.QueueFree(); await Frames(2); BrassMotion.Reduced=reducedAtStart;
            _checks.Add("fixture sinais: um tween interrompivel, repouso, movimento reduzido e retangulo imovel");

            var status=Descendants(_view).OfType<Window>().First(w=>w.Name=="Instrument0"); status.Visible=true;
            var rows=Descendants(status).OfType<BrassPartyRow>().OrderBy(row=>row.MemberIndex).ToArray();
            Require(rows.Length==3,"tres fichas obrigatorias");
            var deadline=Time.GetTicksMsec()+22000;
            while(rows.Sum(row=>row.Meter.DamageChanges)==0 && Time.GetTicksMsec()<deadline)await Frames(1);
            Require(rows.Sum(row=>row.Meter.DamageChanges)>0,"fichas nao observaram dano real");
            _view.Simulation.Paused=true; await Frames(4); // QA pause only; owner remains ExpeditionView.
            for(int i=0;i<3;i++)Require(Math.Abs(rows[i].Meter.HealthRatio-_view.Simulation.Party[i].Health/_view.Simulation.Party[i].MaxHealth)<.001,"UI difere da vida real");
            if(CaptureDirectory.Length>0 && DisplayServer.GetName()!="headless")
            {
                Directory.CreateDirectory(CaptureDirectory);
                foreach(var scroll in Descendants(status).OfType<ScrollContainer>())scroll.ScrollVertical=0;
                await Frames(3); await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
                Require(status.GetTexture().GetImage().SavePng(Path.Combine(CaptureDirectory,"status-damage.png"))==Error.Ok,"captura Status falhou");
                foreach(var scroll in Descendants(status).OfType<ScrollContainer>())scroll.ScrollVertical=100000;
                await Frames(3); await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
                Require(status.GetTexture().GetImage().SavePng(Path.Combine(CaptureDirectory,"status-options.png"))==Error.Ok,"captura opcoes falhou");
            }
            _checks.Add("rota viva observada: dano real nas fichas; selecao/input nativo cobertos pela suite UI V14 ampliada");
            var report=new {passed=true,scale=BrassUi.Scale,display=DisplayServer.GetName(),reducedAtStart,transitions,checks=_checks,health=_view.Simulation.Party.Select(p=>p.Health).ToArray(),damageChanges=rows.Select(r=>r.Meter.DamageChanges).ToArray()};
            if(EvidencePath.Length>0){Directory.CreateDirectory(Path.GetDirectoryName(EvidencePath)!);File.WriteAllText(EvidencePath,JsonSerializer.Serialize(report,new JsonSerializerOptions{WriteIndented=true}));}
            GD.Print("UI V15 COMPONENT PASS "+JsonSerializer.Serialize(report));GetTree().Quit(0);
        }
        catch(Exception error){GD.PrintErr("UI V15 COMPONENT FAIL: "+error);GetTree().Quit(1);}
    }
    private async Task Frames(int n){for(int i=0;i<n;i++)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);}
    private async Task RealSeconds(double seconds){ulong until=Time.GetTicksMsec()+(ulong)(seconds*1000);while(Time.GetTicksMsec()<until)await Frames(1);}
    private static IEnumerable<Node> Descendants(Node root){foreach(var child in root.GetChildren()){yield return child;foreach(var next in Descendants(child))yield return next;}}
    private static void Require(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
}
