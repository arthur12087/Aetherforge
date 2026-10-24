using System;

namespace Aetherforge.Domain;

/// <summary>GDD 15.4: integer XP, linear thresholds and base-stat growth. No banked XP at cap.</summary>
public sealed class CharacterProgression
{
    public const int MaximumLevel = 20;
    public int Level { get; private set; } = 1;
    public int Experience { get; private set; }
    public int RequiredExperience => Level == MaximumLevel ? 0 : 60 + 30 * (Level - 1);
    public int TotalExperience => SpentExperience(Level) + Experience;
    public static int SpentExperience(int level) => 60 * (level-1) + 15 * (level-1) * (level-2);

    public int GainExperience(int amount)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
        int before = Level;
        int accepted = (int)Math.Min((long)amount, SpentExperience(MaximumLevel)-TotalExperience);
        Experience += accepted;
        while (Level < MaximumLevel && Experience >= RequiredExperience)
        {
            Experience -= RequiredExperience;
            Level++;
        }
        if (Level == MaximumLevel) Experience = 0;
        return Level-before;
    }

    public float ScaleBase(float value, int percentPerLevel)
        => Level == 1 ? value : (float)Math.Floor((double)value * (100 + percentPerLevel * (Level-1)) / 100);
}
