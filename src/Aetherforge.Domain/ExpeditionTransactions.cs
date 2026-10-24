using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Aetherforge.Domain;

public sealed partial class ExpeditionSimulation
{
    public string SessionId { get; private set; } = "";
    public long AttemptId { get; private set; }
    public long StateRevision { get; private set; }
    public long InventoryRevision { get; private set; }
    public ExpeditionManagement Management { get; private set; }
    public bool IsTerminal => Terminal;
    public double PhaseRemaining => Terminal ? 0 : Math.Max(0,_phaseEndsAt-Elapsed);
    private Dictionary<string,ExpeditionOperationReceipt> _operations = new Dictionary<string,ExpeditionOperationReceipt>(StringComparer.Ordinal);
    private IExpeditionCheckpointSink _checkpointSink = VolatileExpeditionCheckpointSink.Instance;
    private int _mutationGate;
    private bool _observeLegacy;
    private static readonly string[] MemberIds={"tank","dps","healer"};
    public static string GetMemberId(PartyRole role) => (int)role>=0 && (int)role<3 ? MemberIds[(int)role] : throw new ArgumentOutOfRangeException(nameof(role));
    private static int MemberIndex(string id) => Array.IndexOf(MemberIds,id);
    private static bool ValidId(string? id) => id!=null && Guid.TryParseExact(id,"N",out var guid) && guid.ToString("N")==id;
    private RouteCommand NewRouteCommand(RouteAction action,bool paused=false) => new RouteCommand(Guid.NewGuid().ToString("N"),SessionId,AttemptId,action,paused);
    private void EnterMutation()
    {
        if (Interlocked.CompareExchange(ref _mutationGate,1,0)!=0) throw new InvalidOperationException("Expedition commands must be serialized; reentry is prohibited.");
    }
    private void ExitMutation() { Volatile.Write(ref _mutationGate,0); }
    private void ObserveLegacyParty()
    {
        _observeLegacy=true;
        foreach (var member in Party)
        { member.BeforeMutation=LegacyMutation; member.Progression.BeforeMutation=LegacyMutation; }
    }
    private void LegacyMutation()
    {
        EnterMutation();
        try { StateRevision=checked(StateRevision+1); }
        finally { ExitMutation(); }
    }

    public ExpeditionReadModel ReadView() => new ExpeditionReadModel(this);

    public EquipmentPreview Compare(ComparisonQuery query)
    {
        if (query==null) throw new ArgumentNullException(nameof(query));
        int index=MemberIndex(query.MemberId);
        var code=ValidateEquipment(query,null,null);
        var before=index<0 ? null : Party[index].Stats;
        var equipped=index<0 || !Enum.IsDefined(typeof(LootSlot),query.Slot) ? null : Party[index].Loadout.Get(query.Slot)?.Id;
        var origin=query.CandidateItemId==null ? "slot" : Rewards.Items.Bag.Contains(query.CandidateItemId) ? "bag" : equipped==query.CandidateItemId ? "slot" : "unavailable";
        EffectiveStats? after=before;
        // Inspection remains useful in combat or outside management, but never authorizes a mutation.
        if (index>=0 && Enum.IsDefined(typeof(LootSlot),query.Slot) && Enum.IsDefined(typeof(EquipmentAction),query.Action))
        {
            ExpeditionDrop? item=null;
            bool found=query.Action==EquipmentAction.Unequip || (query.CandidateItemId!=null && Rewards.Items.Items.TryGetValue(query.CandidateItemId,out item));
            if (found && (item==null || (item.Role==Party[index].Role && item.Slot==query.Slot)))
                after=ExpeditionStats.Calculate(Party[index],Party[index].Loadout.With(query.Slot,item));
        }
        return new EquipmentPreview(query,StateRevision,equipped,origin,before,after,index<0 ? 0 : Party[index].Health,index<0 ? 1 : HeroIntervals[index],code);
    }

