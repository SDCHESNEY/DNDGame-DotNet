namespace DNDGame.Core.Models;

/// <summary>
/// Represents the result of a dice roll.
/// </summary>
public record DiceRollResult
{
    /// <summary>
    /// Gets the original formula used for the roll.
    /// </summary>
    public required string Formula { get; init; }

    /// <summary>
    /// Gets the total result of the roll (including modifier).
    /// </summary>
    public required int Total { get; init; }

    /// <summary>
    /// Gets the individual die results before applying the modifier.
    /// </summary>
    public required int[] IndividualRolls { get; init; }

    /// <summary>
    /// Gets the modifier applied to the roll.
    /// </summary>
    public required int Modifier { get; init; }

    /// <summary>
    /// Gets the timestamp when the roll occurred.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets whether this was a critical hit (natural 20 on d20).
    /// </summary>
    public bool IsCritical { get; init; }

    /// <summary>
    /// Gets whether this was a critical fumble (natural 1 on d20).
    /// </summary>
    public bool IsFumble { get; init; }
}
