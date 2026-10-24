using System;
using System.Collections.Generic;
using System.Linq;

namespace Aetherforge.Domain;

/// <summary>Immutable three-slot loadout. The aggregate alone publishes replacements.</summary>
public sealed class ExpeditionLoadout
{
    public static ExpeditionLoadout Empty { get; } = new ExpeditionLoadout();
    public ExpeditionDrop? Weapon { get; }
    public ExpeditionDrop? Armor { get; }
    public ExpeditionDrop? Accessory { get; }
    internal ExpeditionLoadout(ExpeditionDrop? weapon=null, ExpeditionDrop? armor=null, ExpeditionDrop? accessory=null)
    { Weapon=weapon; Armor=armor; Accessory=accessory; }
    public ExpeditionDrop? Get(LootSlot slot) => slot switch
    {
        LootSlot.Weapon => Weapon, LootSlot.Armor => Armor, LootSlot.Accessory => Accessory,
        _ => throw new ArgumentOutOfRangeException(nameof(slot))
    };
    internal ExpeditionLoadout With(LootSlot slot, ExpeditionDrop? item) => slot switch
    {
        LootSlot.Weapon => new ExpeditionLoadout(item,Armor,Accessory),
        LootSlot.Armor => new ExpeditionLoadout(Weapon,item,Accessory),
        LootSlot.Accessory => new ExpeditionLoadout(Weapon,Armor,item),
        _ => throw new ArgumentOutOfRangeException(nameof(slot))
    };
    internal void Validate(PartyRole role)
    {
        foreach (LootSlot slot in Enum.GetValues(typeof(LootSlot)))
        {
            var item=Get(slot);
            if (item!=null && (item.Role!=role || item.Slot!=slot)) throw new InvalidOperationException("Invalid loadout.");
        }
    }
}

// Canonical registry and locations; reward history never owns a second item.
internal sealed class ExpeditionItems
{
    internal readonly Dictionary<string,ExpeditionDrop> Items = new Dictionary<string,ExpeditionDrop>(StringComparer.Ordinal);
    internal readonly List<string> Bag = new List<string>(), Pending = new List<string>();
    internal readonly ExpeditionLoadout[] Loadouts = { ExpeditionLoadout.Empty, ExpeditionLoadout.Empty, ExpeditionLoadout.Empty };
    internal ExpeditionItems Clone()
    {
        var copy=new ExpeditionItems();
        foreach (var pair in Items) copy.Items.Add(pair.Key,pair.Value);
        copy.Bag.AddRange(Bag); copy.Pending.AddRange(Pending); Array.Copy(Loadouts,copy.Loadouts,3);
        return copy;
    }
    internal void Add(ExpeditionDrop item)
    {
        Items.Add(item.Id,item);
        (Bag.Count<ExpeditionRewards.InventoryCapacity ? Bag : Pending).Add(item.Id);
    }
    internal int Move(int member, LootSlot slot, string? candidate)
    {
        var old=Loadouts[member].Get(slot);
        if (candidate!=null)
        {
            int index=Bag.IndexOf(candidate);
            Bag.RemoveAt(index);
            if (old!=null) Bag.Insert(index,old.Id);
        }
        else if (old!=null) Bag.Add(old.Id);
        Loadouts[member]=Loadouts[member].With(slot,candidate==null ? null : Items[candidate]);
        int recovered=0;
        while (Bag.Count<ExpeditionRewards.InventoryCapacity && Pending.Count>0)
        { Bag.Add(Pending[0]); Pending.RemoveAt(0); recovered++; }
        return recovered;
    }
    internal void Validate()
    {
        if (Bag.Count>ExpeditionRewards.InventoryCapacity) throw new InvalidOperationException("Bag capacity.");
        var locations=new List<string>(Bag); locations.AddRange(Pending);
        for (int i=0;i<3;i++)
        {
            Loadouts[i].Validate((PartyRole)i);
            foreach (LootSlot slot in Enum.GetValues(typeof(LootSlot)))
            {
                var item=Loadouts[i].Get(slot);
                if (item!=null)
                {
                    if (!Items.TryGetValue(item.Id,out var registered) || !ReferenceEquals(item,registered)) throw new InvalidOperationException("Unregistered equipped instance.");
                    locations.Add(item.Id);
                }
            }
        }
        if (locations.Count!=Items.Count || locations.Distinct(StringComparer.Ordinal).Count()!=Items.Count || locations.Any(id=>!Items.ContainsKey(id)))
            throw new InvalidOperationException("Item partition.");
    }
    internal IReadOnlyList<ExpeditionDrop> Project(List<string> ids) => Array.AsReadOnly(ids.Select(id=>Items[id]).ToArray());
}
