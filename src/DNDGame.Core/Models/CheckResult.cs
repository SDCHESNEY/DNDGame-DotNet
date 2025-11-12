namespace DNDGame.Core.Models;

/// <summary>
/// Represents the result of an ability check or saving throw.
/// </summary>
public record CheckResult
{
    /// <summary>
    /// Gets the total result of the check.
    /// </summary>
    public required int Total { get; init; }

    /// <summary>
    /// Gets the die roll result (before modifiers).
    /// </summary>
    public required int Roll { get; init; }

    /// <summary>
    /// Gets the ability modifier applied.
    /// </summary>
    public required int AbilityModifier { get; init; }

    /// <summary>
    /// Gets the proficiency bonus if applicable.
    /// </summary>
    public int ProficiencyBonus { get; init; }

    /// <summary>
    /// Gets the difficulty class (DC) that was being checked against.
    /// </summary>
    public int? DifficultyClass { get; init; }

    /// <summary>
    /// Gets whether the check succeeded.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets whether this was a critical success (natural 20).
    /// </summary>
    public bool IsCritical { get; init; }

    /// <summary>
    /// Gets whether this was a critical failure (natural 1).
    /// </summary>
    public bool IsFumble { get; init; }
}
