using Aetherforge.Domain;
using Godot;
using System;
using System.Linq;

public partial class ExpeditionView
{
    public EquipmentSelection EquipmentSelection { get; }=new();
    public EquipmentPreview? DisplayedEquipmentPreview => _equipmentPage>=0 ? _equipmentPages[_equipmentPage].Preview : null;
    private sealed class EquipmentPage
    {
        public Control[] NormalBody=Array.Empty<Control>(), NormalFooter=Array.Empty<Control>();
        public VBoxContainer Body=null!, Footer=null!;
        public HBoxContainer Target=null!;
        public Label Identity=null!, Detail=null!, Reason=null!;
        public Button Back=null!, Confirm=null!, Choose=null!, Remove=null!;
        public Action? ConfirmHandler;
        public EquipmentPreview? Preview;
        public EquipmentIntent? PressIntent;
        public LootSlot Slot;
        public string? Candidate;
        public EquipmentAction Action;
        public string ReturnItem="";
        public bool Comparing, Result;
    }
    private readonly EquipmentPage[] _equipmentPages={new(),new()};
    private readonly Button[,] _equipmentSlots=new Button[2,3];
    private readonly Button[] _managementButtons=new Button[4];
    private readonly Label[] _managementLabels=new Label[4];
    private int _equipmentPage=-1, _bagReturn=-1;
    private LootSlot _bagReturnSlot;
    private long _equipmentRevision=-1;
    private string _equipmentSession="";
    private long _equipmentAttempt;

    private void InitializeEquipment()
    {
        for(int menu=0;menu<2;menu++)
        {
            int origin=menu; var page=_equipmentPages[menu];
            var body=(VBoxContainer)_menuScrolls[menu].GetChild(0);
            var slots=Column(3); slots.Name="EquipmentSlots"+menu;
            var slotParent=menu==0?(VBoxContainer)StatusPanel:_heroDetails;
            slotParent.AddChild(slots); slotParent.MoveChild(slots,menu==0?5:2);
            slots.AddChild(BrassUi.Emphasis("Equipamento",15,BrassUi.Soot));
            for(int slot=0;slot<3;slot++)
            {
                var value=(LootSlot)slot;
                var button=EquipmentPresentation.Button($"EquipmentSlot{menu}-{value}","",()=>OpenEquipmentSlot(origin,value));
                slots.AddChild(button); _equipmentSlots[menu,slot]=button;
            }
            page.NormalBody=body.GetChildren().OfType<Control>().ToArray();
            page.NormalFooter=_menuFooters[menu].GetChildren().OfType<Control>().ToArray();
            page.Target=new HBoxContainer {Visible=false}; _menuHeaders[menu].AddChild(page.Target);
            var previous=EquipmentPresentation.Button("EquipmentPrevious"+menu,"‹",()=>SelectMember((_selected+2)%3));
            previous.CustomMinimumSize=new Vector2(S(36),S(36));
            previous.SizeFlagsHorizontal=SizeFlags.ShrinkBegin; page.Target.AddChild(previous);
            page.Identity=BrassUi.Wrap("",14,BrassUi.Soot); page.Identity.SizeFlagsHorizontal=SizeFlags.ExpandFill; page.Target.AddChild(page.Identity);
            var next=EquipmentPresentation.Button("EquipmentNext"+menu,"›",()=>SelectMember((_selected+1)%3));
            next.CustomMinimumSize=new Vector2(S(36),S(36));
            next.SizeFlagsHorizontal=SizeFlags.ShrinkEnd; page.Target.AddChild(next);
            page.Body=Column(6); page.Body.Visible=false; body.AddChild(page.Body);
            page.Detail=BrassUi.Wrap("",14,BrassUi.Soot); page.Detail.Name="EquipmentComparisonText"+menu; page.Body.AddChild(page.Detail);
            page.Choose=EquipmentPresentation.Button("ChooseBagCandidate"+menu,"Escolher na bolsa",()=>ChooseEquipmentBag(origin)); page.Body.AddChild(page.Choose);
            page.Remove=EquipmentPresentation.Button("PreviewUnequip"+menu,"Desequipar…",()=>OpenEquipmentComparison(origin,page.Slot,EquipmentAction.Unequip,null)); page.Body.AddChild(page.Remove);
            page.Footer=Column(3); page.Footer.Visible=false; _menuFooters[menu].AddChild(page.Footer);
            page.Reason=BrassUi.Wrap("",13,BrassUi.Tobacco); page.Reason.Name="EquipmentReason"+menu; page.Footer.AddChild(page.Reason);
            var actions=new HBoxContainer(); page.Footer.AddChild(actions);
            page.Back=EquipmentPresentation.Button("ComparisonBack"+menu,"Voltar",()=>BackEquipment(origin)); actions.AddChild(page.Back);
            page.Confirm=EquipmentPresentation.Button("ConfirmEquipment"+menu,"Equipar",()=>{},true); actions.AddChild(page.Confirm);
            page.Confirm.ButtonDown+=()=>page.PressIntent=EquipmentSelection.Current;
            page.Footer.AddChild(BrassUi.Wrap(EquipmentPresentation.SessionWarning,13,BrassUi.Tobacco));
            AddManagement(_menuFooters[menu],menu);
        }
        LootPanel.CompareRequested+=id=>
        {
            var item=Simulation.ReadView().Inventory.FirstOrDefault(x=>x.Id==id);
            if(item!=null) OpenEquipmentComparison(1,item.Slot,EquipmentAction.Equip,id);
        };
        LootPanel.InspectionChanged+=()=>EquipmentSelection.Invalidate();
        AddManagement(_menuFooters[2],2);
        // Separate from the hideable root footer: a closed instrument cannot trap the route.
        var hud=new VBoxContainer {Name="ManagementHud"};
        ((Container)_stagePanel.GetParent()).AddChild(hud);
        ((Container)_stagePanel.GetParent()).MoveChild(hud,1);
        AddManagement(hud,3);
    }

