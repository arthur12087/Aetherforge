using System;
using System.Collections.Generic;
using System.Linq;
using Aetherforge.Domain;

/// <summary>Standalone checks for the local expedition; no Godot, service or save files.</summary>
public static class ExpeditionChecks
{
    public static void Run()
    {
        MarchAndRoles();
        HealingAndDeaths();
        ShieldAndPause();
        RetreatAndReset();
        RouteAndEvents();
        InvalidDeltaAndHealth();
        DeterminismAndTime();
    }

    private static void MarchAndRoles()
    {
        var sim = new ExpeditionSimulation();
        Require(sim.StageIndex == 0 && sim.StageName == "Entrada" && sim.EncounterAt > 0, "Initial route.");
        Require(sim.Party.Select(p => p.Role).SequenceEqual(new[] { PartyRole.Tank, PartyRole.Dps, PartyRole.Healer }), "Starter formation roles.");
        Require(!sim.ActivateShield(), "Shield outside combat.");
        sim.Advance(2);
        Near(sim.Distance, 4.8, "Real march speed.");
        Near(sim.Elapsed, 2, "March elapsed.");
        Require(sim.Phase == ExpeditionPhase.March && sim.Events.All(e => e.Kind == "phase"), "March cannot attack.");
        Require(sim.Party.All(p => p.Health == p.MaxHealth) && sim.Enemies.All(e => e.Health == e.MaxHealth), "March cannot damage.");
        ReachCombat(sim);
        Near(sim.Distance, sim.EncounterAt, "Encounter position.");
        var distance = sim.Distance;
        var marker = sim.Events.Last().Id;
        sim.Advance(.4);
        var attack = sim.Events.Last(e => e.Kind == "attack");
        Require(attack.Actor == 0 && attack.Target == 3 && attack.At == sim.Elapsed, "Hero attack identity/time.");
        sim.Advance(2.1);
        Near(sim.Distance, distance, "Combat holds formation position.");
        Require(sim.Party[0].Health < sim.Party[0].MaxHealth && sim.Party[1].Health == sim.Party[1].MaxHealth && sim.Party[2].Health == sim.Party[2].MaxHealth, "Tank receives common hits.");
        var attacks = sim.Events.Where(e => e.Id > marker && e.Kind == "attack").ToList();
        Require(new[] { 0, 1, 2 }.All(i => attacks.Any(e => e.Actor == i && e.Target >= 3)), "All living heroes attack.");
        Require(attacks.Where(e => e.Actor == 1).Sum(e => e.Amount) > attacks.Where(e => e.Actor == 0).Sum(e => e.Amount), "DPS does more damage than tank.");
        Require(attacks.Any(e => e.Actor >= 3 && e.Target == 0), "Enemy attack events.");
    }

    private static void HealingAndDeaths()
    {
        var sim = new ExpeditionSimulation();
        ReachCombat(sim);
        sim.Advance(.9);
        Require(!sim.Events.Any(e => e.Kind == "heal"), "No heal at full health.");
        sim.Party[0].Health = 70;
        var before = sim.Party[0].Health;
        sim.Advance(.01);
        Require(sim.Party[0].Health > before && sim.HealCooldown > 0, "Conditional automatic heal and cooldown.");
        var heal = sim.Events.Last();
        Require(heal.Kind == "heal" && heal.Actor == 2 && heal.Target == 0 && heal.Amount > 0, "Healer identity.");
        Near(heal.At, sim.Elapsed - .01, "Heal at exact simulated event time.");
        var count = sim.Events.Count(e => e.Kind == "heal");
        sim.Advance(.1);
        Require(sim.Events.Count(e => e.Kind == "heal") == count, "Heal cooldown prevents repeat.");

        var fallen = new ExpeditionSimulation();
        ReachCombat(fallen);
        fallen.Party[0].Health = fallen.Party[2].Health = 0;
        var id = fallen.Events.Last().Id;
        fallen.Advance(2);
        Require(fallen.Party[1].Health < fallen.Party[1].MaxHealth, "Fallback target when tank dies.");
        Require(!fallen.Events.Any(e => e.Id > id && (e.Kind == "heal" || (e.Kind == "attack" && (e.Actor == 0 || e.Actor == 2)))), "Dead heroes cannot attack or heal.");

        var defeat = new ExpeditionSimulation();
        ReachCombat(defeat);
        defeat.Party[0].Health = defeat.Party[2].Health = 0;
        defeat.Party[1].Health = 1;
        defeat.Advance(10);
        Require(defeat.Phase == ExpeditionPhase.Defeat && defeat.Victories == 0, "Actual enemy blow causes defeat.");
        var frozen = State(defeat);
        defeat.Advance(999);
        defeat.Retreat();
        Require(State(defeat) == frozen && !defeat.ActivateShield(), "Defeat is terminal.");
    }

