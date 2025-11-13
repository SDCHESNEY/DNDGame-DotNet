using DNDGame.Core.Models;

namespace DNDGame.Core.Interfaces;

/// <summary>
/// Interface for LLM provider implementations (OpenAI, Anthropic, etc.).
/// </summary>
public interface ILlmProvider
{
    /// <summary>
    /// Generates a completion response from the LLM.
    /// </summary>
    /// <param name="systemPrompt">The system prompt defining the LLM's behavior.</param>
    /// <param name="userMessage">The user's message/query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The LLM's response text and token count.</returns>
    Task<(string Content, int TokensUsed)> CompleteAsync(
        string systemPrompt,
        string userMessage,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a streaming completion response from the LLM.
    /// </summary>
    /// <param name="systemPrompt">The system prompt defining the LLM's behavior.</param>
    /// <param name="userMessage">The user's message/query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async enumerable of response chunks.</returns>
    IAsyncEnumerable<string> StreamCompleteAsync(
        string systemPrompt,
        string userMessage,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the name of the LLM provider (e.g., "OpenAI", "Anthropic").
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Gets the model name being used (e.g., "gpt-4-turbo-preview").
    /// </summary>
    string ModelName { get; }
}