    private ExpeditionCode ValidateEquipment(ComparisonQuery q,long? revision,string? expected)
    {
        if (!ValidId(q.SessionId) || !Enum.IsDefined(typeof(EquipmentAction),q.Action) ||
            !Enum.IsDefined(typeof(LootSlot),q.Slot) || q.AttemptId<1 ||
            (q.Action==EquipmentAction.Equip && string.IsNullOrEmpty(q.CandidateItemId)) ||
            (q.Action==EquipmentAction.Unequip && q.CandidateItemId!=null)) return ExpeditionCode.InvalidCommand;
        if (q.SessionId!=SessionId) return ExpeditionCode.SessionMismatch;
        if (q.AttemptId!=AttemptId) return ExpeditionCode.AttemptMismatch;
        int index=MemberIndex(q.MemberId);
        if (index<0) return ExpeditionCode.MemberNotFound;
        if (Phase==ExpeditionPhase.Combat) return ExpeditionCode.InCombat;
        if (!Terminal && Management!=ExpeditionManagement.Active) return ExpeditionCode.ManagementRequired;
        var old=Rewards.Items.Loadouts[index].Get(q.Slot);
        if (revision.HasValue && (revision.Value!=StateRevision || expected!=old?.Id)) return ExpeditionCode.StaleComparison;
        if (q.Action==EquipmentAction.Unequip)
            return old==null ? ExpeditionCode.NoChange : Rewards.Items.Bag.Count==ExpeditionRewards.InventoryCapacity ? ExpeditionCode.BagFull : ExpeditionCode.Applied;
        if (!Rewards.Items.Items.TryGetValue(q.CandidateItemId!,out var item)) return ExpeditionCode.ItemNotAtExpectedLocation;
        if (item.Role!=Party[index].Role) return ExpeditionCode.RoleMismatch;
        if (item.Slot!=q.Slot) return ExpeditionCode.SlotMismatch;
        if (old?.Id==item.Id) return ExpeditionCode.NoChange;
        return Rewards.Items.Bag.Contains(item.Id) ? ExpeditionCode.Applied : ExpeditionCode.ItemNotAtExpectedLocation;
    }

    public CommandResult Execute(EquipmentCommand command) => ExecuteCommand(command);
    public CommandResult Execute(RouteCommand command) => ExecuteCommand(command);
    private CommandResult ExecuteCommand(ExpeditionCommand command)
    {
        EnterMutation();
        try
        {
            var operation=command?.OperationId ?? "";
            CommandResult Result(CommandOutcome outcome,ExpeditionCode code,ExpeditionOperationReceipt? receipt=null)
                => new CommandResult(operation,outcome,code,ReadView(),receipt);
            if (command==null) return Result(CommandOutcome.Rejected,ExpeditionCode.InvalidCommand);
            if (!ValidId(operation)) return Result(CommandOutcome.Rejected,ExpeditionCode.InvalidOperationId);
            if (!ValidId(command.SessionId) || command.AttemptId<1) return Result(CommandOutcome.Rejected,ExpeditionCode.InvalidCommand);
            if (command.SessionId!=SessionId) return Result(CommandOutcome.Rejected,ExpeditionCode.SessionMismatch);
            if (_operations.TryGetValue(operation,out var prior))
                return prior.Command.SamePayload(command) ? Result(CommandOutcome.Replay,ExpeditionCode.Applied,prior)
                    : Result(CommandOutcome.Rejected,ExpeditionCode.OperationConflict);
            if (command.AttemptId!=AttemptId) return Result(CommandOutcome.Rejected,ExpeditionCode.AttemptMismatch);
            try
            {
                ValidateState();
                var candidate=CloneState();
                ExpeditionCode code; string? old=null, equipped=null; int recovered=0;
                if (command is EquipmentCommand equipment)
                {
                    code=ValidateEquipment(equipment.Query,equipment.ExpectedStateRevision,equipment.ExpectedEquippedItemId);
                    if (code==ExpeditionCode.Applied)
                    {
                        int index=MemberIndex(equipment.Query.MemberId);
                        old=candidate.Rewards.Items.Loadouts[index].Get(equipment.Query.Slot)?.Id;
                        equipped=equipment.Query.CandidateItemId;
                        recovered=candidate.Rewards.Items.Move(index,equipment.Query.Slot,equipped);
                        candidate.Party[index].Loadout=candidate.Rewards.Items.Loadouts[index];
                        candidate.Party[index].Health=Math.Min(candidate.Party[index].Health,candidate.Party[index].MaxHealth);
                        candidate.InventoryRevision=checked(InventoryRevision+1);
                    }
                }
                else if (command is RouteCommand route) code=candidate.ApplyRoute(route);
                else code=ExpeditionCode.InvalidCommand;
                if (code!=ExpeditionCode.Applied)
                    return Result(code==ExpeditionCode.NoChange ? CommandOutcome.NoChange : CommandOutcome.Rejected,code);
                // A reset starts a distinct revision/receipt namespace.
                candidate.StateRevision=command is RouteCommand reset && reset.Action==RouteAction.ResetSession ? 0 : checked(StateRevision+1);
                var receipt=new ExpeditionOperationReceipt(command,candidate.StateRevision,old,equipped,recovered);
                if (candidate.SessionId==SessionId) candidate._operations.Add(operation,receipt);
                candidate.ValidateState();
                if (!Publish(candidate)) return Result(CommandOutcome.Rejected,ExpeditionCode.CheckpointFailed);
                return Result(CommandOutcome.Applied,ExpeditionCode.Applied,receipt);
            }
            catch (OverflowException) { return Result(CommandOutcome.Rejected,ExpeditionCode.NumericOverflow); }
            catch (InvalidOperationException) { return Result(CommandOutcome.Rejected,ExpeditionCode.InvalidState); }
        }
        finally { ExitMutation(); }
    }

