using DNDGame.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using Polly;
using Polly.Retry;
using System.ClientModel;
using System.Runtime.CompilerServices;

namespace DNDGame.Infrastructure.Services;

/// <summary>
/// OpenAI implementation of the LLM provider interface.
/// Handles communication with OpenAI's chat completion API with retry logic.
/// </summary>
public class OpenAiProvider : ILlmProvider
{
    private readonly ChatClient _chatClient;
    private readonly ILogger<OpenAiProvider> _logger;
    private readonly LlmSettings _settings;
    private readonly AsyncRetryPolicy _retryPolicy;

    public OpenAiProvider(
        IOptions<LlmSettings> settings,
        ILogger<OpenAiProvider> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        // Initialize OpenAI chat client
        _chatClient = new ChatClient(
            model: _settings.Model,
            credential: new ApiKeyCredential(_settings.ApiKey));

        // Configure Polly retry policy for transient failures
        _retryPolicy = Policy
            .Handle<ClientResultException>(ex => 
                ex.Status == 429 || // Too Many Requests
                ex.Status == 500 || // Internal Server Error
                ex.Status == 502 || // Bad Gateway
                ex.Status == 503 || // Service Unavailable
                ex.Status == 504)   // Gateway Timeout
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => 
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        "OpenAI API call failed (attempt {RetryCount}/3). " +
                        "Retrying after {RetryDelay}s. Error: {Error}",
                        retryCount,
                        timeSpan.TotalSeconds,
                        exception.Message);
                });
    }

    public string ProviderName => "OpenAI";

    public string ModelName => _settings.Model;

    public async Task<(string Content, int TokensUsed)> CompleteAsync(
        string systemPrompt,
        string userMessage,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Generating OpenAI completion with model {Model}. " +
            "System prompt length: {SystemLength}, User message length: {UserLength}",
            _settings.Model,
            systemPrompt.Length,
            userMessage.Length);

        try
        {
            var response = await _retryPolicy.ExecuteAsync(async () =>
            {
                var messages = new ChatMessage[]
                {
                    new SystemChatMessage(systemPrompt),
                    new UserChatMessage(userMessage)
                };
                
                var chatCompletion = await _chatClient.CompleteChatAsync(
                    messages,
                    new ChatCompletionOptions
                    {
                        MaxOutputTokenCount = _settings.MaxTokens,
                        Temperature = (float)_settings.Temperature
                    },
                    cancellationToken);

                return chatCompletion.Value;
            });

            var content = response.Content[0].Text;
            var tokensUsed = response.Usage.TotalTokenCount;

            _logger.LogInformation(
                "OpenAI completion generated successfully. " +
                "Tokens used: {TokensUsed}, Response length: {ResponseLength}",
                tokensUsed,
                content.Length);

            return (content, tokensUsed);
        }
        catch (ClientResultException ex)
        {
            _logger.LogError(ex,
                "OpenAI API error: Status {Status}, Message: {Message}",
                ex.Status,
                ex.Message);
            throw new InvalidOperationException(
                $"OpenAI API error (Status: {ex.Status}): {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error calling OpenAI API");
            throw;
        }
    }

    public async IAsyncEnumerable<string> StreamCompleteAsync(
        string systemPrompt,
        string userMessage,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Starting OpenAI streaming completion with model {Model}",
            _settings.Model);

        // Build messages array
        var messages = new ChatMessage[]
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userMessage)
        };

        var options = new ChatCompletionOptions
        {
            MaxOutputTokenCount = _settings.MaxTokens,
            Temperature = (float)_settings.Temperature
        };

        var streamingResult = _chatClient.CompleteChatStreamingAsync(
            messages,
            options,
            cancellationToken);

        var tokenCount = 0;
        await foreach (var update in streamingResult.ConfigureAwait(false))
        {
            foreach (var contentPart in update.ContentUpdate)
            {
                if (!string.IsNullOrEmpty(contentPart.Text))
                {
                    tokenCount++;
                    yield return contentPart.Text;
                }
            }
        }

        _logger.LogInformation(
            "OpenAI streaming completion finished. Approximate tokens: {TokenCount}",
            tokenCount);
    }
}

/// <summary>
/// Configuration settings for LLM providers.
/// </summary>
public class LlmSettings
{
    public const string SectionName = "LLM";

    public string Provider { get; set; } = "OpenAI";
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4-turbo-preview";
    public int MaxTokens { get; set; } = 500;
    public double Temperature { get; set; } = 0.7;
    public bool StreamResponses { get; set; } = true;
}
