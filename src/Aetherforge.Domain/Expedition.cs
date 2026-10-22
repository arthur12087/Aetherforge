using System;
using System.Collections.Generic;

namespace Aetherforge.Domain;

public enum ExpeditionPhase { March, Combat, Loot, Complete, Defeat, Retreated }

public sealed class ExpeditionEnemy
{
    public string Kind { get; }
    public string Name { get; }
    public float MaxHealth { get; }
    public float Health { get; internal set; }

    internal ExpeditionEnemy(string kind, string name, float health)
    {
        Kind = kind;
        Name = name;
        MaxHealth = Health = health;
    }
}

public sealed class ExpeditionEvent
{
    public int Id { get; }
    public string Kind { get; }
    public int Actor { get; }
    public int Target { get; }
    public float Amount { get; }
    public double At { get; }
    public string Message { get; }

    internal ExpeditionEvent(int id, string kind, int actor, int target, float amount, double at, string message)
    {
        Id = id; Kind = kind; Actor = actor; Target = target;
        Amount = amount; At = at; Message = message;
    }
}

/// <summary>
/// Local, deterministic route; no rewards or persistent state. Advance consumes all valid
/// time until a terminal state, in steps of at most 50 ms and at exact action boundaries.
/// March speed is 2.4 units/s. Loot is a two-second rest, including treatment of living
/// members by the living healer at its end. Reset starts a new session (including event IDs).
/// </summary>
public sealed class ExpeditionSimulation
{
    private static readonly string[] StageNames = { "Entrada", "Trilhos", "Pressão", "Ninho", "Fornalha" };
    private static readonly double[] Legs = { 16, 22, 18, 24, 28 };
    private static readonly double[] HeroIntervals = { 1.6, 1.25, 2.4 };
    private const int EventLimit = 128;
    private const double Speed = 2.4;
    private readonly List<ExpeditionEnemy> _enemies = new List<ExpeditionEnemy>();
    private readonly List<ExpeditionEvent> _events = new List<ExpeditionEvent>();
    private readonly IReadOnlyList<ExpeditionEnemy> _enemyView;
    private readonly IReadOnlyList<ExpeditionEvent> _eventView;
    private readonly double[] _heroNext = new double[3];
    private readonly double[] _enemyNext = new double[2];
    private readonly int[] _enemySwings = new int[2];
    private double _phaseBeganAt;
    private double _phaseEndsAt;
    private double _marchFrom;
    private double _shieldUntil;
    private double _shieldReadyAt;
    private double _healReadyAt;
    private int _nextEventId;

    public List<PartyMember> Party { get; } = StarterParty.Create();
    public IReadOnlyList<ExpeditionEnemy> Enemies => _enemyView;
    public IReadOnlyList<ExpeditionEvent> Events => _eventView;
    public ExpeditionPhase Phase { get; private set; }
    public int StageIndex { get; private set; }
    public string StageName => StageNames[StageIndex];
    public double Distance { get; private set; }
    public double EncounterAt { get; private set; }
    public double Elapsed { get; private set; }
    public double PhaseTime => Elapsed - _phaseBeganAt;
    public int Victories { get; private set; }
    public bool Paused { get; set; }
    public double ShieldCooldown => Math.Max(0, _shieldReadyAt - Elapsed);
    public double ShieldRemaining => Math.Max(0, _shieldUntil - Elapsed);
    public double HealCooldown => Math.Max(0, _healReadyAt - Elapsed);
    public string LastEvent { get; private set; } = "";

    public ExpeditionSimulation()
    {
        _enemyView = _enemies.AsReadOnly();
        _eventView = _events.AsReadOnly();
        Reset();
    }

    public void Reset()
    {
        Party.Clear();
        Party.AddRange(StarterParty.Create());
        _events.Clear();
        _nextEventId = 0;
        StageIndex = Victories = 0;
        Elapsed = Distance = _phaseBeganAt = _phaseEndsAt = 0;
        _shieldUntil = _shieldReadyAt = _healReadyAt = 0;
        Paused = false;
        Array.Clear(_heroNext, 0, _heroNext.Length);
        BeginMarch();
    }