    private static void ShieldAndPause()
    {
        var baseline = new ExpeditionSimulation();
        var shielded = new ExpeditionSimulation();
        ReachCombat(baseline);
        ReachCombat(shielded);
        Require(shielded.ActivateShield() && !shielded.ActivateShield(), "Shield activation/cooldown.");
        var shieldEvent = shielded.Events.Last();
        Require(shieldEvent.Kind == "shield" && shieldEvent.Actor == 0 && shieldEvent.Target == 0 && shieldEvent.At == shielded.Elapsed, "Shield event.");
        shielded.Advance(1.4);
        baseline.Advance(1.4);
        Require(shielded.Party[0].Health > baseline.Party[0].Health, "Shield mitigates real damage versus baseline.");
        var baselineHit = baseline.Events.First(e => e.Kind == "hurt" && e.Actor >= 3);
        var shieldHit = shielded.Events.First(e => e.Kind == "hurt" && e.Actor >= 3);
        Near(shieldHit.Amount, baselineHit.Amount * .35, "65 percent shield mitigation.", .0001);
        Require(shielded.ShieldCooldown > shielded.ShieldRemaining && shielded.ShieldRemaining > 0, "Shield duration and cooldown.");
        shielded.Party[0].Health = 70;
        shielded.Advance(.01);
        shielded.Paused = true;
        var before = State(shielded);
        shielded.Advance(10000);
        Require(State(shielded) == before && !shielded.ActivateShield(), "Pause freezes time, HP, distance, events and all cooldowns.");
        shielded.Paused = false;
        shielded.Advance(4);
        Require(shielded.ShieldRemaining == 0 && shielded.ShieldCooldown > 0 && !shielded.ActivateShield(), "Shield expires before cooldown.");

        var ready = new ExpeditionSimulation();
        ReachCombat(ready);
        ready.Paused = true;
        Require(!ready.ActivateShield(), "Paused activation rejected even when ready.");
        ready.Paused = false;
        ready.Party[0].Health = 0;
        Require(!ready.ActivateShield(), "Dead tank cannot shield.");
    }

    private static void RetreatAndReset()
    {
        var sim = new ExpeditionSimulation();
        sim.Advance(2);
        var elapsed = sim.Elapsed;
        var distance = sim.Distance;
        sim.Paused = true;
        sim.Retreat();
        Require(sim.Phase == ExpeditionPhase.Retreated && sim.Elapsed == elapsed && sim.Distance == distance && sim.Victories == 0, "Retreat without hidden time or rewards.");
        sim.Paused = false;
        var frozen = State(sim);
        sim.Advance(double.MaxValue);
        sim.Retreat();
        Require(State(sim) == frozen, "Retreat terminal and idempotent.");
        sim.Reset();
        Require(State(sim) == State(new ExpeditionSimulation()), "Reset creates a full fresh session.");
        ReachCombat(sim);
        sim.ActivateShield();
        sim.Advance(3);
        sim.Paused = true;
        sim.Reset();
        Require(!sim.Paused && State(sim) == State(new ExpeditionSimulation()), "Reset clears HP, shield, timers, history and pause.");
    }

