namespace DNDGame.Application.DTOs;

/// <summary>
/// Represents an action taken by a player during gameplay.
/// </summary>
public record PlayerActionDto
{
    public int CharacterId { get; init; }
    public string ActionType { get; init; } = string.Empty; // "attack", "move", "cast_spell", "use_item", etc.
    public string Description { get; init; } = string.Empty;
    public Dictionary<string, object> Parameters { get; init; } = [];
}
