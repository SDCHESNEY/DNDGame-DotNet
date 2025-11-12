namespace DNDGame.Core.Models;

/// <summary>
/// Represents the result of an attack roll.
/// </summary>
public record AttackResult
{
    /// <summary>
    /// Gets the total attack roll.
    /// </summary>
    public required int AttackRoll { get; init; }

    /// <summary>
    /// Gets the target's armor class.
    /// </summary>
    public required int TargetAC { get; init; }

    /// <summary>
    /// Gets whether the attack hit.
    /// </summary>
    public required bool Hit { get; init; }

    /// <summary>
    /// Gets whether this was a critical hit (natural 20).
    /// </summary>
    public bool IsCritical { get; init; }

    /// <summary>
    /// Gets whether this was a critical miss (natural 1).
    /// </summary>
    public bool IsFumble { get; init; }

    /// <summary>
    /// Gets the damage dealt (if hit).
    /// </summary>
    public int Damage { get; init; }

    /// <summary>
    /// Gets the damage type.
    /// </summary>
    public string? DamageType { get; init; }
}