    private static void RouteAndEvents()
    {
        var sim = new ExpeditionSimulation();
        var kinds = new HashSet<string>();
        var stages = new HashSet<int>();
        var phaseTimes = new List<double>();
        var sawHeal = false;
        var sawDamage = false;
        var sawRest = false;
        var sawKill = false;
        var sawBound = false;
        var lastId = 0;
        var lastAt = 0d;
        var previousDistance = 0d;
        var previousPhase = sim.Phase;
        for (var i = 0; i < 4000 && sim.Phase != ExpeditionPhase.Complete; i++)
        {
            sim.Advance(.05);
            stages.Add(sim.StageIndex);
            foreach (var enemy in sim.Enemies) kinds.Add(enemy.Kind);
            Require(sim.Distance >= previousDistance, "Distance never resets between stages.");
            if (previousPhase != ExpeditionPhase.March && sim.Phase != ExpeditionPhase.March)
                Near(sim.Distance, previousDistance, "Only march moves.");
            previousDistance = sim.Distance;
            previousPhase = sim.Phase;
            Require(sim.Party.All(p => p.Health >= 0 && p.Health <= p.MaxHealth) && sim.Enemies.All(e => e.Health >= 0 && e.Health <= e.MaxHealth), "Bounded HP throughout route.");
            sawDamage |= sim.Party.Any(p => p.Health < p.MaxHealth);
            sawRest |= sim.Phase == ExpeditionPhase.Loot;
            Require(sim.Events.Count <= 128, "Recent event limit.");
            sawBound |= sim.Events.Count == 128;
            foreach (var ev in sim.Events.Where(e => e.Id > lastId))
            {
                Require(ev.Id == lastId + 1 && ev.At >= lastAt && ev.At <= sim.Elapsed, "Monotonic event IDs and simulated timestamps.");
                Require(new[] { "attack", "hurt", "heal", "shield", "kill", "phase", "reward", "level", "drop" }.Contains(ev.Kind), "Contract event kind.");
                Require(ev.Actor >= 0 && ev.Actor < 3 + sim.Enemies.Count && ev.Target >= 0 && ev.Target < 3 + sim.Enemies.Count, "Contract actor/target indices.");
                Require(ev.Amount >= 0 && !float.IsNaN(ev.Amount), "Valid event amount.");
                if (ev.Kind == "heal") { sawHeal = true; Require(ev.Actor == 2 && ev.Target < 3 && ev.Amount > 0, "Visible healer event."); }
                if (ev.Kind == "kill") sawKill = true;
                if (ev.Kind == "phase") phaseTimes.Add(ev.At);
                lastId = ev.Id;
                lastAt = ev.At;
            }
        }
        Require(sim.Phase == ExpeditionPhase.Complete && sim.Victories == 5 && sim.StageIndex == 4, "Automatic victory through five stages.");
        Require(sim.Elapsed >= 100 && sim.Elapsed <= 160, "Route must last 100–160 simulated seconds without shield.");
        Require(kinds.SetEquals(new[] { "rat", "scavenger", "sentry", "drone", "watcher", "foreman" }) && stages.Count == 5, "Five stages and six enemy types.");
        Require(sawDamage && sawHeal && sawRest && sawKill && sawBound, "Damage, healing, rest, kills and bounded event eviction observed.");
        Near(sim.Distance, 108, "Absolute route distance.");
        // Each intermediate combat ends in two seconds of rest; no hidden transition time.
        Require(phaseTimes.Count == 15, "Initial march + five combats + four rests/marches + completion.");
        for (var i = 2; i < 14; i += 3) Near(phaseTimes[i + 1] - phaseTimes[i], 2, "Declared rest duration.");
        var frozen = State(sim);
        sim.Advance(1000);
        sim.Retreat();
        Require(State(sim) == frozen && !sim.ActivateShield(), "Victory terminal.");
        sim.Reset();
        Require(State(sim) == State(new ExpeditionSimulation()), "Reset after final victory.");
    }

    private static void InvalidDeltaAndHealth()
    {
        var sim = new ExpeditionSimulation();
        foreach (var paused in new[] { false, true })
        {
            sim.Paused = paused;
            foreach (var delta in new[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity, -1d, -double.Epsilon })
            {
                var before = State(sim);
                var rejected = false;
                try { sim.Advance(delta); } catch (ArgumentOutOfRangeException) { rejected = true; }
                Require(rejected && State(sim) == before, "Invalid delta rejected atomically, including during pause.");
            }
        }
        sim.Paused = false;
        var unchanged = State(sim);
        sim.Advance(0);
        Require(State(sim) == unchanged, "Zero delta is a no-op.");
        sim.Party[0].Health = float.PositiveInfinity;
        sim.Party[1].Health = -4;
        sim.Party[2].Health = float.NaN;
        sim.Advance(.1);
        Require(sim.Party[0].Health == sim.Party[0].MaxHealth && sim.Party[1].Health == 0 && sim.Party[2].Health == 0, "Public party HP clamped safely.");
    }