    private ExpeditionCode ApplyRoute(RouteCommand command)
    {
        switch (command.Action)
        {
            case RouteAction.RequestManagement:
                if (Terminal || Management!=ExpeditionManagement.None) return ExpeditionCode.NoChange;
                Management=Phase==ExpeditionPhase.Combat ? ExpeditionManagement.Requested : ExpeditionManagement.Active; break;
            case RouteAction.CancelManagementRequest:
                if (Management!=ExpeditionManagement.Requested) return ExpeditionCode.NoChange;
                Management=ExpeditionManagement.None; break;
            case RouteAction.FinishManagement:
                if (Management!=ExpeditionManagement.Active) return ExpeditionCode.NoChange;
                Management=ExpeditionManagement.None; break;
            case RouteAction.SetPaused:
                if (Management==ExpeditionManagement.Active) return ExpeditionCode.ManagementActive;
                if (_paused==command.Paused) return ExpeditionCode.NoChange;
                _paused=command.Paused; break;
            case RouteAction.Retreat:
                if (Terminal) return ExpeditionCode.NoChange;
                ChangePhase(ExpeditionPhase.Retreated,"Equipe recuou; ganhos anteriores preservados"); break;
            case RouteAction.RestartRoute: RestartCore(); break;
            case RouteAction.ResetSession: ResetCore(); break;
            default: return ExpeditionCode.InvalidCommand;
        }
        return ExpeditionCode.Applied;
    }

    public AdvanceResult AdvanceDetailed(double delta)
    {
        if (double.IsNaN(delta) || double.IsInfinity(delta) || delta<0) throw new ArgumentOutOfRangeException(nameof(delta));
        EnterMutation();
        try
        {
            double start=Elapsed, until=Elapsed+delta;
            var events=new List<ExpeditionEvent>();
            bool failed=false;
            while (delta>0 && !Paused && !Terminal && Management!=ExpeditionManagement.Active && Elapsed<until)
            {
                var candidate=CloneState();
                int marker=_nextEventId;
                candidate.AdvanceStep(until);
                candidate.StateRevision=checked(StateRevision+1);
                candidate.ValidateState();
                // Only meaningful boundaries use the checkpoint port; ordinary steps stay volatile.
                bool boundary=candidate.Phase!=Phase || candidate.Rewards.Receipts.Count!=Rewards.Receipts.Count;
                if (boundary && !AcceptCheckpoint(candidate)) { failed=true; break; }
                CopyFrom(candidate);
                events.AddRange(_events.Where(e=>e.Id>marker));
            }
            double consumed=Elapsed-start;
            var reason=failed ? AdvanceStopReason.CheckpointFailed : Terminal ? AdvanceStopReason.Terminal :
                Management==ExpeditionManagement.Active ? AdvanceStopReason.Management : Paused ? AdvanceStopReason.Paused : AdvanceStopReason.Horizon;
            return new AdvanceResult(consumed,Math.Max(0,delta-consumed),reason,StateRevision,events.AsReadOnly());
        }
        finally { ExitMutation(); }
    }

    private bool AcceptCheckpoint(ExpeditionSimulation candidate)
    {
        try { return _checkpointSink.Commit(new ExpeditionSnapshot(candidate)); }
        catch { return false; }
    }
    private bool Publish(ExpeditionSimulation candidate)
    {
        if (!AcceptCheckpoint(candidate)) return false;
        CopyFrom(candidate); return true;
    }

