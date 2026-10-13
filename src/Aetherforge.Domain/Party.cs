using System;
using System.Collections.Generic;

namespace Aetherforge.Domain;

public enum PartyRole { Tank, Dps, Healer }

public sealed class PartyMember
{
    public PartyRole Role { get; }
    public string Name { get; }
    public string Model { get; }
    public float MaxHealth { get; }
    public float Health { get; set; }
    public float Attack { get; }
    public float Defense { get; }
    public PartyMember(PartyRole role, string name, string model, float hp, float attack, float defense)
    {
        Role = role; Name = name; Model = model; MaxHealth = hp; Health = hp; Attack = attack; Defense = defense;
    }
}

public static class StarterParty
{
    public static List<PartyMember> Create() => new()
    {
        new(PartyRole.Tank, "Guarda da Caldeira", "Knight", 180, 8, 10),
        new(PartyRole.Dps, "Pistoleiro", "Rogue", 100, 18, 4),
        new(PartyRole.Healer, "Alquimista do Éter", "Mage", 110, 6, 5)
    };
}

/// <summary>Visual combat demonstration for M0. No rewards, XP or persistent economy.</summary>
public sealed class CombatPreview
{
    public List<PartyMember> Party { get; } = StarterParty.Create();
    public const float EnemyMaxHealth = 260;
    public float EnemyHealth { get; private set; } = EnemyMaxHealth;
    public int Encounters { get; private set; }
    public string LastEvent { get; private set; } = "Equipe em marcha";
    private int _tick;

    public CombatSnapshot Snapshot() => new() { Tick = _tick, Encounters = Encounters, EnemyHealth = EnemyHealth, Health = Party.ConvertAll(p => p.Health).ToArray(), LastEvent = LastEvent };

    public void ApplySnapshot(CombatSnapshot state)
    {
        _tick = state.Tick; Encounters = state.Encounters; EnemyHealth = state.EnemyHealth; LastEvent = state.LastEvent;
        for (var i = 0; i < Party.Count; i++) Party[i].Health = state.Health[i];
    }

    public void Step()
    {
        _tick++;
        if (EnemyHealth <= 0)
        {
            EnemyHealth = EnemyMaxHealth; Encounters++;
            foreach (var member in Party) member.Health = member.MaxHealth;
            LastEvent = "Novo encontro";
            return;
        }
        var tank = Party[0];
        var target = tank.Health > 0 ? tank : Party.Find(p => p.Health > 0);
        if (target == null)
        {
            foreach (var member in Party) member.Health = member.MaxHealth;
            EnemyHealth = EnemyMaxHealth;
            LastEvent = "Equipe recuperada";
            return;
        }
        target.Health = Math.Max(0, target.Health - Math.Max(1, 19 - target.Defense));
        foreach (var member in Party)
            if (member.Health > 0) EnemyHealth = Math.Max(0, EnemyHealth - Math.Max(1, member.Attack - 3));
        if (_tick % 4 == 0 && Party[2].Health > 0)
        {
            var hurt = Party.Find(p => p.Health > 0 && p.Health < p.MaxHealth * .75f);
            if (hurt != null)
            {
                hurt.Health = Math.Min(hurt.MaxHealth, hurt.Health + 30);
                LastEvent = "Infusão Etérea";
                return;
            }
        }
        LastEvent = EnemyHealth == 0 ? "Encontro vencido" : "Tank protege a equipe";
    }
}

public static class MovementRules
{
    public const float Speed = 4;
    public static (float X, float Y) Validate(float x, float y)
    {
        if (float.IsNaN(x) || float.IsInfinity(x) || float.IsNaN(y) || float.IsInfinity(y)) return (0, 0);
        var length = Math.Sqrt(x * x + y * y);
        return length > 1 ? ((float)(x / length), (float)(y / length)) : (x, y);
    }
}
