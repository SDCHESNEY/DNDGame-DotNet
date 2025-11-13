namespace DNDGame.Application.DTOs;

/// <summary>
/// Dungeon Master response with metadata
/// </summary>
public record DmResponseDto
{
    /// <summary>
    /// The DM's narrative response
    /// </summary>
    public required string Content { get; init; }
    
    /// <summary>
    /// Suggested player actions based on the context
    /// </summary>
    public List<string> SuggestedActions { get; init; } = [];
    
    /// <summary>
    /// Number of AI tokens used for this response
    /// </summary>
    public int TokensUsed { get; init; }
    
    /// <summary>
    /// Time taken to generate the response in milliseconds
    /// </summary>
    public long ResponseTimeMs { get; init; }
    
    /// <summary>
    /// Estimated cost in USD (based on token usage)
    /// </summary>
    public decimal EstimatedCost { get; init; }
    
    /// <summary>
    /// Whether the response was flagged by content moderation
    /// </summary>
    public bool WasModerated { get; init; }
}