    /// <exception cref="ArgumentOutOfRangeException">Delta must be finite and nonnegative, even while paused.</exception>
    public void Advance(double delta)
    {
        if (double.IsNaN(delta) || double.IsInfinity(delta) || delta < 0)
            throw new ArgumentOutOfRangeException(nameof(delta), "Delta must be finite and nonnegative.");
        if (Paused || Terminal || delta == 0) return;
        var until = Elapsed + delta;
        // Overflow of the horizon is harmless: the finite route reaches a terminal state first.
        while (!Terminal && Elapsed < until)
        {
            ClampParty();
            if (FirstLivingHero() < 0)
            {
                ChangePhase(ExpeditionPhase.Defeat, "A equipe caiu");
                break;
            }

            var next = Math.Min(until, Elapsed + .05);
            if (Phase == ExpeditionPhase.Combat)
            {
                for (var i = 0; i < 3; i++)
                    if (Party[i].Health > 0) next = Math.Min(next, Math.Max(Elapsed, _heroNext[i]));
                for (var i = 0; i < _enemies.Count; i++)
                    if (_enemies[i].Health > 0) next = Math.Min(next, _enemyNext[i]);
                if (NeedsHealing()) next = Math.Min(next, Math.Max(Elapsed, _healReadyAt));
            }
            else next = Math.Min(next, _phaseEndsAt);

            Elapsed = next;
            if (Phase == ExpeditionPhase.March)
            {
                Distance = Math.Min(EncounterAt, _marchFrom + (Elapsed - _phaseBeganAt) * Speed);
                if (Elapsed >= _phaseEndsAt) BeginCombat();
            }
            else if (Phase == ExpeditionPhase.Loot)
            {
                if (Elapsed >= _phaseEndsAt) EndRest();
            }
            else TickCombat();
        }
    }

    public bool ActivateShield()
    {
        if (Paused || Phase != ExpeditionPhase.Combat || Party[0].Health <= 0 || ShieldCooldown > 0) return false;
        _shieldUntil = Elapsed + 4;
        _shieldReadyAt = Elapsed + 12;
        Emit("shield", 0, 0, 0, "Escudo de pressão: 65% de mitigação por 4s");
        return true;
    }

    public void Retreat()
    {
        if (!Terminal) ChangePhase(ExpeditionPhase.Retreated, "Equipe recuou; sem recompensas");
    }

    private bool Terminal => Phase == ExpeditionPhase.Complete || Phase == ExpeditionPhase.Defeat || Phase == ExpeditionPhase.Retreated;

    private void BeginMarch()
    {
        _marchFrom = Distance;
        EncounterAt = Distance + Legs[StageIndex];
        SpawnEncounter();
        ChangePhase(ExpeditionPhase.March, "Marcha: " + StageName);
        _phaseEndsAt = Elapsed + Legs[StageIndex] / Speed;
    }

    private void SpawnEncounter()
    {
        _enemies.Clear();
        switch (StageIndex)
        {
            case 0: AddEnemy("rat", "Rato de escória", 100); AddEnemy("rat", "Rato de fuligem", 90); break;
            case 1: AddEnemy("scavenger", "Catador dos trilhos", 140); AddEnemy("rat", "Rato dos trilhos", 100); break;
            case 2: AddEnemy("sentry", "Sentinela de pressão", 180); AddEnemy("drone", "Drone de inspeção", 120); break;
            case 3: AddEnemy("watcher", "Vigia do ninho", 180); AddEnemy("scavenger", "Catador do ninho", 150); break;
            default: AddEnemy("foreman", "Capataz da Fornalha", 420); AddEnemy("drone", "Drone da fornalha", 100); break;
        }
        Array.Clear(_enemySwings, 0, _enemySwings.Length);
    }

    private void AddEnemy(string kind, string name, float hp) => _enemies.Add(new ExpeditionEnemy(kind, name, hp));

    private void BeginCombat()
    {
        Distance = EncounterAt;
        ChangePhase(ExpeditionPhase.Combat, "Encontro: " + StageName);
        for (var i = 0; i < 3; i++) _heroNext[i] = Elapsed + .4 + i * .2;
        for (var i = 0; i < _enemies.Count; i++) _enemyNext[i] = Elapsed + 1 + i * .35;
    }

