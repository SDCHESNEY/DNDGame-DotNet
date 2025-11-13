namespace DNDGame.Application.DTOs;

/// <summary>
/// Request to generate NPC dialogue
/// </summary>
public record GenerateNpcDialogueRequest
{
    /// <summary>
    /// The session ID for context
    /// </summary>
    public required int SessionId { get; init; }
    
    /// <summary>
    /// Name of the NPC
    /// </summary>
    public required string NpcName { get; init; }
    
    /// <summary>
    /// NPC's personality traits
    /// </summary>
    public required string Personality { get; init; }
    
    /// <summary>
    /// What the players said to the NPC
    /// </summary>
    public required string PlayerMessage { get; init; }
    
    /// <summary>
    /// Optional: NPC's occupation
    /// </summary>
    public string? Occupation { get; init; }
    
    /// <summary>
    /// Optional: NPC's current mood
    /// </summary>
    public string? Mood { get; init; }
    
    /// <summary>
    /// Optional: Additional context about the NPC
    /// </summary>
    public Dictionary<string, string>? Metadata { get; init; }
}
