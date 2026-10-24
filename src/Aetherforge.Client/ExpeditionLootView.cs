using Aetherforge.Domain;
using Godot;
using System;
using System.Linq;

/// <summary>Session bag projected by revision. Inspection never moves a piece.</summary>
public partial class ExpeditionLootView : VBoxContainer
{
    public GridContainer Bag { get; private set; } = null!;
    public string SelectedItemId { get; private set; } = "";
    public event Action? InventoryChanged;
    public event Action? InspectionChanged;
    public event Action<string>? CompareRequested;
    private ExpeditionSimulation _simulation = null!;
    private readonly Button[] _slots = new Button[ExpeditionRewards.InventoryCapacity];
    private readonly Control[] _empty = new Control[ExpeditionRewards.InventoryCapacity];
    private Label _balances = null!, _capacity = null!, _detail = null!;
    private long _renderedRevision = -1;
    private string _session="";
    private Button _compare=null!;
    private long _inspectionEpoch;
    private readonly Action?[] _inspectHandlers=new Action?[ExpeditionRewards.InventoryCapacity];
    public void Bind(ExpeditionSimulation simulation) => _simulation=simulation;
    public override void _Ready()
    {
        Name="SessionLoot"; SizeFlagsHorizontal=SizeFlags.ExpandFill;
        AddThemeConstantOverride("separation", BrassUi.S(6));
        _balances=BrassUi.Wrap("", 14, BrassUi.Soot); _balances.Name="SessionBalances"; AddChild(_balances);
        _capacity=BrassUi.Wrap("", 13, BrassUi.Tobacco); AddChild(_capacity);
        Bag=new GridContainer { Name="EquipmentBag", Columns=7, SizeFlagsHorizontal=SizeFlags.ExpandFill }; AddChild(Bag);
        for (int i=0; i<_slots.Length; i++)
        {
            var button=BrassUi.Button("", ()=>{}); button.Name="LootSlot"+i;
            button.ExpandIcon=true; button.AddThemeFontSizeOverride("font_size", BrassUi.S(12));
            button.AddThemeConstantOverride("icon_max_width", BrassUi.S(12)); button.AddThemeConstantOverride("h_separation", BrassUi.S(1));
            button.CustomMinimumSize=new Vector2(0, BrassUi.S(36)); button.SizeFlagsHorizontal=SizeFlags.ExpandFill;
            foreach(var state in new[] { "normal", "hover", "pressed", "hover_pressed", "disabled" })
                button.AddThemeStyleboxOverride(state, BrassUi.Box(state=="hover" ? BrassUi.Linen : BrassUi.Paper, BrassUi.Tobacco, state=="disabled"?0:1, 0, 0));
            _slots[i]=button; Bag.AddChild(button);
            var engraving=new BrassDossierEmptySlot(); button.AddChild(engraving); BrassUi.FullRect(engraving); _empty[i]=engraving;
        }
        Bag.Resized+=()=>
        {
            // GridContainer lays out in integer pixels; fractional minima can escape its final row.
            int columns=Math.Clamp((int)(Bag.Size.X/(BrassUi.S(36)+Bag.GetThemeConstant("h_separation"))),1,7);
            Bag.Columns=columns;
            float side=Math.Max(BrassUi.S(36),(float)Math.Floor((Bag.Size.X-(columns-1)*Bag.GetThemeConstant("h_separation"))/columns));
            foreach(var slot in _slots) if(Math.Abs(slot.CustomMinimumSize.Y-side)>1) slot.CustomMinimumSize=new Vector2(0,side);
            WireGrid();
        };
        _detail=BrassUi.Wrap("Selecione um item para inspecionar.", 14, BrassUi.Soot); _detail.Name="LootItemDetail"; AddChild(_detail);
        _compare=EquipmentPresentation.Button("CompareCandidate","Comparar",()=>CompareRequested?.Invoke(SelectedItemId));
        _compare.Visible=false; AddChild(_compare);
        AddChild(BrassUi.Wrap(EquipmentPresentation.SessionWarning, 13, BrassUi.Tobacco));
        UpdateState();
    }
    public void UpdateState(ExpeditionReadModel? view=null)
    {
        var rewards=view??_simulation.ReadView();
        Set(_balances,$"{rewards.Coins} moedas · {rewards.Scrap} sucatas\n{rewards.Components} componentes brutos");
        Set(_capacity,$"Bolsa · {rewards.Inventory.Count} / {ExpeditionRewards.InventoryCapacity}"+(rewards.PendingDrops.Count>0 ? $" · {rewards.PendingDrops.Count} itens pendentes, sem espaço" : ""));
        if(_renderedRevision==rewards.InventoryRevision && _session==rewards.SessionId) return;
        bool focused=_slots.Any(x=>x.HasFocus());
        string focusId=_slots.FirstOrDefault(x=>x.HasFocus())?.GetMeta("item_id","").AsString()??"";
        _renderedRevision=rewards.InventoryRevision; _session=rewards.SessionId;
        for(int i=0;i<_slots.Length;i++)
        {
            var button=_slots[i]; bool occupied=i<rewards.Inventory.Count;
            if(_inspectHandlers[i]!=null) { button.Pressed-=_inspectHandlers[i]; _inspectHandlers[i]=null; }
            button.Disabled=!occupied; button.FocusMode=occupied?FocusModeEnum.All:FocusModeEnum.None; _empty[i].Visible=!occupied;
            if(!occupied) { button.Text=""; button.Icon=null; button.TooltipText="Espaço vazio"; continue; }
            var item=rewards.Inventory[i];
            string id=item.Id; _inspectHandlers[i]=()=>Inspect(id); button.Pressed+=_inspectHandlers[i];
            button.SetMeta("item_id",id);
            button.Text=item.Rarity==LootRarity.Common ? "C" : item.Rarity==LootRarity.Uncommon ? "I" : "R";
            button.Icon=BrassUi.Icon(item.Slot==LootSlot.Armor ? "shield" : item.Slot==LootSlot.Accessory ? "vital" : item.Role==PartyRole.Dps ? "rifle" : item.Role==PartyRole.Healer ? "heal" : "shield");
            var color=BrassUi.Soot;
            button.AddThemeColorOverride("font_color",color); button.AddThemeColorOverride("icon_normal_color",color);
            foreach(var state in new[] { "font_hover_color", "font_pressed_color", "font_focus_color", "font_hover_pressed_color", "icon_hover_color", "icon_pressed_color", "icon_focus_color", "icon_hover_pressed_color" })
                button.AddThemeColorOverride(state,BrassUi.Soot);
            button.TooltipText=Describe(item);
        }
        UpdateInspection(rewards);
        WireGrid();
        if(focused && !FocusItem(focusId)) _slots.FirstOrDefault(x=>!x.Disabled)?.GrabFocus();
        InventoryChanged?.Invoke();
    }
    private void WireGrid()
    {
        int count=_simulation.ReadView().Inventory.Count;
        for(int i=0;i<count;i++)
        {
            var b=_slots[i]; int c=Bag.Columns;
            b.FocusNeighborLeft=_slots[i%c>0?i-1:i].GetPath();
            b.FocusNeighborRight=_slots[i%c<c-1 && i+1<count?i+1:i].GetPath();
            b.FocusNeighborTop=_slots[i>=c?i-c:i].GetPath();
            b.FocusNeighborBottom=_slots[i+c<count?i+c:i].GetPath();
        }
    }
    private void UpdateInspection(ExpeditionReadModel view)
    {
        var equipped=view.Members.SelectMany(m=>new[]{m.Loadout.Weapon,m.Loadout.Armor,m.Loadout.Accessory}).FirstOrDefault(x=>x?.Id==SelectedItemId);
        var item=view.Inventory.FirstOrDefault(x=>x.Id==SelectedItemId)??equipped;
        _compare.Visible=item!=null; _compare.Disabled=equipped!=null;
        Set(_detail,item==null?"Selecione um item para inspecionar.":Describe(item)+(equipped==null?"\nGuardada na bolsa.":"\nEquipada. Bônus aplicado ao integrante."));
    }
    public bool FocusItem(string id)
    {
        var view=_simulation.ReadView(); int index=view.Inventory.ToList().FindIndex(x=>x.Id==id);
        if(index<0)return false;
        SelectedItemId=id; UpdateInspection(view); _slots[index].GrabFocus(); return true;
    }
    public void FocusCompatible(PartyRole role,LootSlot slot)
    {
        ClearInspection();
        var inventory=_simulation.ReadView().Inventory;
        int index=inventory.ToList().FindIndex(x=>x.Role==role && x.Slot==slot);
        if(index>=0) _slots[index].GrabFocus();
        else for(Node? parent=GetParent();parent!=null;parent=parent.GetParent()) if(parent is ScrollContainer scroll){scroll.GrabFocus();break;}
    }
    public bool ClearInspection()
    {
        if(SelectedItemId.Length==0) return false;
        string id=SelectedItemId; SelectedItemId=""; _inspectionEpoch++; UpdateInspection(_simulation.ReadView());
        int index=_simulation.ReadView().Inventory.ToList().FindIndex(x=>x.Id==id);
        if(index>=0)_slots[index].GrabFocus();
        InspectionChanged?.Invoke(); return true;
    }
    private async void Inspect(string id)
    {
        var view=_simulation.ReadView(); if(!view.Inventory.Any(x=>x.Id==id))return;
        SelectedItemId=id; long epoch=++_inspectionEpoch; InspectionChanged?.Invoke(); UpdateInspection(view);
        InventoryChanged?.Invoke();
        // Let wrapped text settle before exposing it in short or enlarged windows.
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        if(!IsInsideTree() || !IsVisibleInTree() || epoch!=_inspectionEpoch || SelectedItemId!=id)return;
        for(Node? parent=GetParent();parent!=null && parent is not Window;parent=parent.GetParent())
            if(parent is ScrollContainer scroll) { scroll.EnsureControlVisible(_detail); break; }
        _compare.GrabFocus();
    }
    public static string Describe(ExpeditionDrop item)
    {
        string rarity=item.Rarity==LootRarity.Common?"Comum":item.Rarity==LootRarity.Uncommon?"Incomum":"Raro";
        string role=item.Role==PartyRole.Tank?"Guarda":item.Role==PartyRole.Dps?"Pistoleiro":"Alquimista";
        string bonus=item.AttackBonus>0?$"+{item.AttackBonus} ataque"+(item.Role==PartyRole.Healer?$" / +{item.AttackBonus} potência da infusão":"") :item.DefenseBonus>0?$"+{item.DefenseBonus} defesa":$"+{item.HealthBonus} vida";
        return $"{item.Name} · {rarity}\nCompatível com {role} · {bonus}";
    }
    private static void Set(Label label,string text) { if(label.Text!=text)label.Text=text; }
}
