namespace DNDGame.Application.DTOs;

/// <summary>
/// Represents a character's position in the initiative order.
/// </summary>
public record InitiativeEntryDto
{
    public int CharacterId { get; init; }
    public string CharacterName { get; init; } = string.Empty;
    public int InitiativeRoll { get; init; }
    public int CurrentHP { get; init; }
    public int MaxHP { get; init; }
    public List<string> Conditions { get; init; } = [];
}
