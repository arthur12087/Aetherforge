using System;

namespace Aetherforge.Domain;

/// <summary>One calculation for combat, comparison and presentation. Gear is added after level rounding.</summary>
public sealed class EffectiveStats
{
    public float BaseHealth { get; }
    public float BaseAttack { get; }
    public float BaseDefense { get; }
    public float LevelHealth { get; }
    public float LevelAttack { get; }
    public float LevelDefense { get; }
    public int HealthBonus { get; }
    public int AttackBonus { get; }
    public int DefenseBonus { get; }
    public float MaxHealth => LevelHealth + HealthBonus;
    public float Attack => LevelAttack + AttackBonus;
    public float Defense => LevelDefense + DefenseBonus;
    public float InfusionPotential { get; }
    internal EffectiveStats(PartyMember member, ExpeditionLoadout loadout)
    {
        BaseHealth=member.BaseHealth; BaseAttack=member.BaseAttack; BaseDefense=member.BaseDefense;
        LevelHealth=member.Progression.ScaleBase(BaseHealth,10);
        LevelAttack=member.Progression.ScaleBase(BaseAttack,6);
        LevelDefense=member.Progression.ScaleBase(BaseDefense,5);
        HealthBonus=loadout.Accessory?.HealthBonus ?? 0;
        AttackBonus=loadout.Weapon?.AttackBonus ?? 0;
        DefenseBonus=loadout.Armor?.DefenseBonus ?? 0;
        InfusionPotential=member.Role==PartyRole.Healer ? member.Progression.ScaleBase(ExpeditionSimulation.InfusionAmount,8)+AttackBonus : 0;
    }
}

public static class ExpeditionStats
{
    public static EffectiveStats Calculate(PartyMember member, ExpeditionLoadout? loadout = null)
    {
        if (member == null) throw new ArgumentNullException(nameof(member));
        var gear=loadout ?? member.Loadout;
        gear.Validate(member.Role);
        return new EffectiveStats(member,gear);
    }
}
