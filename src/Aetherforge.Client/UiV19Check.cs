using Aetherforge.Domain;
using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

/// <summary>Observes a naturally advancing route. UI operations use Viewport input, never Pressed callbacks.</summary>
public partial class UiV19Check : Node
{
    public string EvidencePath { get; set; }="";
    public string CaptureDirectory { get; set; }="";
    private ExpeditionView _view=null!;
    private readonly HashSet<Window> _entered = new();
    public void Bind(ExpeditionView view)=>_view=view;
    public override async void _Ready()
    {
        try
        {
            await Frames(4); await _view.VerifyLayouts();
            var sim=_view.Simulation;
            var status=Descendants(_view).OfType<Window>().Single(w=>w.Name=="Instrument0");
            var hero=Descendants(_view).OfType<Window>().Single(w=>w.Name=="Instrument1");
            var xp=Descendants(status).OfType<ProgressBar>().Single(bar=>bar.Name=="StatusExperienceBar");
            var xpText=Descendants(status).OfType<Label>().Single(label=>label.Name=="StatusExperienceText");
            Require(sim.Party.All(p=>p.Progression.Level==1 && p.Progression.Experience==0),"initial progress");
            Require(!Descendants(status).OfType<Button>().Any(button=>button.Name=="RoutePause" || button.Text=="Recuar"),"route actions duplicated in Status");
            await Until(()=>sim.Rewards.Receipts.Count>0,10000);
            await Click(Button("RoutePause")); Require(sim.Paused,"pause first reward");
            Require(sim.Rewards.Coins==8 && sim.Rewards.Scrap==1 && sim.Party.All(p=>p.Progression.Experience==12),"first victory credits");
            Require(xp.Value==12 && xp.MaxValue==60 && xpText.Text.Contains("12 / 60"),"XP bar not bound to reward");
            Require(Descendants(_view).OfType<Label>().Single(label=>label.Name=="VictoryRewardNotice").Visible,"missing reward feedback");
            await Capture(status,"status-first-reward");
            await Click(Button("RoutePause"));
            await Until(()=>sim.Phase is ExpeditionPhase.Complete or ExpeditionPhase.Defeat,25000);
            Require(sim.Phase==ExpeditionPhase.Complete && sim.Rewards.Receipts.Count==5,"route did not complete");
            Require(sim.Party.All(p=>p.Progression.Level==2 && p.Progression.Experience==62),"level two expected");
            await Frames(2);
            Require(xp.Value==62 && xp.MaxValue==90 && xpText.Text.Contains("NÍVEL 2"),"XP overflow not visible");
            Require(sim.Rewards.Inventory.Count>=1 && sim.Rewards.Receipts.Last().Drop!=null,"boss loot missing");
            var firstItem=sim.Rewards.Inventory[0];
            var firstSlot=Button("LootSlot0"); Require(!firstSlot.Disabled,"owned loot disabled");
            await Click(firstSlot);
            await Frames(3);
            Require(_view.LootPanel.SelectedItemId==firstItem.Id,"item inspection did not select exact drop");
            var detail=Descendants(hero).OfType<Label>().Single(l=>l.Name=="LootItemDetail");
            Require(detail.Text.Contains(firstItem.Name),"item description mismatch");
            var heroScroll=Descendants(hero).OfType<ScrollContainer>().First();
            Require(heroScroll.GetGlobalRect().Grow(1).Encloses(detail.GetGlobalRect()),"selected item description not visible");
            var bagRect=_view.LootPanel.Bag.GetGlobalRect();
            var outside=Descendants(_view.LootPanel.Bag).OfType<Button>().Where(button=>!bagRect.Encloses(button.GetGlobalRect())).ToArray();
            Require(outside.Length==0,"loot slot outside grid "+bagRect+": "+string.Join("; ",outside.Select(button=>button.Name+" "+button.GetGlobalRect())));
            await Capture(hero,"loot-after-boss",false);
            await Capture(status,"status-level-two");
            if(CaptureDirectory.Length>0 && DisplayServer.GetName()!="headless")
                GetViewport().GetTexture().GetImage().SavePng(Path.Combine(CaptureDirectory,"overview-level-two.png"));
            var coins=sim.Rewards.Coins; var random=sim.Rewards.RandomState;
            await Click(Button("ResetRoute"));
            Require(sim.Phase==ExpeditionPhase.March && sim.Victories==0 && sim.Rewards.Coins==coins && sim.Rewards.RandomState==random && sim.Party.All(p=>p.Progression.TotalExperience==122),"retry lost progress or reset RNG");
            await Click(Button("RoutePause")); Require(sim.Paused,"pause new attempt");
            await _view.VerifyLayouts();
            var report=new { passed=true, scale=BrassUi.Scale, display=DisplayServer.GetName(), overview=_view.Overview,
                input="Viewport.PushInput, no direct Pressed, no simulation Advance from QA",
                levels=sim.Party.Select(p=>new {p.Name,p.Progression.Level,p.Progression.Experience,p.Progression.RequiredExperience,p.MaxHealth,p.Attack,p.Defense}).ToArray(),
                sim.Rewards.Coins,sim.Rewards.Scrap,sim.Rewards.Components, receipts=sim.Rewards.Receipts.Count,
                loot=sim.Rewards.Inventory.Select(item=>new{item.Id,item.Name,item.Role,item.Rarity,item.Slot}).ToArray(),
                checks=new[]{"first victory XP and feedback","real level-up and overflow","boss item owned and inspectable","selected item description visible","bag grid bounds","retry retains progress and RNG","route actions moved from Status"}};
            if(EvidencePath.Length>0) { Directory.CreateDirectory(Path.GetDirectoryName(EvidencePath)!); File.WriteAllText(EvidencePath,JsonSerializer.Serialize(report,new JsonSerializerOptions{WriteIndented=true})); }
            GD.Print("UI V19 PROGRESSION PASS "+JsonSerializer.Serialize(report)); GetTree().Quit(0);
        }
        catch(Exception error) { GD.PrintErr("UI V19 PROGRESSION FAIL: "+error); GetTree().Quit(1); }
    }
    private Button Button(string name)=>Descendants(_view).OfType<Button>().Single(button=>button.Name==name);
    private async Task Until(Func<bool> condition,int milliseconds)
    {
        ulong end=Time.GetTicksMsec()+(ulong)milliseconds;
        while(!condition() && Time.GetTicksMsec()<end)await Frames(1);
        Require(condition(),"timeout waiting for natural route");
    }
    private async Task Capture(Window window,string name,bool top=true)
    {
        if(CaptureDirectory.Length==0 || DisplayServer.GetName()=="headless")return;
        Directory.CreateDirectory(CaptureDirectory);
        if(top) foreach(var scroll in Descendants(window).OfType<ScrollContainer>())scroll.ScrollVertical=0;
        await Frames(3); await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
        Require(window.GetTexture().GetImage().SavePng(Path.Combine(CaptureDirectory,name+".png"))==Error.Ok,"capture failed");
    }
    private async Task Click(Button button)
    {
        Require(!button.Disabled,"disabled button "+button.Name);
        for(Node? parent=button.GetParent();parent!=null && parent is not Window;parent=parent.GetParent())
            if(parent is ScrollContainer scroll)scroll.EnsureControlVisible(button);
        await Frames(2);
        var window=button.GetWindow(); var point=button.GetGlobalRect().GetCenter();
        Require(new Rect2(Vector2.Zero,window.Size).HasPoint(point),"button outside window");
        if(window.IsEmbedded()){point+=window.Position;window=_view.GetWindow();}
        if(_entered.Add(window) && window.GuiGetHoveredControl()==null)window.NotifyMouseEntered();
        window.PushInput(new InputEventMouseMotion{Position=point,GlobalPosition=point,WindowId=window.GetWindowId()},true);
        window.PushInput(new InputEventMouseButton{Position=point,GlobalPosition=point,ButtonIndex=MouseButton.Left,ButtonMask=MouseButtonMask.Left,Pressed=true,WindowId=window.GetWindowId()},true);
        window.PushInput(new InputEventMouseButton{Position=point,GlobalPosition=point,ButtonIndex=MouseButton.Left,Pressed=false,WindowId=window.GetWindowId()},true);
        await Frames(2);
    }
    private async Task Frames(int count){for(int i=0;i<count;i++)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);}
    private static IEnumerable<Node> Descendants(Node root){foreach(var node in root.GetChildren()){yield return node;foreach(var child in Descendants(node))yield return child;}}
    private static void Require(bool value,string message){if(!value)throw new InvalidOperationException(message);}
}
