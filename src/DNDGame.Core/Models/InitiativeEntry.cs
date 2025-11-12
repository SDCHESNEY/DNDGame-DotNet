namespace DNDGame.Core.Models;

/// <summary>
/// Represents a character's initiative in combat.
/// </summary>
public record InitiativeEntry
{
    /// <summary>
    /// Gets the character's ID.
    /// </summary>
    public required int CharacterId { get; init; }

    /// <summary>
    /// Gets the character's name.
    /// </summary>
    public required string CharacterName { get; init; }

    /// <summary>
    /// Gets the initiative roll result.
    /// </summary>
    public required int InitiativeRoll { get; init; }

    /// <summary>
    /// Gets the character's current hit points.
    /// </summary>
    public int CurrentHP { get; init; }

    /// <summary>
    /// Gets the character's maximum hit points.
    /// </summary>
    public int MaxHP { get; init; }

    /// <summary>
    /// Gets the list of active conditions on the character.
    /// </summary>
    public List<string> Conditions { get; init; } = [];
}
