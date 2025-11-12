using DNDGame.Core.Enums;

namespace DNDGame.Core.Entities;

/// <summary>
/// Represents a condition affecting a character in D&D 5e.
/// </summary>
public class Condition
{
    /// <summary>
    /// Gets or sets the unique identifier for this condition instance.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the character ID this condition is applied to.
    /// </summary>
    public int CharacterId { get; set; }

    /// <summary>
    /// Gets or sets the character this condition is applied to.
    /// </summary>
    public Character Character { get; set; } = null!;

    /// <summary>
    /// Gets or sets the type of condition.
    /// </summary>
    public ConditionType Type { get; set; }

    /// <summary>
    /// Gets or sets the duration in rounds (null for indefinite).
    /// </summary>
    public int? Duration { get; set; }

    /// <summary>
    /// Gets or sets the DC for saving throws to end the condition (if applicable).
    /// </summary>
    public int? SaveDC { get; set; }

    /// <summary>
    /// Gets or sets when the condition was applied.
    /// </summary>
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the source of the condition (spell name, ability, etc.).
    /// </summary>
    public string? Source { get; set; }
}
