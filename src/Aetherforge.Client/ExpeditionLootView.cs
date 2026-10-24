using Aetherforge.Domain;
using Godot;
using System;

/// <summary>Read-only session bag. Item inspection is functional; equipping is a later increment.</summary>
public partial class ExpeditionLootView : VBoxContainer
{
    public GridContainer Bag { get; private set; } = null!;
    public string SelectedItemId { get; private set; } = "";
    public event Action? InventoryChanged;
    private ExpeditionSimulation _simulation = null!;
    private readonly Button[] _slots = new Button[ExpeditionRewards.InventoryCapacity];
    private readonly Control[] _empty = new Control[ExpeditionRewards.InventoryCapacity];
    private Label _balances = null!, _capacity = null!, _detail = null!;
    private int _renderedCount = -1;
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
            int slot=i;
            var button=BrassUi.Button("", ()=>Inspect(slot)); button.Name="LootSlot"+i;
            button.ExpandIcon=true; button.AddThemeFontSizeOverride("font_size", BrassUi.S(12));
            button.AddThemeConstantOverride("icon_max_width", BrassUi.S(12)); button.AddThemeConstantOverride("h_separation", BrassUi.S(1));
            button.CustomMinimumSize=new Vector2(0, BrassUi.S(28)); button.SizeFlagsHorizontal=SizeFlags.ExpandFill;
            foreach(var state in new[] { "normal", "hover", "pressed", "hover_pressed", "disabled" })
                button.AddThemeStyleboxOverride(state, BrassUi.Box(state=="hover" ? BrassUi.Linen : BrassUi.Paper, BrassUi.Tobacco, state=="disabled"?0:1, 0, 0));
            _slots[i]=button; Bag.AddChild(button);
            var engraving=new BrassDossierEmptySlot(); button.AddChild(engraving); BrassUi.FullRect(engraving); _empty[i]=engraving;
        }
        Bag.Resized+=()=>
        {
            // GridContainer lays out in integer pixels; fractional minima can escape its final row.
            float side=Math.Max(1,(float)Math.Floor((Bag.Size.X-6*Bag.GetThemeConstant("h_separation"))/7));
            foreach(var slot in _slots) if(Math.Abs(slot.CustomMinimumSize.Y-side)>1) slot.CustomMinimumSize=new Vector2(0,side);
        };
        _detail=BrassUi.Wrap("Selecione um item para inspecionar.", 14, BrassUi.Soot); _detail.Name="LootItemDetail"; AddChild(_detail);
        AddChild(BrassUi.Wrap("Ganhos desta sessão · perdidos ao fechar. Equipar será a próxima etapa.", 13, BrassUi.Tobacco));
        UpdateState();
    }
    public void UpdateState()
    {
        var rewards=_simulation.Rewards;
        Set(_balances,$"{rewards.Coins} moedas · {rewards.Scrap} sucatas\n{rewards.Components} componentes brutos");
        Set(_capacity,$"Bolsa · {rewards.Inventory.Count} / {ExpeditionRewards.InventoryCapacity}"+(rewards.PendingDrops.Count>0 ? $" · {rewards.PendingDrops.Count} itens pendentes, sem espaço" : ""));
        if(_renderedCount==rewards.Inventory.Count) return;
        _renderedCount=rewards.Inventory.Count;
        if(_renderedCount==0) { SelectedItemId=""; Set(_detail,"Selecione um item para inspecionar."); }
        for(int i=0;i<_slots.Length;i++)
        {
            var button=_slots[i]; bool occupied=i<rewards.Inventory.Count;
            button.Disabled=!occupied; button.FocusMode=occupied?FocusModeEnum.All:FocusModeEnum.None; _empty[i].Visible=!occupied;
            if(!occupied) { button.Text=""; button.Icon=null; button.TooltipText="Espaço vazio"; continue; }
            var item=rewards.Inventory[i];
            button.Text=item.Rarity==LootRarity.Common ? "C" : item.Rarity==LootRarity.Uncommon ? "I" : "R";
            button.Icon=BrassUi.Icon(item.Slot==LootSlot.Armor ? "shield" : item.Slot==LootSlot.Accessory ? "vital" : item.Role==PartyRole.Dps ? "rifle" : item.Role==PartyRole.Healer ? "heal" : "shield");
            var color=item.Rarity==LootRarity.Rare ? BrassUi.Wine : item.Rarity==LootRarity.Uncommon ? BrassUi.Tobacco : BrassUi.Soot;
            button.AddThemeColorOverride("font_color",color); button.AddThemeColorOverride("icon_normal_color",color);
            foreach(var state in new[] { "font_hover_color", "font_pressed_color", "font_focus_color", "font_hover_pressed_color", "icon_hover_color", "icon_pressed_color", "icon_focus_color", "icon_hover_pressed_color" })
                button.AddThemeColorOverride(state,BrassUi.Soot);
            button.TooltipText=Describe(item);
        }
        InventoryChanged?.Invoke();
    }
    private async void Inspect(int index)
    {
        if(index>=_simulation.Rewards.Inventory.Count) return;
        var item=_simulation.Rewards.Inventory[index]; SelectedItemId=item.Id;
        Set(_detail,Describe(item));
        // Let wrapped text settle before exposing it in short or enlarged windows.
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        if(!IsInsideTree())return;
        for(Node? parent=GetParent();parent!=null && parent is not Window;parent=parent.GetParent())
            if(parent is ScrollContainer scroll) { scroll.EnsureControlVisible(_detail); break; }
    }
    public static string Describe(ExpeditionDrop item)
    {
        string rarity=item.Rarity==LootRarity.Common?"Comum":item.Rarity==LootRarity.Uncommon?"Incomum":"Raro";
        string role=item.Role==PartyRole.Tank?"Guarda":item.Role==PartyRole.Dps?"Pistoleiro":"Alquimista";
        string bonus=item.AttackBonus>0?$"+{item.AttackBonus} ataque"+(item.Role==PartyRole.Healer?$" / +{item.AttackBonus} cura":"") :item.DefenseBonus>0?$"+{item.DefenseBonus} defesa":$"+{item.HealthBonus} vida";
        return $"{item.Name} · {rarity}\nCompatível com {role} · {bonus}\nGuardado na bolsa. Bônus ainda não aplicado.";
    }
    private static void Set(Label label,string text) { if(label.Text!=text)label.Text=text; }
}