    private static void DeterminismAndTime()
    {
        var one = new ExpeditionSimulation();
        var two = new ExpeditionSimulation();
        foreach (var delta in new[] { .01, .16, 7.25, 2d, 8.4, 31.125, .5 })
        {
            one.Advance(delta);
            two.Advance(delta);
            Require(State(one) == State(two), "Identical inputs give identical states/events.");
        }
        var large = new ExpeditionSimulation();
        var sliced = new ExpeditionSimulation();
        large.Advance(50);
        for (var i = 0; i < 200; i++) sliced.Advance(.25);
        Compare(large, sliced);
        Near(large.Elapsed, 50, "Large delta is fully consumed across encounters.");
        var huge = new ExpeditionSimulation();
        huge.Advance(double.MaxValue);
        large.Advance(1000);
        for (var i = 0; i < 600; i++) sliced.Advance(.25);
        Compare(large, sliced);
        Compare(large, huge);
        Require(huge.Phase == ExpeditionPhase.Complete && huge.Elapsed <= 160, "Very large finite delta completes without truncation or overflow.");
    }

    private static void Compare(ExpeditionSimulation a, ExpeditionSimulation b)
    {
        Require(a.Phase == b.Phase && a.StageIndex == b.StageIndex && a.Victories == b.Victories, "Delta partition phase invariance.");
        Near(a.Elapsed, b.Elapsed, "Delta partition elapsed invariance.");
        Near(a.Distance, b.Distance, "Delta partition distance invariance.");
        Near(a.PhaseTime, b.PhaseTime, "Delta partition phase timer.");
        Near(a.HealCooldown, b.HealCooldown, "Delta partition heal timer.");
        for (var i = 0; i < 3; i++) Near(a.Party[i].Health, b.Party[i].Health, "Delta partition hero HP.");
        for (var i = 0; i < a.Enemies.Count; i++) Near(a.Enemies[i].Health, b.Enemies[i].Health, "Delta partition enemy HP.");
        Require(a.Events.Count == b.Events.Count, "Delta partition event count.");
        for (var i = 0; i < a.Events.Count; i++)
        {
            var x = a.Events[i]; var y = b.Events[i];
            Require(x.Id == y.Id && x.Kind == y.Kind && x.Actor == y.Actor && x.Target == y.Target && x.Amount == y.Amount && x.Message == y.Message, "Delta partition event identity.");
            Near(x.At, y.At, "Delta partition event timestamp.");
        }
    }

    private static void ReachCombat(ExpeditionSimulation sim) => sim.Advance((sim.EncounterAt - sim.Distance) / 2.4);

    private static string State(ExpeditionSimulation sim) => string.Join("|", new[]
    {
        sim.Phase.ToString(), sim.StageIndex.ToString(), sim.Victories.ToString(), sim.Elapsed.ToString("R"),
        sim.PhaseTime.ToString("R"), sim.Distance.ToString("R"), sim.EncounterAt.ToString("R"), sim.Paused.ToString(),
        sim.ShieldCooldown.ToString("R"), sim.ShieldRemaining.ToString("R"), sim.HealCooldown.ToString("R"), sim.LastEvent,
        string.Join(",", sim.Party.Select(p => p.Health.ToString("R"))),
        string.Join(",", sim.Enemies.Select(e => e.Kind + ":" + e.Health.ToString("R"))),
        string.Join(";", sim.Events.Select(e => $"{e.Id}/{e.Kind}/{e.Actor}/{e.Target}/{e.Amount:R}/{e.At:R}/{e.Message}"))
    });

    private static void Near(double actual, double expected, string message, double tolerance = .000001)
        => Require(Math.Abs(actual - expected) <= tolerance, message + $" Expected {expected:R}, got {actual:R}.");

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("Expedition: " + message);
    }
}
