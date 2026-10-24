using Aetherforge.Domain;
using System;
using System.Collections.Generic;

/// <summary>UI lifetime, separate from the domain revision. A → B → A never revives an old gesture.</summary>
public sealed class EquipmentIntent
{
    public long Epoch { get; }
    public EquipmentPreview Preview { get; }
    public EquipmentCommand Command { get; }
    internal EquipmentIntent(long epoch, EquipmentPreview preview)
    { Epoch=epoch; Preview=preview; Command=preview.Confirm(Guid.NewGuid().ToString("N")); }
}

public sealed class EquipmentSelection
{
    public long Epoch { get; private set; }
    public EquipmentIntent? Current { get; private set; }
    private readonly HashSet<string> _feedback = new();
    public int AppliedFeedbackCount { get; private set; }
    public void Invalidate() { Epoch++; Current=null; }
    public EquipmentIntent Present(EquipmentPreview preview)
    { Invalidate(); return Current=new EquipmentIntent(Epoch,preview); }
    public bool IsCurrent(EquipmentIntent intent) => ReferenceEquals(Current,intent) && intent.Epoch==Epoch;
    public CommandResult? Confirm(ExpeditionSimulation simulation, EquipmentIntent intent)
    {
        if (!IsCurrent(intent)) return null;
        // Never query here. Only the immutable envelope of the displayed preview may be sent.
        Invalidate();
        return simulation.Execute(intent.Command);
    }
    public bool AcceptFeedback(CommandResult result)
    {
        if(result.Outcome!=CommandOutcome.Applied || !_feedback.Add(result.LatestReadModel.SessionId+":"+result.OperationId)) return false;
        AppliedFeedbackCount++; return true;
    }
}