    internal void ValidateState()
    {
        if (!ValidId(SessionId) || AttemptId<1 || StateRevision<0 || InventoryRevision<0 || Party.Count!=3 ||
            StageIndex<0 || StageIndex>4 || Victories<0 || Victories>5 || !Enum.IsDefined(typeof(ExpeditionPhase),Phase) ||
            !Enum.IsDefined(typeof(ExpeditionManagement),Management) || double.IsNaN(Elapsed) || double.IsInfinity(Elapsed) || Elapsed<0 ||
            (Management==ExpeditionManagement.Requested && Phase!=ExpeditionPhase.Combat) ||
            (Management==ExpeditionManagement.Active && Phase!=ExpeditionPhase.March && Phase!=ExpeditionPhase.Loot))
            throw new InvalidOperationException("Invalid expedition state.");
        Rewards.Validate(AttemptId);
        bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value) && value>=0;
        if (!Finite(Distance) || !Finite(EncounterAt) || !Finite(_phaseBeganAt) || !Finite(_phaseEndsAt) || !Finite(_marchFrom) ||
            !Finite(_shieldUntil) || !Finite(_shieldReadyAt) || !Finite(_healReadyAt) ||
            _heroNext.Any(t=>!Finite(t)) || _enemyNext.Any(t=>!Finite(t)) || _enemySwings.Any(n=>n<0) || _nextEventId<0 ||
            (Phase==ExpeditionPhase.Complete && (StageIndex!=4 || Victories!=5)) ||
            (Phase==ExpeditionPhase.Loot && Victories!=StageIndex+1) ||
            ((Phase==ExpeditionPhase.Combat || Phase==ExpeditionPhase.March) && Victories!=StageIndex))
            throw new InvalidOperationException("Invalid scheduler state.");
        foreach (var enemy in _enemies)
            if (!Finite(enemy.Health) || !Finite(enemy.MaxHealth) || enemy.MaxHealth<=0 || enemy.Health>enemy.MaxHealth)
                throw new InvalidOperationException("Invalid enemy state.");
        for (int i=0;i<3;i++)
        {
            var member=Party[i];
            if (member.Role!=(PartyRole)i || member.Loadout!=Rewards.Items.Loadouts[i] || float.IsNaN(member.Health) ||
                float.IsInfinity(member.Health) || member.Health<0 || member.Health>member.MaxHealth ||
                !Finite(member.MaxHealth) || !Finite(member.Attack) || !Finite(member.Defense) || member.MaxHealth<=0 ||
                member.Progression.Level<1 || member.Progression.Level>20)
                throw new InvalidOperationException("Invalid party state.");
        }
    }

    internal ExpeditionSimulation CloneState()
    {
        var copy=new ExpeditionSimulation(true);
        copy.CopyFrom(this);
        return copy;
    }

    private void CopyFrom(ExpeditionSimulation source)
    {
        SessionId=source.SessionId; AttemptId=source.AttemptId; StateRevision=source.StateRevision; InventoryRevision=source.InventoryRevision;
        Management=source.Management; Phase=source.Phase; StageIndex=source.StageIndex; Victories=source.Victories;
        Distance=source.Distance; EncounterAt=source.EncounterAt; Elapsed=source.Elapsed; _paused=source._paused;
        _phaseBeganAt=source._phaseBeganAt; _phaseEndsAt=source._phaseEndsAt; _marchFrom=source._marchFrom;
        _shieldUntil=source._shieldUntil; _shieldReadyAt=source._shieldReadyAt; _healReadyAt=source._healReadyAt;
        _nextEventId=source._nextEventId; LastEvent=source.LastEvent;
        Array.Copy(source._heroNext,_heroNext,3); Array.Copy(source._enemyNext,_enemyNext,2); Array.Copy(source._enemySwings,_enemySwings,2);
        _events.Clear(); _events.AddRange(source._events);
        _enemies.Clear();
        foreach (var enemy in source._enemies) _enemies.Add(new ExpeditionEnemy(enemy.Kind,enemy.Name,enemy.MaxHealth) { Health=enemy.Health });
        if (_observeLegacy && Party.Count==3)
        { for (int i=0;i<3;i++) Party[i].CopyFrom(source.Party[i]); }
        else Party=source.Party.Select(p=>p.Clone()).ToList();
        Rewards=source.Rewards.Clone();
        _operations=new Dictionary<string,ExpeditionOperationReceipt>(source._operations,StringComparer.Ordinal);
    }
}
