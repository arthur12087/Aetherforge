using System;
using System.Collections.Generic;

namespace Aetherforge.Domain;

public enum ExpeditionManagement { None, Requested, Active }
public enum EquipmentAction { Equip, Unequip }
public enum RouteAction { RequestManagement, CancelManagementRequest, FinishManagement, SetPaused, Retreat, RestartRoute, ResetSession }
public enum CommandOutcome { Applied, NoChange, Rejected, Replay }
public enum ExpeditionCode
{
    Applied, NoChange, InvalidCommand, InvalidOperationId, SessionMismatch, AttemptMismatch,
    OperationConflict, MemberNotFound, InCombat, ManagementRequired, ManagementActive,
    StaleComparison, ItemNotAtExpectedLocation, RoleMismatch, SlotMismatch, BagFull,
    InvalidState, NumericOverflow, CheckpointFailed, CommitIndeterminate
}
public enum AdvanceStopReason { Horizon, Paused, Management, Terminal, CheckpointFailed }

public abstract class ExpeditionCommand
{
    public string OperationId { get; }
    public string SessionId { get; }
    public long AttemptId { get; }
    protected ExpeditionCommand(string operationId, string sessionId, long attemptId)
    { OperationId=operationId; SessionId=sessionId; AttemptId=attemptId; }
    internal abstract bool SamePayload(ExpeditionCommand other);
}

public sealed class ComparisonQuery
{
    public string SessionId { get; }
    public long AttemptId { get; }
    public string MemberId { get; }
    public LootSlot Slot { get; }
    public EquipmentAction Action { get; }
    public string? CandidateItemId { get; }
    public ComparisonQuery(string sessionId,long attemptId,string memberId,LootSlot slot,EquipmentAction action,string? candidateItemId=null)
    { SessionId=sessionId; AttemptId=attemptId; MemberId=memberId; Slot=slot; Action=action; CandidateItemId=candidateItemId; }
}

public sealed class EquipmentCommand : ExpeditionCommand
{
    public ComparisonQuery Query { get; }
    public string? ExpectedEquippedItemId { get; }
    public long ExpectedStateRevision { get; }
    public EquipmentCommand(string operationId, ComparisonQuery query,long expectedStateRevision,string? expectedEquippedItemId)
        : base(operationId,query.SessionId,query.AttemptId)
    { Query=query; ExpectedStateRevision=expectedStateRevision; ExpectedEquippedItemId=expectedEquippedItemId; }
    internal override bool SamePayload(ExpeditionCommand other) => other is EquipmentCommand c &&
        SessionId==c.SessionId && AttemptId==c.AttemptId && Query.MemberId==c.Query.MemberId && Query.Slot==c.Query.Slot &&
        Query.Action==c.Query.Action && Query.CandidateItemId==c.Query.CandidateItemId &&
        ExpectedEquippedItemId==c.ExpectedEquippedItemId && ExpectedStateRevision==c.ExpectedStateRevision;
}

public sealed class RouteCommand : ExpeditionCommand
{
    public RouteAction Action { get; }
    public bool Paused { get; }
    public RouteCommand(string operationId,string sessionId,long attemptId,RouteAction action,bool paused=false)
        : base(operationId,sessionId,attemptId) { Action=action; Paused=paused; }
    internal override bool SamePayload(ExpeditionCommand other) => other is RouteCommand c &&
        SessionId==c.SessionId && AttemptId==c.AttemptId && Action==c.Action && Paused==c.Paused;
}

public sealed class EquipmentPreview
{
    public ComparisonQuery Query { get; }
    public long ExpectedStateRevision { get; }
    public string? ExpectedEquippedItemId { get; }
    public string ObservedOrigin { get; }
    public EffectiveStats? Before { get; }
    public EffectiveStats? After { get; }
    public float HealthBefore { get; }
    public float HealthAfter { get; }
    public double DpsBefore { get; }
    public double DpsAfter { get; }
    public float AttackDelta => (After?.Attack ?? 0)-(Before?.Attack ?? 0);
    public float DefenseDelta => (After?.Defense ?? 0)-(Before?.Defense ?? 0);
    public float HealthDelta => (After?.MaxHealth ?? 0)-(Before?.MaxHealth ?? 0);
    public bool CanConfirm => BlockReason==ExpeditionCode.Applied;
    public ExpeditionCode BlockReason { get; }
    internal EquipmentPreview(ComparisonQuery query,long revision,string? equipped,string origin,EffectiveStats? before,
        EffectiveStats? after,float health,double interval,ExpeditionCode code)
    {
        Query=query; ExpectedStateRevision=revision; ExpectedEquippedItemId=equipped; ObservedOrigin=origin;
        Before=before; After=after; HealthBefore=health; HealthAfter=after==null ? health : Math.Min(health,after.MaxHealth);
        DpsBefore=(before?.Attack ?? 0)/interval; DpsAfter=(after?.Attack ?? 0)/interval; BlockReason=code;
    }
    public EquipmentCommand Confirm(string operationId) => new EquipmentCommand(operationId,Query,ExpectedStateRevision,ExpectedEquippedItemId);
}

/// <summary>Immutable receipt, separate from the latest view returned to the caller.</summary>
public sealed class ExpeditionOperationReceipt
{
    public ExpeditionCommand Command { get; }
    public long AppliedRevision { get; }
    public string? PreviousItemId { get; }
    public string? EquippedItemId { get; }
    public int PendingRecovered { get; }
    internal ExpeditionOperationReceipt(ExpeditionCommand command,long revision,string? previous=null,string? equipped=null,int recovered=0)
    { Command=command; AppliedRevision=revision; PreviousItemId=previous; EquippedItemId=equipped; PendingRecovered=recovered; }
}

public sealed class CommandResult
{
    public string OperationId { get; }
    public CommandOutcome Outcome { get; }
    public ExpeditionCode Code { get; }
    public ExpeditionOperationReceipt? Original { get; }
    public long? AppliedRevision => Original?.AppliedRevision;
    public int PendingRecovered => Original?.PendingRecovered ?? 0;
    // This stage acknowledges memory only. No file adapter or durable promise exists.
    public string Durability => "Volatile";
    public ExpeditionReadModel LatestReadModel { get; }
    internal CommandResult(string operationId,CommandOutcome outcome,ExpeditionCode code,ExpeditionReadModel view,ExpeditionOperationReceipt? receipt=null)
    { OperationId=operationId; Outcome=outcome; Code=code; LatestReadModel=view; Original=receipt; }
}

public sealed class AdvanceResult
{
    public double Consumed { get; }
    public double Discarded { get; }
    public AdvanceStopReason StopReason { get; }
    public long StateRevision { get; }
    public IReadOnlyList<ExpeditionEvent> Events { get; }
    internal AdvanceResult(double consumed,double discarded,AdvanceStopReason reason,long revision,IReadOnlyList<ExpeditionEvent> events)
    { Consumed=consumed; Discarded=discarded; StopReason=reason; StateRevision=revision; Events=events; }
}