    private void TickCombat()
    {
        // Ties resolve tank, DPS, healer, then enemies in formation order.
        // A killing blow therefore cancels that enemy's pending attack.
        for (var actor = 0; actor < 3; actor++)
        {
            if (Party[actor].Health <= 0 || _heroNext[actor] > Elapsed) continue;
            _heroNext[actor] += HeroIntervals[actor];
            var target = FirstLivingEnemy();
            if (target < 0) break;
            var enemy = _enemies[target];
            var amount = Math.Min(enemy.Health, Math.Max(1, Party[actor].Attack - Armor(enemy.Kind)));
            Emit("attack", actor, 3 + target, amount, Party[actor].Name + " ataca " + enemy.Name);
            enemy.Health = Math.Max(0, enemy.Health - amount);
            Emit("hurt", actor, 3 + target, amount, enemy.Name + " sofreu dano");
            if (enemy.Health == 0) Emit("kill", actor, 3 + target, 0, enemy.Name + " caiu");
        }
        if (FirstLivingEnemy() < 0)
        {
            Victories++;
            if (StageIndex == 4) ChangePhase(ExpeditionPhase.Complete, "Rota concluída; cinco encontros vencidos");
            else
            {
                ChangePhase(ExpeditionPhase.Loot, "Encontro vencido; descanso de 2s, sem recompensas persistentes");
                _phaseEndsAt = Elapsed + 2;
            }
            return;
        }

        for (var actor = 0; actor < _enemies.Count; actor++)
        {
            var enemy = _enemies[actor];
            if (enemy.Health <= 0 || _enemyNext[actor] > Elapsed) continue;
            _enemyNext[actor] += EnemyInterval(enemy.Kind);
            var target = FirstLivingHero();
            if (target < 0) break;
            _enemySwings[actor]++;
            // Drones harass the gunner; watchers pressure the healer every third blow.
            var flank = enemy.Kind == "drone" ? 1 : enemy.Kind == "watcher" ? 2 : -1;
            if (flank >= 0 && _enemySwings[actor] % 3 == 0 && Party[flank].Health > 0) target = flank;
            var member = Party[target];
            var damage = Math.Max(1, EnemyAttack(enemy.Kind) - member.Defense);
            if (target == 0 && ShieldRemaining > 0) damage *= .35f;
            damage = Math.Min(member.Health, damage);
            Emit("attack", 3 + actor, target, damage, enemy.Name + " golpeia " + member.Name);
            member.Health = Math.Max(0, member.Health - damage);
            Emit("hurt", 3 + actor, target, damage, member.Name + " sofreu dano");
            if (member.Health == 0) Emit("kill", 3 + actor, target, 0, member.Name + " caiu");
        }

        if (FirstLivingHero() < 0) ChangePhase(ExpeditionPhase.Defeat, "A equipe caiu");
        else if (HealCooldown == 0 && NeedsHealing())
        {
            var target = 0;
            var lowest = 1f;
            for (var i = 0; i < 3; i++)
                if (Party[i].Health > 0 && Party[i].Health / Party[i].MaxHealth < lowest)
                { target = i; lowest = Party[i].Health / Party[i].MaxHealth; }
            Heal(target, 42, "Infusão Etérea");
            _healReadyAt = Elapsed + 4;
        }
    }

    private bool NeedsHealing()
    {
        if (Party[2].Health <= 0) return false;
        for (var i = 0; i < 3; i++)
            if (Party[i].Health > 0 && Party[i].Health <= Party[i].MaxHealth * .72f) return true;
        return false;
    }

    private void EndRest()
    {
        if (Party[2].Health > 0)
            for (var i = 0; i < 3; i++)
                if (Party[i].Health > 0 && Party[i].Health < Party[i].MaxHealth)
                    Heal(i, Party[i].MaxHealth * .2f, "Tratamento após 2s de descanso");
        StageIndex++;
        BeginMarch();
    }

    private void Heal(int target, float amount, string message)
    {
        amount = Math.Min(amount, Party[target].MaxHealth - Party[target].Health);
        Party[target].Health += amount;
        Emit("heal", 2, target, amount, message);
    }

    private int FirstLivingHero()
    {
        for (var i = 0; i < 3; i++) if (Party[i].Health > 0) return i;
        return -1;
    }

    private int FirstLivingEnemy()
    {
        for (var i = 0; i < _enemies.Count; i++) if (_enemies[i].Health > 0) return i;
        return -1;
    }

    private void ClampParty()
    {
        foreach (var member in Party)
            member.Health = float.IsNaN(member.Health) ? 0 : Math.Max(0, Math.Min(member.MaxHealth, member.Health));
    }

    private void ChangePhase(ExpeditionPhase phase, string message)
    {
        Phase = phase;
        _phaseBeganAt = Elapsed;
        // Route transitions are anchored to the guard, the formation's reference actor.
        Emit("phase", 0, 0, 0, message);
    }

    private void Emit(string kind, int actor, int target, float amount, string message)
    {
        LastEvent = message;
        if (_events.Count == EventLimit) _events.RemoveAt(0);
        _events.Add(new ExpeditionEvent(++_nextEventId, kind, actor, target, amount, Elapsed, message));
    }

    private static float Armor(string kind) => kind == "sentry" ? 4 : kind == "foreman" ? 3 : kind == "scavenger" ? 1 : 0;
    private static float EnemyAttack(string kind) => kind == "rat" ? 17 : kind == "scavenger" ? 22 : kind == "sentry" ? 25 : kind == "drone" ? 19 : kind == "watcher" ? 24 : 31;
    private static double EnemyInterval(string kind) => kind == "rat" ? 1.8 : kind == "scavenger" ? 2.2 : kind == "sentry" ? 2.4 : kind == "drone" ? 1.7 : kind == "watcher" ? 2.3 : 2.5;
}