    private void AddManagement(VBoxContainer parent,int index)
    {
        _managementLabels[index]=BrassUi.Wrap("",13,index==3&&!Overview?BrassUi.Linen:BrassUi.Tobacco);
        _managementLabels[index].Name="ManagementState"+index; parent.AddChild(_managementLabels[index]);
        var row=new HBoxContainer(); parent.AddChild(row);
        _managementButtons[index]=EquipmentPresentation.Button("EquipmentManagement"+index,"Gerenciar entre encontros",()=>
        {
            var state=Simulation.ReadView().Management;
            ExecuteRoute(state==ExpeditionManagement.Active?RouteAction.FinishManagement:state==ExpeditionManagement.Requested?RouteAction.CancelManagementRequest:RouteAction.RequestManagement);
        });
        row.AddChild(_managementButtons[index]);
        if(index==3) row.AddChild(EquipmentPresentation.Button("ManagementOpenHero","Abrir Herói",()=>SetMenuVisible(1,true)));
    }

    private CommandResult ExecuteRoute(RouteAction action,bool paused=false)
    {
        EquipmentSelection.Invalidate();
        var view=Simulation.ReadView();
        var result=Simulation.Execute(new RouteCommand(Guid.NewGuid().ToString("N"),view.SessionId,view.AttemptId,action,paused));
        RefreshState(result.LatestReadModel);
        return result;
    }

    private void OpenEquipmentSlot(int origin,LootSlot slot)
    {
        CloseOtherEquipment(origin); EquipmentSelection.Invalidate();
        var page=_equipmentPages[origin]; page.Slot=slot; page.Candidate=null; page.Comparing=false; page.Result=false;
        page.ReturnItem=Simulation.ReadView().Members[_selected].Loadout.Get(slot)?.Id??"";
        ShowEquipmentPage(origin,true); RefreshEquipment(Simulation.ReadView());
        page.Choose.GrabFocus(); WireFocus(_menus[origin]);
    }

