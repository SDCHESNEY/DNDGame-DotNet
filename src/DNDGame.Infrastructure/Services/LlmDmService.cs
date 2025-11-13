using DNDGame.Core.Enums;
using DNDGame.Core.Interfaces;
using DNDGame.Core.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DNDGame.Infrastructure.Services;

/// <summary>
/// AI Dungeon Master service that orchestrates LLM provider, prompt templates, and content moderation.
/// Generates narrative responses for player actions in D&D game sessions.
/// </summary>
public class LlmDmService : ILlmDmService
{
    private readonly ILlmProvider _llmProvider;
    private readonly IPromptTemplateService _promptTemplateService;
    private readonly IContentModerationService _contentModerationService;
    private readonly ILogger<LlmDmService> _logger;

    public LlmDmService(
        ILlmProvider llmProvider,
        IPromptTemplateService promptTemplateService,
        IContentModerationService contentModerationService,
        ILogger<LlmDmService> logger)
    {
        _llmProvider = llmProvider;
        _promptTemplateService = promptTemplateService;
        _contentModerationService = contentModerationService;
        _logger = logger;
    }

    public async Task<DmResponse> GenerateResponseAsync(
        SessionContext context,
        string playerAction,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Generating DM response for session {SessionId}. Player action length: {Length}",
            context.SessionId,
            playerAction.Length);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Step 1: Moderate player input
            var inputModeration = await _contentModerationService.ModerateInputAsync(
                playerAction,
                cancellationToken);

            if (!inputModeration.IsSafe)
            {
                _logger.LogWarning(
                    "Player input blocked due to moderation: {Violations}",
                    string.Join(", ", inputModeration.Violations));

                throw new InvalidOperationException(
                    $"Content moderation failed: {string.Join(", ", inputModeration.Violations)}");
            }

            // Step 2: Determine session mode from context
            var sessionMode = DetermineSessionMode(context);

            // Step 3: Build system prompt
            var systemPrompt = _promptTemplateService.GetSystemPrompt(sessionMode);

            // Step 4: Build user message with context
            var userMessage = BuildUserMessage(context, playerAction);

            // Step 5: Get LLM response
            var (content, tokensUsed) = await _llmProvider.CompleteAsync(
                systemPrompt,
                userMessage,
                cancellationToken);

            // Step 6: Moderate LLM output
            var outputModeration = await _contentModerationService.ModerateOutputAsync(
                content,
                cancellationToken);

            var finalContent = outputModeration.WasSanitized 
                ? outputModeration.SanitizedContent!
                : content;

            var wasModerated = outputModeration.HasViolations;

            if (outputModeration.HasViolations)
            {
                _logger.LogWarning(
                    "LLM output sanitized: {Violations}",
                    string.Join(", ", outputModeration.Violations));
            }

            // Step 7: Extract suggested actions (simple parsing)
            var suggestedActions = ExtractSuggestedActions(finalContent);

            stopwatch.Stop();

            var response = DmResponse.Create(
                finalContent,
                tokensUsed,
                stopwatch.Elapsed,
                suggestedActions,
                wasModerated);

