using System;
using System.Collections.Generic;

namespace Aetherforge.Domain;

public enum LootRarity { Common, Uncommon, Rare }
public enum LootSlot { Weapon, Armor, Accessory }

/// <summary>Owned session loot; no equipment effects or transfer to the integration inventory yet.</summary>
public sealed class ExpeditionDrop
{
    public string Id { get; }
    public PartyRole Role { get; }
    public LootSlot Slot { get; }
    public LootRarity Rarity { get; }
    public int AttackBonus => Slot == LootSlot.Weapon ? Rarity == LootRarity.Common ? 2 : Rarity == LootRarity.Uncommon ? 3 : 4 : 0;
    public int DefenseBonus => Slot == LootSlot.Armor ? Rarity == LootRarity.Common ? 2 : Rarity == LootRarity.Uncommon ? 3 : 4 : 0;
    public int HealthBonus => Slot == LootSlot.Accessory ? Rarity == LootRarity.Common ? 10 : Rarity == LootRarity.Uncommon ? 15 : 22 : 0;
    public string Name => Slot == LootSlot.Armor ? "Couraça de Latão" : Slot == LootSlot.Accessory ? "Medalhão de Éter"
        : Role == PartyRole.Tank ? "Lâmina da Caldeira" : Role == PartyRole.Dps ? "Rifle dos Trilhos" : "Foco de Éter";
    internal ExpeditionDrop(string id, PartyRole role, LootSlot slot, LootRarity rarity)
    { Id=id; Role=role; Slot=slot; Rarity=rarity; }
}

public sealed class ExpeditionReward
{
    public long Sequence { get; }
    public int StageIndex { get; }
    public int Coins { get; }
    public int Scrap { get; }
    public int Components { get; }
    public IReadOnlyList<int> ExperienceGranted { get; }
    public IReadOnlyList<int> LevelsGained { get; }
    public ExpeditionDrop? Drop { get; }
    internal ExpeditionReward(long sequence, int stage, int coins, int scrap, int components, int[] experience, int[] levels, ExpeditionDrop? drop)
    {
        Sequence=sequence; StageIndex=stage; Coins=coins; Scrap=scrap; Components=components;
        ExperienceGranted=Array.AsReadOnly(experience); LevelsGained=Array.AsReadOnly(levels); Drop=drop;
    }
}

/// <summary>Session reward ledger. Seeded RNG advances only for a committed victory.</summary>
public sealed class ExpeditionRewards
{
    public const uint DefaultSeed = 0xA37F019;
    public const int InventoryCapacity = 21;
    private readonly uint _seed;
    private uint _random;
    private long _nextItem;
    private readonly List<ExpeditionReward> _receipts = new List<ExpeditionReward>();
    private readonly List<ExpeditionDrop> _inventory = new List<ExpeditionDrop>(), _pending = new List<ExpeditionDrop>();
    private readonly bool[] _credited = new bool[5];
    public IReadOnlyList<ExpeditionReward> Receipts { get; }
    public IReadOnlyList<ExpeditionDrop> Inventory { get; }
    public IReadOnlyList<ExpeditionDrop> PendingDrops { get; }
    public long Coins { get; private set; }
    public long Scrap { get; private set; }
    public long Components { get; private set; }
    public uint RandomState => _random;
    public ExpeditionReward? LastReward => _receipts.Count == 0 ? null : _receipts[_receipts.Count-1];

    public ExpeditionRewards(uint seed = DefaultSeed)
    {
        _seed=seed; _random=seed;
        Receipts=_receipts.AsReadOnly(); Inventory=_inventory.AsReadOnly(); PendingDrops=_pending.AsReadOnly();
    }
    public void BeginAttempt() => Array.Clear(_credited, 0, _credited.Length);
    public void Reset()
    {
        _random=_seed; _nextItem=0; Coins=Scrap=Components=0;
        _receipts.Clear(); _inventory.Clear(); _pending.Clear(); BeginAttempt();
    }
    internal ExpeditionReward? CreditVictory(int stage, List<PartyMember> party)
    {
        if (_credited[stage]) return null;
        int[] xp={12,16,20,24,50}, coins={8,10,12,14,35}, scrap={1,1,2,2,5};
        var granted = new int[party.Count]; var levels = new int[party.Count];
        for (int i=0; i<party.Count; i++)
        {
            int before=party[i].Progression.TotalExperience;
            levels[i]=party[i].GainExperience(xp[stage]);
            granted[i]=party[i].Progression.TotalExperience-before;
        }
        ExpeditionDrop? drop=null;
        if (stage==4 || Next(100)<10)
        {
            int roll=Next(100);
            var rarity=roll<75 ? LootRarity.Common : roll<95 ? LootRarity.Uncommon : LootRarity.Rare;
            drop=new ExpeditionDrop("local-"+(++_nextItem).ToString("D6"), (PartyRole)Next(3), (LootSlot)Next(3), rarity);
            if (_inventory.Count < InventoryCapacity) _inventory.Add(drop); else _pending.Add(drop);
        }
        Coins+=coins[stage]; Scrap+=scrap[stage]; int components=stage==4 ? 1 : 0; Components+=components;
        var receipt=new ExpeditionReward(_receipts.Count+1L, stage, coins[stage], scrap[stage], components, granted, levels, drop);
        _receipts.Add(receipt); _credited[stage]=true;
        return receipt;
    }
    private int Next(uint bound)
    {
        uint value, threshold=unchecked(0u-bound)%bound;
        do { _random=unchecked(_random*1664525u+1013904223u); value=_random; } while (value<threshold);
        return (int)(value%bound);
    }
}