    private void OpenEquipmentComparison(int origin,LootSlot slot,EquipmentAction action,string? candidate)
    {
        CloseOtherEquipment(origin); EquipmentSelection.Invalidate();
        var page=_equipmentPages[origin]; page.Slot=slot; page.Action=action; page.Candidate=candidate; page.Comparing=true; page.Result=false;
        page.ReturnItem=candidate??Simulation.ReadView().Members[_selected].Loadout.Get(slot)?.Id??"";
        ShowEquipmentPage(origin,true); PresentEquipmentPreview(origin,Simulation.ReadView(),false);
        page.Back.GrabFocus(); WireFocus(_menus[origin]);
    }

    private void CloseOtherEquipment(int origin)
    {
        if(_equipmentPage>=0 && _equipmentPage!=origin) ShowEquipmentPage(_equipmentPage,false);
    }
    private void ShowEquipmentPage(int origin,bool show)
    {
        var page=_equipmentPages[origin];
        foreach(var control in page.NormalBody) control.Visible=!show;
        foreach(var control in page.NormalFooter) control.Visible=!show;
        page.Body.Visible=page.Footer.Visible=page.Target.Visible=show;
        _equipmentPage=show?origin:-1;
        _menuScrolls[origin].ScrollVertical=0;
        WireFocus(_menus[origin]);
    }

    private void ChooseEquipmentBag(int origin)
    {
        var slot=_equipmentPages[origin].Slot;
        EquipmentSelection.Invalidate(); ShowEquipmentPage(origin,false);
        _bagReturn=origin; _bagReturnSlot=slot;
        SetMenuVisible(1,true,opener:_equipmentSlots[origin,(int)slot]);
        LootPanel.FocusCompatible(Simulation.ReadView().Members[_selected].Role,slot);
        WireFocus(_menus[1]);
    }

    private bool BackEquipment(int origin)
    {
        if(_equipmentPage==origin)
        {
            var page=_equipmentPages[origin]; EquipmentSelection.Invalidate();
            ShowEquipmentPage(origin,false);
            if(origin==1 && LootPanel.FocusItem(page.ReturnItem)) { }
            else _equipmentSlots[origin,(int)page.Slot].GrabFocus();
            WireFocus(_menus[origin]); return true;
        }
        if(origin==1 && LootPanel.ClearInspection()) { EquipmentSelection.Invalidate(); WireFocus(_menus[1]); return true; }
        if(origin==1 && _bagReturn>=0)
        {
            int target=_bagReturn; _bagReturn=-1;
            SetMenuVisible(target,true); OpenEquipmentSlot(target,_bagReturnSlot); return true;
        }
        return false;
    }

