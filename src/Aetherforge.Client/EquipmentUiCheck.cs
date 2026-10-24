using Aetherforge.Domain;
using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

/// <summary>Opt-in isolated UI fixtures. Actual gestures use PushInput; reflection only seeds boundary data or delivers a stale callback.</summary>
public partial class EquipmentUiCheck : Node
{
    public string EvidencePath {get;set;}="";
    public string CaptureDirectory {get;set;}="";
    private ExpeditionView _view=null!;
    private readonly List<string> _checks=new();
    private readonly HashSet<Window> _entered=new();
    private int _assertions;
    public void Bind(ExpeditionView view)=>_view=view;
    public override async void _Ready()
    {
        try
        {
            await Frames(4);
            foreach(int i in new[]{0,1,2}) if(!Window(i).Visible) await Click(Button("OpenInstrument"+i));
            await Click(Button("RoutePause"));
            var sim=_view.Simulation;
            var armor=Add(PartyRole.Tank,LootSlot.Armor);
            var weapon=Add(PartyRole.Dps,LootSlot.Weapon);
            var focus=Add(PartyRole.Healer,LootSlot.Weapon);
            var health=Add(PartyRole.Tank,LootSlot.Accessory,LootRarity.Rare);
            ForceRefresh(); await Frames(4);
            await Inspect(armor.Id);
            Require(sim.ReadView().Members[0].Loadout.Armor==null,"inspection is read only");
            await Key(Button("CompareCandidate"),Godot.Key.Enter);
            Require(_view.DisplayedEquipmentPreview?.BlockReason==ExpeditionCode.ManagementRequired,"F01 blocked before explicit management");
            Require(Button("ComparisonBack1").HasFocus(),"U15 opening Enter focuses Back");
            await Click(Button("EquipmentManagement1"));
            Require(sim.Management==ExpeditionManagement.Active && sim.Paused,"F10 preserves prior pause");
            Require(_view.DisplayedEquipmentPreview?.DefenseDelta==2,"E01 domain preview defense");
            await Capture("empty-slot");
            var comparisonScroll=Descendants(Window(1)).OfType<ScrollContainer>().First();
            comparisonScroll.ScrollVertical=(int)comparisonScroll.GetVScrollBar().MaxValue/2; await Capture("comparison-middle");
            comparisonScroll.ScrollVertical=(int)comparisonScroll.GetVScrollBar().MaxValue; await Capture("comparison-end");
            Button("ConfirmEquipment1").GrabFocus(); await Capture("confirm-focused");
            await Key(Button("ConfirmEquipment1"),Godot.Key.Enter);
            Require(sim.ReadView().Members[0].Stats.Defense==12 && sim.ReadView().Members[0].Loadout.Armor?.Id==armor.Id,"E01 explicit confirm applies");
            int cues=_view.EquipmentSelection.AppliedFeedbackCount;
            Require(cues==1 && Button("ConfirmEquipment1").Disabled,"U18 result consumes intent once");
            Require(Button("EquipmentSlot0-Armor").Text.Contains(armor.Name),"U01 Status slot updated same action");
            await Capture("applied");
            await Key(Button("ComparisonBack1"),Godot.Key.Escape);
            Require(Window(1).Visible && Button("EquipmentSlot1-Armor").HasFocus(),"U08 Escape one layer and equipped ID focus");
            await Click(Button("EquipmentSlot1-Armor"));
            await Click(Button("PreviewUnequip1"));
            Require(_view.DisplayedEquipmentPreview?.DefenseDelta==-2,"E01 unequip removes full bonus");
            await Click(Button("ConfirmEquipment1"));
            Require(sim.ReadView().Inventory.Any(x=>x.Id==armor.Id) && sim.ReadView().Members[0].Stats.Defense==10,"U07 remove returns same ID");
            await Click(Button("ComparisonBack1"));
            Require(_view.LootPanel.SelectedItemId==armor.Id,"U16 removed item selection restored by ID");
            _checks.Add("U01/U07/U08/U15/U16/U18: inspect, explicit compare/equip/remove, immediate Status, input/focus, no redraw cue");

            await Click(Button("SelectMember1")); await Inspect(weapon.Id); await Click(Button("CompareCandidate"));
            var old=_view.EquipmentSelection.Current!;
            await Click(Button("SelectStatusMember0"));
            Require(_view.DisplayedEquipmentPreview?.BlockReason==ExpeditionCode.RoleMismatch && Button("ConfirmEquipment1").Disabled,"E05/E13 incompatible target retained");
            await Click(Button("SelectStatusMember1"));
            string before=State();
            typeof(ExpeditionView).GetMethod("ConfirmEquipment",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(_view,new object[]{1,old});
            Require(State()==before && _view.EquipmentSelection.AppliedFeedbackCount==2,"E13 ABA stale callback cannot consume new preview");
            await Click(Button("ConfirmEquipment1"));
            Require(sim.ReadView().Members[1].Stats.Attack==20,"U03 fresh activation after ABA");
            await Click(Button("ComparisonBack1"));
            // Same attributes, different IDs and equal count swap.
            var equal=Add(PartyRole.Dps,LootSlot.Weapon); ForceRefresh();
            await Inspect(equal.Id); await Click(Button("CompareCandidate"));
            Require(_view.DisplayedEquipmentPreview?.AttackDelta==0 && !Button("ConfirmEquipment1").Disabled,"E02 equal is allowed");
            await Click(Button("ConfirmEquipment1"));
            Require(sim.ReadView().Members[1].Loadout.Weapon?.Id==equal.Id && sim.ReadView().Inventory.Any(x=>x.Id==weapon.Id),"E02 identity swap");
            Require(Descendants(_view.LootPanel.Bag).OfType<Button>().Any(b=>b.GetMeta("item_id","").AsString()==weapon.Id),"U06 same count bag refresh");
            await Click(Button("ComparisonBack1"));
            var better=Add(PartyRole.Dps,LootSlot.Weapon,LootRarity.Uncommon); ForceRefresh();
            await Inspect(better.Id); await Click(Button("CompareCandidate"));
            Require(_view.DisplayedEquipmentPreview?.AttackDelta==1,"E03 subtract old weapon bonus");
            await Click(Button("ConfirmEquipment1")); await Click(Button("ComparisonBack1"));
            await Inspect(weapon.Id); await Click(Button("CompareCandidate"));
            Require(_view.DisplayedEquipmentPreview?.AttackDelta==-1 && Button("ConfirmEquipment1").Text.Contains("perda"),"E04 explicit loss action");
            before=State(); await Key(Button("ComparisonBack1"),Godot.Key.Escape);
            Require(State()==before,"U08 cancel leaves full snapshot intact");
            _checks.Add("U02/U03/U06/U09: equal/better/worse, same count, shared selection ABA, stale gesture, cancel");

            await Click(Button("SelectMember2")); await Inspect(focus.Id); await Click(Button("CompareCandidate"));
            Require(_view.DisplayedEquipmentPreview?.After?.InfusionPotential==44,"E06 infusion preview");
            await Click(Button("ConfirmEquipment1"));
            Require(sim.CurrentInfusionAmount==44,"U04 next healing equals displayed infusion");
            await Click(Button("ComparisonBack1")); await Click(Button("SelectMember0"));
            await Inspect(health.Id); await Click(Button("CompareCandidate"));
            var preview=_view.DisplayedEquipmentPreview!;
            Require(preview.HealthAfter==preview.HealthBefore && preview.HealthDelta==22,"E07 max increase never heals");
            await Click(Button("ConfirmEquipment1")); await Click(Button("ComparisonBack1"));
            await Click(Button("EquipmentSlot0-Accessory")); await Click(Button("PreviewUnequip0"));
            await Capture("status-before-check");
            Require(_view.DisplayedEquipmentPreview?.HealthDelta==-22,"U05 Status removal preview: "+_view.DisplayedEquipmentPreview?.HealthDelta+" "+_view.DisplayedEquipmentPreview?.BlockReason);
            await Capture("status-removal"); await Click(Button("ComparisonBack0"));
            // Boundary HP fixtures are confined to this opt-in harness.
            sim.Party[0].Health=202; ForceRefresh();
            var commonHealth=Add(PartyRole.Tank,LootSlot.Accessory); ForceRefresh();
            await Inspect(commonHealth.Id); await Click(Button("CompareCandidate"));
            Require(_view.DisplayedEquipmentPreview?.HealthAfter==190 && Label("EquipmentReason1").Text.Contains("202 → 190"),"E08 explicit HP cut");
            await Capture("health-cut"); await Click(Button("ConfirmEquipment1")); await Click(Button("ComparisonBack1"));
            await Inspect(health.Id); await Click(Button("CompareCandidate"));
            Require(_view.DisplayedEquipmentPreview?.HealthAfter==190,"E08 switching back cannot heal");
            await Click(Button("ConfirmEquipment1")); await Click(Button("ComparisonBack1"));
            sim.Party[0].Health=0; ForceRefresh();
            await Inspect(commonHealth.Id); await Click(Button("CompareCandidate"));
            Require(_view.DisplayedEquipmentPreview?.HealthAfter==0 && Label("EquipmentReason1").Text.Contains("Continua caído"),"E09 fallen warning");
            await Click(Button("ComparisonBack1"));
            sim.Party[0].Health=202; ForceRefresh();
            _checks.Add("U04/U05: authoritative infusion, HP and both instrument entry points");

            // Fill via fixture only; production has no item grant/import path.
            while(sim.ReadView().Inventory.Count<21)Add(PartyRole.Tank,LootSlot.Armor);
            Add(PartyRole.Healer,LootSlot.Accessory); Add(PartyRole.Dps,LootSlot.Accessory);
            ForceRefresh(); await Frames(3);
            await Click(Button("EquipmentSlot1-Accessory")); await Click(Button("PreviewUnequip1"));
            Require(_view.DisplayedEquipmentPreview?.BlockReason==ExpeditionCode.BagFull && Button("ConfirmEquipment1").Disabled,"E12 bag full removal visual rejection");
            await Capture("bag-full"); await Click(Button("ComparisonBack1"));
            await Inspect(armor.Id); await Click(Button("CompareCandidate"));
            var fifo=sim.ReadView().PendingDrops[0].Id;
            await Click(Button("ConfirmEquipment1"));
            Require(sim.ReadView().Inventory.Count==21 && sim.ReadView().Inventory.Last().Id==fifo && sim.ReadView().PendingDrops.Count==1,"E11 FIFO recovery and same count view");
            Require(Label("EquipmentReason1").Text.Contains("Recuperados da pendência: 1"),"U07 recovery receipt visible");
            await Click(Button("ComparisonBack1"));
            var another=sim.ReadView().Inventory.First(x=>x.Role==PartyRole.Tank && x.Slot==LootSlot.Armor);
            await Inspect(another.Id); await Click(Button("CompareCandidate"));
            await Click(Button("ConfirmEquipment1"));
            Require(sim.ReadView().Inventory.Count==21 && sim.ReadView().PendingDrops.Count==1,"E10 full swap preserves pending");
            await Click(Button("ComparisonBack1"));
            _checks.Add("U06/U07: full bag, swap 21→21, blocked removal, FIFO receipt");

            foreach(var size in new[]{new Vector2I(1366,728),new Vector2I(1920,1040),new Vector2I(2560,1400)}) await _view.VerifyMenuBounds(size);
            // All windows closed: Active remains reachable from the root HUD.
            await Click(Button("CloseInstrument0")); await Click(Button("CloseInstrument1")); await Click(Button("CloseInstrument2"));
            Require(sim.Management==ExpeditionManagement.Active && Button("EquipmentManagement3").IsVisibleInTree(),"F10 permanent HUD exit");
            _view.GetViewport().GuiReleaseFocus(); await RootKey(Godot.Key.Space);
            Require(sim.Management==ExpeditionManagement.Active && sim.Paused,"U14 Space cannot change active management");
            await Capture("closed-management"); await Click(Button("EquipmentManagement3"));
            Require(sim.Management==ExpeditionManagement.None && sim.Paused,"F10 finish preserves pause");
            // Keyboard-only traversal: initial root focus, then Tab/Enter/Escape through the GUI.
            Button("OpenInstrument1").GrabFocus();
            await Navigate(Button("OpenInstrument1")); await Key(Button("OpenInstrument1"),Godot.Key.Enter);
            await Navigate(Button("EquipmentSlot1-Armor")); await Key(Button("EquipmentSlot1-Armor"),Godot.Key.Enter);
            await Navigate(Button("ChooseBagCandidate1")); await Key(Button("ChooseBagCandidate1"),Godot.Key.Enter);
            var focusedCandidate=Window(1).GuiGetFocusOwner() as Button;
            Require(focusedCandidate!=null && focusedCandidate.Name.ToString().StartsWith("LootSlot"),"U15 choosing bag focuses candidate without inspecting");
            await Key(focusedCandidate!,Godot.Key.Enter);
            await Navigate(Button("CompareCandidate")); await Key(Button("CompareCandidate"),Godot.Key.Enter);
            await Key(Button("ComparisonBack1"),Godot.Key.Escape);
            await Click(Button("CloseInstrument1"));
            await Click(Button("OpenInstrument2")); await Click(Button("RoutePause"));
            await Until(()=>sim.Phase==ExpeditionPhase.Combat,12000);
            await Click(Button("RoutePause"));
            await Click(Button("EquipmentManagement2"));
            Require(sim.Management==ExpeditionManagement.Requested && sim.Paused,"F02 paused combat only requests");
            await Click(Button("OpenInstrument1"));
            await Inspect(sim.ReadView().Inventory.First().Id); await Click(Button("CompareCandidate"));
            Require(_view.DisplayedEquipmentPreview?.BlockReason==ExpeditionCode.InCombat && Button("ConfirmEquipment1").Disabled,"U11 combat paused never permits confirm");
            var inCombatIntent=_view.EquipmentSelection.Current;
            sim.Party[0].GainExperience(48); // Threshold fixture; the UI receives the real victory/level event.
            await Click(Button("EquipmentManagement2"));
            Require(sim.Management==ExpeditionManagement.None && sim.Paused,"F04 cancel does not resume");
            await Click(Button("EquipmentManagement2")); await Click(Button("RoutePause"));
            await Until(()=>sim.Management==ExpeditionManagement.Active,15000);
            Require(sim.Phase==ExpeditionPhase.Loot && sim.PhaseRemaining==2 && sim.Victories==1,"U12 first victory stops with full rest");
            await Frames(4); Require(sim.PhaseRemaining==2,"U12 wait does not tick rest");
            Require(sim.ReadView().Members[0].Level==2 && _view.DisplayedEquipmentPreview?.ExpectedStateRevision==sim.StateRevision && !_view.EquipmentSelection.IsCurrent(inCombatIntent!),"U10 new preview after victory/level and authorization change");
            await Click(Button("ComparisonBack1"));
            await Click(Button("EquipmentManagement2"));
            await Click(Button("RouteRetreat")); Require(sim.Phase==ExpeditionPhase.Retreated,"U13 terminal route command");
            if(!Window(1).Visible)await Click(Button("OpenInstrument1")); await Click(Button("EquipmentSlot1-Accessory")); await Click(Button("PreviewUnequip1"));
            var previous=_view.EquipmentSelection.Current;
            await RootKey(Godot.Key.R); await Frames(3);
            Require(!_view.EquipmentSelection.IsCurrent(previous!) && sim.ReadView().Members[0].Loadout.Accessory?.Id==health.Id,"F09 restart invalidates and keeps loadout");
            await Click(Button("RoutePause"));
            _checks.Add("U08/U10/U11/U12/U13/U14/U17: bounds, closed HUD, Space, paused combat request/cancel, victory stop, restart");
            Require(_view.EquipmentSelection.AppliedFeedbackCount==11,"U18 one feedback per eleven applied transactions");
            var receipt=sim.CaptureCheckpoint().Operations.First(x=>x.Command is EquipmentCommand);
            before=State(); var replay=sim.Execute((EquipmentCommand)receipt.Command);
            Require(replay.Outcome==CommandOutcome.Replay && !_view.EquipmentSelection.AcceptFeedback(replay) && State()==before,"U18 Replay reads present without repeated cue or mutation");
            await _view.VerifyLayouts();
            foreach(var button in Descendants(_view).OfType<Button>().Where(b=>b.Name.ToString().StartsWith("Equipment") || b.Name.ToString().StartsWith("ConfirmEquipment") || b.Name.ToString().StartsWith("ComparisonBack")))
            foreach(var state in new[]{"normal","hover","pressed","hover_pressed","disabled"})
            {
                var style=button.GetThemeStylebox(state) as StyleBoxFlat;
                if(style==null)continue;
                var ink=button.GetThemeColor(state=="disabled"?"font_disabled_color":"font_color");
                double a=Luminance(ink),b=Luminance(style.BgColor);
                Require((Math.Max(a,b)+.05)/(Math.Min(a,b)+.05)>=4.5,"U17 text contrast "+button.Name+" "+state);
            }
            var report=new {passed=true,scale=BrassUi.Scale,display=DisplayServer.GetName(),overview=_view.Overview,reducedMotion=BrassMotion.Reduced,viewport=_view.GetWindow().Size.ToString(),assertions=_assertions,checks=_checks,
                input="Viewport.PushInput mouse/keyboard; isolated reflection seeding, stale callback injection",limitations="No physical keyboard/mouse, DPI, multimonitor or human comprehension certification"};
            if(EvidencePath.Length>0){Directory.CreateDirectory(Path.GetDirectoryName(EvidencePath)!);File.WriteAllText(EvidencePath,JsonSerializer.Serialize(report,new JsonSerializerOptions{WriteIndented=true}));}
            GD.Print("EQUIPMENT UI PASS "+JsonSerializer.Serialize(report)); GetTree().Quit(0);
        }
        catch(Exception error){GD.PrintErr("EQUIPMENT UI FAIL: "+error);GetTree().Quit(1);}
    }
    private ExpeditionDrop Add(PartyRole role,LootSlot slot,LootRarity rarity=LootRarity.Common)
    {
        var rewards=_view.Simulation.Rewards;
        var field=typeof(ExpeditionRewards).GetField("_nextItem",BindingFlags.Instance|BindingFlags.NonPublic)!;
        long sequence=(long)field.GetValue(rewards)!+1; field.SetValue(rewards,sequence);
        var item=(ExpeditionDrop)Activator.CreateInstance(typeof(ExpeditionDrop),BindingFlags.Instance|BindingFlags.NonPublic,null,new object[]{"local-"+sequence.ToString("D6"),role,slot,rarity},null)!;
        var registry=typeof(ExpeditionRewards).GetField("Items",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(rewards)!;
        registry.GetType().GetMethod("Add",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(registry,new object[]{item});
        return item;
    }
    private void ForceRefresh()
    {
        typeof(ExpeditionLootView).GetField("_renderedRevision",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(_view.LootPanel,-1L);
        typeof(ExpeditionView).GetMethod("RefreshState",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(_view,new object?[]{null});
    }
    private string State()=>JsonSerializer.Serialize(_view.Simulation.CaptureCheckpoint());
    private async Task Inspect(string id)
    {
        int index=_view.Simulation.ReadView().Inventory.ToList().FindIndex(x=>x.Id==id);
        Require(index>=0,"fixture item in bag"); await Click(Button("LootSlot"+index));
    }
    private Button Button(string name)=>Descendants(_view).OfType<Button>().Single(b=>b.Name==name);
    private Label Label(string name)=>Descendants(_view).OfType<Label>().Single(b=>b.Name==name);
    private Window Window(int i)=>Descendants(_view).OfType<Window>().Single(w=>w.Name=="Instrument"+i);
    private async Task Key(Button button,Godot.Key key)
    {
        button.GrabFocus(); await Frames(1); var window=button.GetWindow();
        window.PushInput(new InputEventKey{Keycode=key,Pressed=true,WindowId=window.GetWindowId()},true);
        window.PushInput(new InputEventKey{Keycode=key,Pressed=false,WindowId=window.GetWindowId()},true); await Frames(3);
    }
    private async Task RootKey(Godot.Key key)
    {
        var window=_view.GetWindow();window.PushInput(new InputEventKey{Keycode=key,Pressed=true},true);window.PushInput(new InputEventKey{Keycode=key,Pressed=false},true);await Frames(3);
    }
    private async Task Click(Button button)
    {
        Require(!button.Disabled && button.IsVisibleInTree(),"enabled visible input "+button.Name);
        for(Node? p=button.GetParent();p!=null && p is not Godot.Window;p=p.GetParent())if(p is ScrollContainer scroll)scroll.EnsureControlVisible(button);
        await Frames(3); var window=button.GetWindow();var point=button.GetGlobalRect().GetCenter();
        Require(new Rect2(Vector2.Zero,window.Size).HasPoint(point),"button in viewport "+button.Name+" "+point+" "+window.Size);
        if(window.IsEmbedded()){point+=window.Position;window=_view.GetWindow();}
        if(_entered.Add(window) && window.GuiGetHoveredControl()==null)window.NotifyMouseEntered();
        window.PushInput(new InputEventMouseMotion{Position=point,GlobalPosition=point,WindowId=window.GetWindowId()},true);
        foreach(bool pressed in new[]{true,false})window.PushInput(new InputEventMouseButton{Position=point,GlobalPosition=point,ButtonIndex=MouseButton.Left,Pressed=pressed,WindowId=window.GetWindowId()},true);
        await Frames(3);
    }
    private async Task Capture(string name)
    {
        await Frames(3);
        foreach(var window in Descendants(_view).OfType<Window>().Where(w=>w.Visible))
        {
            var rect=new Rect2(Vector2.Zero,window.Size);
            foreach(var part in Descendants(window).OfType<Control>().Where(c=>c.Name=="FixedHeader" || c.Name=="FixedFooter"))
                Require(rect.Grow(1).Encloses(part.GetGlobalRect()),"U17 critical area clipped: "+name+" "+window.Name+" "+part.Name+" "+part.GetGlobalRect());
            Require(Descendants(window).OfType<ScrollContainer>().First().Size.Y>=BrassUi.S(40),"U17 scroll body minimum "+name+" "+window.Name);
        }
        if(CaptureDirectory.Length==0 || DisplayServer.GetName()=="headless")return;
        Directory.CreateDirectory(CaptureDirectory);await Frames(2);await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
        _view.GetViewport().GetTexture().GetImage().SavePng(Path.Combine(CaptureDirectory,name+".png"));
        if(!_view.Overview) foreach(var window in Descendants(_view).OfType<Window>().Where(w=>w.Visible))
            window.GetTexture().GetImage().SavePng(Path.Combine(CaptureDirectory,name+"-"+window.Name+".png"));
    }
    private async Task Navigate(Button target)
    {
        var window=target.GetWindow();
        for(int i=0;i<160 && !target.HasFocus();i++)
        {
            window.PushInput(new InputEventKey{Keycode=Godot.Key.Tab,Pressed=true},true);
            window.PushInput(new InputEventKey{Keycode=Godot.Key.Tab,Pressed=false},true); await Frames(1);
        }
        Require(target.HasFocus(),"U15 Tab reaches "+target.Name);
    }
    private static double Luminance(Color color)
    {
        double Channel(double c)=>c<=.04045?c/12.92:Math.Pow((c+.055)/1.055,2.4);
        return .2126*Channel(color.R)+.7152*Channel(color.G)+.0722*Channel(color.B);
    }
    private async Task Until(Func<bool> test,int ms){ulong end=Time.GetTicksMsec()+(ulong)ms;while(!test() && Time.GetTicksMsec()<end)await Frames(1);Require(test(),"natural route deadline");}
    private async Task Frames(int count){for(int i=0;i<count;i++)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);}
    private void Require(bool value,string message){_assertions++;if(!value)throw new InvalidOperationException(message);}
    private static IEnumerable<Node> Descendants(Node node){foreach(var child in node.GetChildren()){yield return child;foreach(var nested in Descendants(child))yield return nested;}}
}
