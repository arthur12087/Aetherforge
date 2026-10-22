using Godot;
using System;
using System.Collections.Generic;

/// <summary>Original 24-unit linen/brass symbols, imported once and shared by menus and HUD.</summary>
public static class BrassIcons
{
    private static readonly Dictionary<string, Texture2D> Cache = new(StringComparer.Ordinal);

    public static Texture2D Get(string key)
    {
        if (Cache.TryGetValue(key, out var icon)) return icon;
        if (key != "shield" && key != "rifle" && key != "heal" &&
            key != "pause" && key != "return" && key != "route" && key != "vital")
            throw new ArgumentException($"Unknown Brass icon: {key}", nameof(key));
        icon = GD.Load<Texture2D>($"res://Assets/Brass/Icons/{key}.svg")
            ?? throw new InvalidOperationException($"Missing local Brass icon: {key}");
        Cache.Add(key, icon);
        return icon;
    }
}
