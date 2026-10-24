using System;
using System.Collections.Generic;
using System.Linq;

namespace Aetherforge.Domain;

/// <summary>Memory-only commit barrier. Implementations must not reenter the aggregate.</summary>
public interface IExpeditionCheckpointSink
{
    bool Commit(ExpeditionSnapshot candidate);
}

public sealed class VolatileExpeditionCheckpointSink : IExpeditionCheckpointSink
{
    public static VolatileExpeditionCheckpointSink Instance { get; } = new VolatileExpeditionCheckpointSink();
    private VolatileExpeditionCheckpointSink() { }
    public bool Commit(ExpeditionSnapshot candidate) => true;
}

public sealed class ExpeditionMemberView
{
    public string MemberId { get; }
    public PartyRole Role { get; }
    public string Name { get; }
    public string Model { get; }
    public int Level { get; }
    public int Experience { get; }
    public float Health { get; }
    public EffectiveStats Stats { get; }
    public ExpeditionLoadout Loadout { get; }
    internal ExpeditionMemberView(PartyMember member)
    {
        MemberId=ExpeditionSimulation.GetMemberId(member.Role); Role=member.Role; Name=member.Name; Model=member.Model;
        Level=member.Progression.Level; Experience=member.Progression.Experience; Health=member.Health;
        Stats=member.Stats; Loadout=member.Loadout;
    }
}

/// <summary>Detached immutable view; collections never expose mutable party, enemy or inventory state.</summary>
public sealed class ExpeditionReadModel
{
    public string SessionId { get; }
    public long AttemptId { get; }
    public long StateRevision { get; }
    public long InventoryRevision { get; }
    public ExpeditionPhase Phase { get; }
    public ExpeditionManagement Management { get; }
    public bool Paused { get; }
    public int StageIndex { get; }
    public int Victories { get; }
    public double Elapsed { get; }
    public double Distance { get; }
    public double EncounterAt { get; }
    public double PhaseRemaining { get; }
    public double ShieldCooldown { get; }
    public double ShieldRemaining { get; }
    public double HealCooldown { get; }
    public long Coins { get; }
    public long Scrap { get; }
    public long Components { get; }
    public long RewardSequence { get; }
    public IReadOnlyList<ExpeditionMemberView> Members { get; }
    public IReadOnlyList<ExpeditionDrop> Inventory { get; }
    public IReadOnlyList<ExpeditionDrop> PendingDrops { get; }
    internal ExpeditionReadModel(ExpeditionSimulation sim)
    {
        SessionId=sim.SessionId; AttemptId=sim.AttemptId; StateRevision=sim.StateRevision; InventoryRevision=sim.InventoryRevision;
        Phase=sim.Phase; Management=sim.Management; Paused=sim.Paused; StageIndex=sim.StageIndex; Victories=sim.Victories;
        Elapsed=sim.Elapsed; Distance=sim.Distance; EncounterAt=sim.EncounterAt; PhaseRemaining=sim.PhaseRemaining;
        ShieldCooldown=sim.ShieldCooldown; ShieldRemaining=sim.ShieldRemaining; HealCooldown=sim.HealCooldown;
        Coins=sim.Rewards.Coins; Scrap=sim.Rewards.Scrap; Components=sim.Rewards.Components;
        RewardSequence=sim.Rewards.LastReward?.Sequence ?? 0;
        Members=Array.AsReadOnly(sim.Party.Select(p=>new ExpeditionMemberView(p)).ToArray());
        Inventory=sim.Rewards.Inventory; PendingDrops=sim.Rewards.PendingDrops;
    }
}

