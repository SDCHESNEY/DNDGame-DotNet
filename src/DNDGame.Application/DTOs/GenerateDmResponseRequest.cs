namespace DNDGame.Application.DTOs;

/// <summary>
/// Request to generate a Dungeon Master response to player actions
/// </summary>
public record GenerateDmResponseRequest
{
    /// <summary>
    /// The session ID for context and history
    /// </summary>
    public required int SessionId { get; init; }
    
    /// <summary>
    /// The player's action or message
    /// </summary>
    public required string PlayerMessage { get; init; }
    
    /// <summary>
    /// Optional character ID if a specific character is taking the action
    /// </summary>
    public int? CharacterId { get; init; }
    
    /// <summary>
    /// Whether to stream the response in real-time
    /// </summary>
    public bool Stream { get; init; } = false;
}
