namespace DNDGame.Application.DTOs;

/// <summary>
/// Request to generate a scene description
/// </summary>
public record GenerateSceneDescriptionRequest
{
    /// <summary>
    /// The session ID for context
    /// </summary>
    public required int SessionId { get; init; }
    
    /// <summary>
    /// Name of the location (e.g., "Tavern", "Dark Forest")
    /// </summary>
    public required string LocationName { get; init; }
    
    /// <summary>
    /// Type of location (e.g., "indoor", "outdoor", "dungeon")
    /// </summary>
    public required string LocationType { get; init; }
    
    /// <summary>
    /// Brief description of the location
    /// </summary>
    public string? Description { get; init; }
    
    /// <summary>
    /// Optional: Notable features of the location
    /// </summary>
    public List<string>? Features { get; init; }
    
    /// <summary>
    /// Optional: NPCs present in the scene
    /// </summary>
    public List<string>? NpcsPresent { get; init; }
    
    /// <summary>
    /// Optional: Additional location details
    /// </summary>
    public Dictionary<string, string>? Details { get; init; }
}