            _logger.LogInformation(
                "DM response generated successfully. Tokens: {Tokens}, Time: {Time}ms",
                tokensUsed,
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error generating DM response for session {SessionId}", context.SessionId);
            throw;
        }
    }

    public async IAsyncEnumerable<string> StreamResponseAsync(
        SessionContext context,
        string playerAction,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Starting streaming DM response for session {SessionId}",
            context.SessionId);

        // Step 1: Moderate player input
        var inputModeration = await _contentModerationService.ModerateInputAsync(
            playerAction,
            cancellationToken);

        if (!inputModeration.IsSafe)
        {
            _logger.LogWarning(
                "Player input blocked during streaming: {Violations}",
                string.Join(", ", inputModeration.Violations));

            yield return $"[Content blocked: {string.Join(", ", inputModeration.Violations)}]";
            yield break;
        }

        // Step 2: Build prompts
        var sessionMode = DetermineSessionMode(context);
        var systemPrompt = _promptTemplateService.GetSystemPrompt(sessionMode);
        var userMessage = BuildUserMessage(context, playerAction);

        // Step 3: Stream from LLM
        var fullResponse = new System.Text.StringBuilder();

        await foreach (var chunk in _llmProvider.StreamCompleteAsync(
            systemPrompt,
            userMessage,
            cancellationToken))
        {
            fullResponse.Append(chunk);
            yield return chunk;
        }

        // Step 4: Moderate complete response (logged only, already streamed)
        var outputModeration = await _contentModerationService.ModerateOutputAsync(
            fullResponse.ToString(),
            cancellationToken);

        if (outputModeration.HasViolations)
        {
            _logger.LogWarning(
                "Streamed LLM output contained violations: {Violations}",
                string.Join(", ", outputModeration.Violations));
        }

        _logger.LogInformation(
            "Streaming DM response completed for session {SessionId}",
            context.SessionId);
    }

    public async Task<DmResponse> GenerateNpcDialogueAsync(
        SessionContext context,
        NpcContext npc,
        string playerMessage,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Generating NPC dialogue for {NpcName} in session {SessionId}",
            npc.Name,
            context.SessionId);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Moderate input
            var inputModeration = await _contentModerationService.ModerateInputAsync(
                playerMessage,
                cancellationToken);

            if (!inputModeration.IsSafe)
            {
                throw new InvalidOperationException(
                    $"Player message blocked: {string.Join(", ", inputModeration.Violations)}");
            }

            // Build NPC prompt
            var npcPrompt = _promptTemplateService.GetNpcPrompt(npc, playerMessage);

            // Use a simple system prompt for NPC dialogue
            var systemPrompt = "You are a skilled actor playing an NPC in a D&D game. " +
                              "Stay in character and respond naturally to the player.";

            // Get LLM response
            var (content, tokensUsed) = await _llmProvider.CompleteAsync(
                systemPrompt,
                npcPrompt,
                cancellationToken);

            // Moderate output
            var outputModeration = await _contentModerationService.ModerateOutputAsync(
                content,
                cancellationToken);

            var finalContent = outputModeration.WasSanitized 
                ? outputModeration.SanitizedContent!
                : content;

            stopwatch.Stop();

            _logger.LogInformation(
                "NPC dialogue generated for {NpcName}. Tokens: {Tokens}, Time: {Ms}ms",
                npc.Name,
                tokensUsed,
                stopwatch.ElapsedMilliseconds);

            var response = DmResponse.Create(finalContent, tokensUsed, stopwatch.Elapsed);
            return response;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error generating NPC dialogue for {NpcName}", npc.Name);
            throw;
        }
    }

    public async Task<DmResponse> DescribeSceneAsync(
        SessionContext context,
        LocationContext location,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Generating scene description for {LocationName} in session {SessionId}",
            location.Name,
            context.SessionId);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Build scene prompt
            var scenePrompt = _promptTemplateService.GetScenePrompt(location);

            // Use a descriptive system prompt
            var systemPrompt = "You are an expert at creating vivid, immersive scene descriptions " +
                              "for D&D games. Paint a picture with words that engages all the senses.";

            // Get LLM response
            var (content, tokensUsed) = await _llmProvider.CompleteAsync(
                systemPrompt,
                scenePrompt,
                cancellationToken);

            // Moderate output
            var outputModeration = await _contentModerationService.ModerateOutputAsync(
                content,
                cancellationToken);

            var finalContent = outputModeration.WasSanitized 
                ? outputModeration.SanitizedContent!
                : content;

            stopwatch.Stop();

            _logger.LogInformation(
                "Scene description generated for {LocationName}. Tokens: {Tokens}, Time: {Ms}ms",
                location.Name,
                tokensUsed,
                stopwatch.ElapsedMilliseconds);

            var response = DmResponse.Create(finalContent, tokensUsed, stopwatch.Elapsed);
            return response;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error generating scene description for {LocationName}", location.Name);
            throw;
        }
    }

    private SessionMode DetermineSessionMode(SessionContext context)
    {
        // Simple heuristic: if there's only one character, it's solo mode
        return context.CharacterCount == 1 ? SessionMode.Solo : SessionMode.Multiplayer;
    }

    private string BuildUserMessage(SessionContext context, string playerAction)
    {
        var message = new System.Text.StringBuilder();

        // Add formatted context
        message.AppendLine(_promptTemplateService.FormatContext(context));
        message.AppendLine();

        // Determine scenario type and add appropriate prompt
        if (IsInCombat(context))
        {
            message.AppendLine(_promptTemplateService.GetCombatPrompt(context));
        }
        else
        {
            message.AppendLine(_promptTemplateService.GetExplorationPrompt(context));
        }

        message.AppendLine();
        message.AppendLine("=== PLAYER ACTION ===");
        message.AppendLine(playerAction);

        return message.ToString();
    }

    private bool IsInCombat(SessionContext context)
    {
        // Check world flags for combat state
        if (context.HasWorldFlag("InCombat"))
        {
            var inCombatFlag = context.GetWorldFlag<object>("InCombat");
            if (inCombatFlag is bool inCombat)
            {
                return inCombat;
            }
        }

        // Check if recent messages mention combat keywords
        var recentContent = string.Join(" ", 
            context.RecentMessages.TakeLast(3).Select(m => m.Content.ToLower()));

        return recentContent.Contains("attack") || 
               recentContent.Contains("combat") || 
               recentContent.Contains("initiative") ||
               recentContent.Contains("damage");
    }

    private List<string> ExtractSuggestedActions(string content)
    {
        // Simple extraction: look for sentences ending with "?" or action verbs
        var suggestions = new List<string>();

        // Look for questions (potential prompts for player actions)
        var sentences = content.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var sentence in sentences)
        {
            var trimmed = sentence.Trim();
            if (trimmed.Contains("you", StringComparison.OrdinalIgnoreCase) &&
                (trimmed.Contains("do", StringComparison.OrdinalIgnoreCase) ||
                 trimmed.Contains("want", StringComparison.OrdinalIgnoreCase) ||
                 trimmed.Contains("could", StringComparison.OrdinalIgnoreCase)))
            {
                suggestions.Add(trimmed);
                
                if (suggestions.Count >= 3)
                    break;
            }
        }

        return suggestions;
    }
}
