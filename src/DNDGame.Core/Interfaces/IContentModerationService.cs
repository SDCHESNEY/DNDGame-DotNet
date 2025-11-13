using DNDGame.Core.Models;

namespace DNDGame.Core.Interfaces;

/// <summary>
/// Interface for content moderation service.
/// Ensures appropriate content in player inputs and LLM outputs.
/// </summary>
public interface IContentModerationService
{
    /// <summary>
    /// Moderates player input before sending to the LLM.
    /// </summary>
    /// <param name="content">The content to moderate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Moderation result indicating if content is safe.</returns>
    Task<ModerationResult> ModerateInputAsync(
        string content,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Moderates LLM output before sending to the player.
    /// </summary>
    /// <param name="content">The content to moderate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Moderation result indicating if content is safe.</returns>
    Task<ModerationResult> ModerateOutputAsync(
        string content,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sanitizes content by removing or replacing inappropriate elements.
    /// </summary>
    /// <param name="content">The content to sanitize.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Sanitized content.</returns>
    Task<string> SanitizeContentAsync(
        string content,
        CancellationToken cancellationToken = default);
}
