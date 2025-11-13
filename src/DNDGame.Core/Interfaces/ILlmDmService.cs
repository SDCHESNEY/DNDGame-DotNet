using DNDGame.Core.Models;

namespace DNDGame.Core.Interfaces;

/// <summary>
/// Interface for the AI Dungeon Master service.
/// Orchestrates LLM providers, templates, and moderation for game narration.
/// </summary>
public interface ILlmDmService
{
    /// <summary>
    /// Generates a DM response for a player action within a session.
    /// </summary>
    /// <param name="context">The session context including messages and characters.</param>
    /// <param name="playerAction">The player's action or message.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The DM's response.</returns>
    Task<DmResponse> GenerateResponseAsync(
        SessionContext context,
        string playerAction,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a streaming DM response for a player action.
    /// </summary>
    /// <param name="context">The session context including messages and characters.</param>
    /// <param name="playerAction">The player's action or message.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async enumerable of response chunks.</returns>
    IAsyncEnumerable<string> StreamResponseAsync(
        SessionContext context,
        string playerAction,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates NPC dialogue based on context and player message.
    /// </summary>
    /// <param name="context">The session context.</param>
    /// <param name="npc">The NPC context.</param>
    /// <param name="playerMessage">The player's message to the NPC.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The NPC's dialogue response as a DmResponse.</returns>
    Task<DmResponse> GenerateNpcDialogueAsync(
        SessionContext context,
        NpcContext npc,
        string playerMessage,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a scene description for a location.
    /// </summary>
    /// <param name="context">The session context.</param>
    /// <param name="location">The location context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The scene description as a DmResponse.</returns>
    Task<DmResponse> DescribeSceneAsync(
        SessionContext context,
        LocationContext location,
        CancellationToken cancellationToken = default);
}