    private void PresentEquipmentPreview(int origin,ExpeditionReadModel view,bool updated)
    {
        var page=_equipmentPages[origin];
        var member=view.Members[_selected];
        var preview=Simulation.Compare(new ComparisonQuery(view.SessionId,view.AttemptId,member.MemberId,page.Slot,page.Action,page.Candidate));
        page.Preview=preview;
        var intent=EquipmentSelection.Present(preview);
        if(page.ConfirmHandler!=null) page.Confirm.Pressed-=page.ConfirmHandler;
        page.ConfirmHandler=()=>
        {
            var gesture=page.PressIntent??intent; page.PressIntent=null;
            ConfirmEquipment(origin,gesture);
        };
        page.Confirm.Pressed+=page.ConfirmHandler;
        page.Confirm.Visible=true; page.Confirm.Disabled=!preview.CanConfirm;
        page.Choose.Visible=page.Remove.Visible=false;
        var candidate=view.Inventory.Concat(view.PendingDrops).Concat(view.Members.SelectMany(m=>new[]{m.Loadout.Weapon,m.Loadout.Armor,m.Loadout.Accessory}).OfType<ExpeditionDrop>()).FirstOrDefault(x=>x.Id==page.Candidate);
        var equipped=member.Loadout.Get(page.Slot);
        page.Detail.Text="Equipada: "+EquipmentPresentation.Item(equipped)+"\n"+
            (page.Action==EquipmentAction.Unequip?"Após: Vazio · bônus 0":"Nova: "+EquipmentPresentation.Item(candidate)+"\nCompatível com "+(candidate==null?"indisponível":EquipmentPresentation.Role(candidate.Role)))+"\n\n"+
            EquipmentPresentation.Comparison(preview)+"\n\n"+Capacity(view)+"\n"+
            (page.Action==EquipmentAction.Unequip?"Peça removida tem prioridade sobre pendentes; exige vaga prévia.":equipped!=null?"Antiga volta à posição liberada pela nova. Pendentes aguardam.":"Abre uma vaga; o primeiro pendente pode ser recuperado.");
        page.Reason.Text=(updated && preview.CanConfirm?"Condições atualizadas. ":"")+EquipmentPresentation.Reason(preview.BlockReason)+
            (preview.HealthAfter<preview.HealthBefore?$" Vida limitada: {EquipmentPresentation.Number(preview.HealthBefore)} → {EquipmentPresentation.Number(preview.HealthAfter)} PV.":preview.HealthBefore==0?" Continua caído até reiniciar a rota.":"");
        page.Confirm.Text=page.Action==EquipmentAction.Unequip?"Desequipar":
            preview.AttackDelta<0||preview.DefenseDelta<0||preview.HealthDelta<0?"Equipar mesmo com perda":
            equipped!=null && preview.AttackDelta==0 && preview.DefenseDelta==0 && preview.HealthDelta==0?"Trocar peça (mesmos atributos)":"Equipar";
        page.Confirm.TooltipText="Alvo: "+member.Name;
        if(page.Confirm.Disabled && page.Confirm.HasFocus()) page.Back.GrabFocus();
        WireFocus(_menus[origin]);
    }

    private void ConfirmEquipment(int origin,EquipmentIntent intent)
    {
        var page=_equipmentPages[origin];
        if(_equipmentPage!=origin || !EquipmentSelection.IsCurrent(intent))
        {
            // A delayed gesture cannot consume the replacement preview.
            if(_equipmentPage==origin) page.Reason.Text=EquipmentPresentation.Reason(ExpeditionCode.StaleComparison);
            return;
        }
        page.Confirm.Disabled=true;
        var result=EquipmentSelection.Confirm(Simulation,intent);
        if(result==null) return;
        page.Result=true;
        bool cue=EquipmentSelection.AcceptFeedback(result);
        page.Reason.Text=result.Outcome switch
        {
            CommandOutcome.Applied => (intent.Preview.Query.Action==EquipmentAction.Unequip?"Peça devolvida à bolsa":"Peça equipada em "+result.LatestReadModel.Members[_selected].Name)+
                (intent.Preview.AttackDelta==0 && intent.Preview.DefenseDelta==0 && intent.Preview.HealthDelta==0?". Sem mudança nos atributos":". Atributos atualizados; usados nas próximas ações ao continuar")+
                (result.PendingRecovered>0?$". Recuperados da pendência: {result.PendingRecovered}":""),
            CommandOutcome.Replay => "Esta operação já foi aplicada.",
            CommandOutcome.NoChange => "Sem alteração.",
            _ => EquipmentPresentation.Reason(result.Code)
        };
        if(cue) GD.Print("EQUIPMENT APPLIED "+result.OperationId);
        page.Detail.Text="Resultado · "+page.Reason.Text+"\n\n"+EquipmentPresentation.Comparison(intent.Preview)+"\n\n"+
            "Slot final: "+EquipmentPresentation.Item(result.LatestReadModel.Members[_selected].Loadout.Get(page.Slot))+"\n"+Capacity(result.LatestReadModel);
        page.Back.GrabFocus(); RefreshState(result.LatestReadModel); WireFocus(_menus[origin]);
    }

