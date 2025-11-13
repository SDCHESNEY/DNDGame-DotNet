using DNDGame.Core.Enums;

namespace DNDGame.Core.Models;

/// <summary>
/// Represents a response from the AI Dungeon Master.
/// </summary>
public record DmResponse(
    string Content,
    MessageRole Role,
    int TokensUsed,
    TimeSpan ResponseTime,
    List<string> SuggestedActions)
{
    /// <summary>
    /// Creates a DM response with default role (DungeonMaster).
    /// </summary>
    public static DmResponse Create(
        string content,
        int tokensUsed,
        TimeSpan responseTime,
        List<string>? suggestedActions = null)
    {
        return new DmResponse(
            content,
            MessageRole.DungeonMaster,
            tokensUsed,
            responseTime,
            suggestedActions ?? new List<string>());
    }

    /// <summary>
    /// Gets whether the response contains any suggested actions.
    /// </summary>
    public bool HasSuggestedActions => SuggestedActions.Any();

    /// <summary>
    /// Gets the response time in milliseconds.
    /// </summary>
    public double ResponseTimeMs => ResponseTime.TotalMilliseconds;

    /// <summary>
    /// Gets the estimated cost of the response (approximate OpenAI GPT-4 pricing).
    /// </summary>
    public decimal EstimatedCost => TokensUsed * 0.00003m; // $0.03 per 1K tokens average
}
