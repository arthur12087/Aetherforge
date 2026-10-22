using Godot;
using System;
using System.Collections.Generic;

/// <summary>Local, real OpenType weights. No synthetic bold or system-font substitution.</summary>
public static class BrassTypography
{
    public const int WeightAxis = 0x77676874; // OpenType wght: numeric key survives the native dictionary path.
    public const int TabularFeature = 0x746E756D;
    public const int BodySize = 14;
    public const int CriticalSize = 13;
    public const int NameSize = 18;
    public const int DataSize = 28;
    public const int ButtonHeight = 36;

    private static readonly Dictionary<string, FontFile> Files = new();
    private static readonly Dictionary<(string Family, int Weight), FontVariation> Weights = new();
    public static Font Body => Weight("SourceSans3", 400);
    public static Font Strong => Weight("SourceSans3", 600);
    public static Font Display => Load("Limelight-Regular.ttf");

    /// <summary>Also used by the native A/B specimen; both families use identical real weights.</summary>
    public static Font Weight(string family, int weight)
    {
        if (family != "SourceSans3" && family != "Jost")
            throw new ArgumentException("Expected SourceSans3 or Jost.", nameof(family));
        if (weight != 400 && weight != 600)
            throw new ArgumentOutOfRangeException(nameof(weight), "The UI uses real weights 400 and 600.");
        var key = (family, weight);
        if (Weights.TryGetValue(key, out var cached)) return cached;
        // Source Sans 3's file defaults to 200, so even Regular MUST specify wght.
        var font = new FontVariation
        {
            BaseFont = Load($"{family}-Variable.ttf"),
            VariationOpentype = new Godot.Collections.Dictionary { [WeightAxis] = weight },
            // Jost provides tnum. This Source Sans file has tabular digits by default
            // (equal hmtx advances) and has no tnum substitution to request.
            OpentypeFeatures = family == "Jost"
                ? new Godot.Collections.Dictionary { [TabularFeature] = 1 }
                : new Godot.Collections.Dictionary(),
            VariationEmbolden = 0
        };
        Weights.Add(key, font);
        return font;
    }

    private static FontFile Load(string file)
    {
        if (Files.TryGetValue(file, out var cached)) return cached;
        var font = GD.Load<FontFile>($"res://Assets/Brass/Fonts/{file}")
            ?? throw new InvalidOperationException($"Missing local Brass font: {file}");
        font.AllowSystemFallback = false;
        Files.Add(file, font);
        return font;
    }
}
