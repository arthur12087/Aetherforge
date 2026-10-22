using Godot;
using System;

/// <summary>Window-local shortcuts through the same viewport input path as controls.</summary>
public partial class ExpeditionMenuInput : Node
{
    public Action<InputEvent>? Handle { get; set; }
    public override void _Input(InputEvent input) => Handle?.Invoke(input);
}
