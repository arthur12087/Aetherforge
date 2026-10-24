using System;
using System.Collections.Generic;

namespace Aetherforge.Domain;

public enum LootRarity { Common, Uncommon, Rare }
public enum LootSlot { Weapon, Armor, Accessory }

/// <summary>Immutable session item. Never transferred to the separate integration inventory.</summary>
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
    {
        if (string.IsNullOrEmpty(id) || !Enum.IsDefined(typeof(PartyRole),role) ||
            !Enum.IsDefined(typeof(LootSlot),slot) || !Enum.IsDefined(typeof(LootRarity),rarity))
            throw new ArgumentException("Invalid expedition item.");
        Id=id; Role=role; Slot=slot; Rarity=rarity;
    }
}

public sealed class ExpeditionReward
{
    public long Sequence { get; }
    public int StageIndex { get; }
    public long AttemptId { get; }
    public int Coins { get; }
    public int Scrap { get; }
    public int Components { get; }
    public IReadOnlyList<int> ExperienceGranted { get; }
    public IReadOnlyList<int> LevelsGained { get; }
    public ExpeditionDrop? Drop { get; }
    internal ExpeditionReward(long sequence, long attempt, int stage, int coins, int scrap, int components, int[] experience, int[] levels, ExpeditionDrop? drop)
    {
        Sequence=sequence; AttemptId=attempt; StageIndex=stage; Coins=coins; Scrap=scrap; Components=components;
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
    internal ExpeditionItems Items = new ExpeditionItems();
    private readonly bool[] _credited = new bool[5];
    public IReadOnlyList<ExpeditionReward> Receipts { get; }
    public IReadOnlyList<ExpeditionDrop> Inventory => Items.Project(Items.Bag);
    public IReadOnlyList<ExpeditionDrop> PendingDrops => Items.Project(Items.Pending);
    public long Coins { get; private set; }
    public long Scrap { get; private set; }
    public long Components { get; private set; }
    public uint RandomState => _random;
    public uint Seed => _seed;
    public long LastIssuedItemSequence => _nextItem;
    public ExpeditionReward? LastReward => _receipts.Count == 0 ? null : _receipts[_receipts.Count-1];

    public ExpeditionRewards(uint seed = DefaultSeed)
    {
        _seed=seed; _random=seed;
        Receipts=_receipts.AsReadOnly();
    }
    internal void BeginAttempt() => Array.Clear(_credited, 0, _credited.Length);
    internal void Reset()
    {
        _random=_seed; _nextItem=0; Coins=Scrap=Components=0;
        _receipts.Clear(); Items=new ExpeditionItems(); BeginAttempt();
    }
    internal ExpeditionReward? CreditVictory(int stage, List<PartyMember> party, long attempt)
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
            _nextItem=checked(_nextItem+1);
            drop=new ExpeditionDrop("local-"+_nextItem.ToString("D6",System.Globalization.CultureInfo.InvariantCulture), (PartyRole)Next(3), (LootSlot)Next(3), rarity);
            Items.Add(drop);
        }
        Coins=checked(Coins+coins[stage]); Scrap=checked(Scrap+scrap[stage]); int components=stage==4 ? 1 : 0; Components=checked(Components+components);
        var receipt=new ExpeditionReward(checked(_receipts.Count+1L), attempt, stage, coins[stage], scrap[stage], components, granted, levels, drop);
        _receipts.Add(receipt); _credited[stage]=true;
        return receipt;
    }
    internal ExpeditionRewards Clone()
    {
        var copy=new ExpeditionRewards(_seed) { _random=_random, _nextItem=_nextItem, Coins=Coins, Scrap=Scrap, Components=Components, Items=Items.Clone() };
        copy._receipts.AddRange(_receipts); Array.Copy(_credited,copy._credited,5);
        return copy;
    }
    internal IReadOnlyList<bool> Credited => Array.AsReadOnly((bool[])_credited.Clone());
    internal void Validate(long attempt)
    {
        Items.Validate();
        if (Coins<0 || Scrap<0 || Components<0 || _nextItem<0) throw new InvalidOperationException("Invalid reward counters.");
        foreach (var pair in Items.Items)
        {
            var id=pair.Key;
            if (id!=pair.Value.Id || !id.StartsWith("local-",StringComparison.Ordinal) ||
                !long.TryParse(id.Substring(6),System.Globalization.NumberStyles.None,System.Globalization.CultureInfo.InvariantCulture,out long sequence) ||
                sequence<1 || sequence>_nextItem || id!="local-"+sequence.ToString("D6",System.Globalization.CultureInfo.InvariantCulture))
                throw new InvalidOperationException("Invalid issued item identity.");
        }
        var keys=new HashSet<string>(StringComparer.Ordinal);
        var current=new bool[5];
        for (int i=0;i<_receipts.Count;i++)
        {
            var receipt=_receipts[i];
            if (receipt.Sequence!=i+1L || receipt.AttemptId<1 || receipt.AttemptId>attempt || receipt.StageIndex<0 || receipt.StageIndex>4 ||
                !keys.Add(receipt.AttemptId+":"+receipt.StageIndex) || receipt.ExperienceGranted.Count!=3 || receipt.LevelsGained.Count!=3)
                throw new InvalidOperationException("Invalid victory ledger.");
            if (receipt.AttemptId==attempt) current[receipt.StageIndex]=true;
            if (receipt.Drop!=null && (!Items.Items.TryGetValue(receipt.Drop.Id,out var item) || !ReferenceEquals(item,receipt.Drop)))
                throw new InvalidOperationException("Invalid historical drop.");
        }
        for (int i=0;i<5;i++) if (_credited[i]!=current[i]) throw new InvalidOperationException("Invalid current credit keys.");
    }
    private int Next(uint bound)
    {
        uint value, threshold=unchecked(0u-bound)%bound;
        do { _random=unchecked(_random*1664525u+1013904223u); value=_random; } while (value<threshold);
        return (int)(value%bound);
    }
}