/// <summary>
/// Exact detached in-memory checkpoint, with observable state for testing and the later codec.
/// Construction from untrusted serialized data, format/versioning and disk IO belong to the save task.
/// </summary>
public sealed class ExpeditionSnapshot
{
    internal ExpeditionSimulation State { get; }
    public string RulesetId => "v19-equipment-1";
    public ExpeditionReadModel View { get; }
    public uint Seed { get; }
    public uint RandomState { get; }
    public long LastIssuedItemSequence { get; }
    public IReadOnlyList<ExpeditionDrop> Items { get; }
    public IReadOnlyList<ExpeditionReward> Rewards { get; }
    public IReadOnlyList<bool> CurrentAttemptCredited { get; }
    public IReadOnlyList<ExpeditionOperationReceipt> Operations { get; }
    public IReadOnlyList<ExpeditionEvent> Events { get; }
    public IReadOnlyList<ExpeditionEnemyState> Enemies { get; }
    public IReadOnlyList<double> HeroNext { get; }
    public IReadOnlyList<double> EnemyNext { get; }
    public IReadOnlyList<int> EnemySwings { get; }
    public double PhaseBeganAt { get; }
    public double PhaseEndsAt { get; }
    public double MarchFrom { get; }
    public double ShieldUntil { get; }
    public double ShieldReadyAt { get; }
    public double HealReadyAt { get; }
    public int NextEventId { get; }
    public string LastEvent { get; }
    internal ExpeditionSnapshot(ExpeditionSimulation sim)
    {
        State=sim.CloneState(); View=State.ReadView();
        Seed=sim.Rewards.Seed; RandomState=sim.Rewards.RandomState; LastIssuedItemSequence=sim.Rewards.LastIssuedItemSequence;
        Items=Array.AsReadOnly(sim.Rewards.Items.Items.Values.OrderBy(i=>i.Id,StringComparer.Ordinal).ToArray());
        Rewards=Array.AsReadOnly(sim.Rewards.Receipts.ToArray()); CurrentAttemptCredited=sim.Rewards.Credited;
        Operations=State.OperationHistory; Events=Array.AsReadOnly(sim.Events.ToArray());
        Enemies=Array.AsReadOnly(sim.Enemies.Select(e=>new ExpeditionEnemyState(e)).ToArray());
        var timers=State.CaptureTimers();
        HeroNext=Array.AsReadOnly(timers.Hero); EnemyNext=Array.AsReadOnly(timers.Enemy); EnemySwings=Array.AsReadOnly(timers.Swings);
        PhaseBeganAt=timers.Began; PhaseEndsAt=timers.Ends; MarchFrom=timers.From;
        ShieldUntil=timers.Shield; ShieldReadyAt=timers.ShieldReady; HealReadyAt=timers.HealReady;
        NextEventId=timers.EventId; LastEvent=sim.LastEvent;
    }
}

public sealed class ExpeditionEnemyState
{
    public string Kind { get; }
    public string Name { get; }
    public float MaxHealth { get; }
    public float Health { get; }
    internal ExpeditionEnemyState(ExpeditionEnemy enemy)
    { Kind=enemy.Kind; Name=enemy.Name; MaxHealth=enemy.MaxHealth; Health=enemy.Health; }
}

public sealed partial class ExpeditionSimulation
{
    public ExpeditionSnapshot CaptureCheckpoint() => new ExpeditionSnapshot(this);
    public static ExpeditionSimulation RestoreValidated(ExpeditionSnapshot snapshot,IExpeditionCheckpointSink? checkpointSink=null)
    {
        if (snapshot==null) throw new ArgumentNullException(nameof(snapshot));
        var sim=snapshot.State.CloneState(); sim.ValidateState();
        sim._checkpointSink=checkpointSink ?? VolatileExpeditionCheckpointSink.Instance;
        sim.ObserveLegacyParty();
        return sim;
    }
    internal IReadOnlyList<ExpeditionOperationReceipt> OperationHistory => Array.AsReadOnly(_operations.Values.ToArray());
    internal (double[] Hero,double[] Enemy,int[] Swings,double Began,double Ends,double From,double Shield,double ShieldReady,double HealReady,int EventId) CaptureTimers()
        => ((double[])_heroNext.Clone(),(double[])_enemyNext.Clone(),(int[])_enemySwings.Clone(),_phaseBeganAt,_phaseEndsAt,_marchFrom,_shieldUntil,_shieldReadyAt,_healReadyAt,_nextEventId);
}