    private static string Capacity(ExpeditionReadModel view)=>$"Bolsa {view.Inventory.Count}/21 · {view.PendingDrops.Count} pendentes";
    private void RefreshEquipment(ExpeditionReadModel view)
    {
        if(_managementButtons[0]==null) return;
        var member=view.Members[_selected];
        for(int menu=0;menu<2;menu++) for(int slot=0;slot<3;slot++)
        {
            var item=member.Loadout.Get((LootSlot)slot);
            _equipmentSlots[menu,slot].Text=EquipmentPresentation.Slot((LootSlot)slot,member.Role)+" · "+(item==null?"Vazio · bônus 0":"Equipada · "+EquipmentPresentation.Item(item));
        }
        string state=view.Management==ExpeditionManagement.Active?"Gerenciamento ativo — "+(view.Phase==ExpeditionPhase.Loot?"descanso suspenso":"rota parada")+(view.Paused?". Ao concluir, continuará pausada":""):
            view.Management==ExpeditionManagement.Requested?"Gerenciamento após a vitória"+(view.Paused?". Retome o combate para alcançar a vitória":""):
            Terminal?view.Phase==ExpeditionPhase.Complete?"Rota concluída":view.Phase==ExpeditionPhase.Defeat?"Equipe derrotada":"Recuo concluído":view.Phase==ExpeditionPhase.Combat?"Em combate — troca indisponível":"Ative o gerenciamento para trocar";
        for(int i=0;i<4;i++)
        {
            _managementLabels[i].Text=state;
            _managementLabels[i].Visible=i>1 || _equipmentPage!=i;
            _managementButtons[i].Text=view.Management==ExpeditionManagement.Active?"Concluir gerenciamento":view.Management==ExpeditionManagement.Requested?"Cancelar pedido":view.Phase==ExpeditionPhase.Combat?"Gerenciar após a vitória":"Gerenciar entre encontros";
            _managementButtons[i].Disabled=Terminal;
        }
        ((Control)_managementLabels[3].GetParent()).Visible=view.Management!=ExpeditionManagement.None;
        _pauseButtons[0].Disabled=view.Management==ExpeditionManagement.Active;
        if(_equipmentPage>=0)
        {
            int origin=_equipmentPage; var page=_equipmentPages[origin];
            page.Identity.Text=$"Alvo: {member.Name} · {EquipmentPresentation.Role(member.Role)} · Nv. {member.Level}\n{(member.Health==0?"Caído":"Vivo")} · {EquipmentPresentation.Slot(page.Slot,member.Role)}";
            if(page.Comparing && !page.Result && (_equipmentRevision!=view.StateRevision || EquipmentSelection.Current==null)) PresentEquipmentPreview(origin,view,true);
            else if(!page.Comparing)
            {
                var item=member.Loadout.Get(page.Slot);
                page.Detail.Text=EquipmentPresentation.Slot(page.Slot,member.Role)+" · "+EquipmentPresentation.Item(item)+"\n"+(item==null?"Escolha uma peça compatível na bolsa.":"Equipada neste integrante. Comparar outra peça não remove esta.");
                page.Choose.Visible=true; page.Choose.Text=item==null?"Escolher na bolsa":"Comparar outra peça";
                page.Remove.Visible=item!=null; page.Confirm.Visible=false; page.Reason.Text=Capacity(view);
            }
        }
        bool identityChanged=_equipmentSession!="" && (_equipmentSession!=view.SessionId || _equipmentAttempt!=view.AttemptId);
        if(identityChanged && _equipmentPage>=0) { var origin=_equipmentPage; EquipmentSelection.Invalidate(); ShowEquipmentPage(origin,false); _equipmentSlots[origin,0].GrabFocus(); }
        _equipmentSession=view.SessionId; _equipmentAttempt=view.AttemptId; _equipmentRevision=view.StateRevision;
    }
}
