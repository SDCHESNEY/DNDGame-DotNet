using DNDGame.Core.Interfaces;
using DNDGame.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace DNDGame.Application.Services;

/// <summary>
/// Service for moderating content to ensure appropriate gameplay.
/// Filters inappropriate content from player inputs and LLM outputs.
/// </summary>
public class ContentModerationService : IContentModerationService
{
    private readonly ILogger<ContentModerationService> _logger;
    private readonly ContentModerationSettings _settings;
    
    // Basic blocklists for demonstration - in production, use more sophisticated methods
    private readonly HashSet<string> _nsfwKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "explicit", "nsfw", "sexual", "nude", "naked"
        // Add more keywords as needed
    };

    private readonly HashSet<string> _harassmentKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "kill yourself", "kys", "hate", "racist", "slur"
        // Add more keywords as needed
    };

    public ContentModerationService(
        IOptions<ContentModerationSettings> settings,
        ILogger<ContentModerationService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<ModerationResult> ModerateInputAsync(
        string content,
        CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled)
        {
            return ModerationResult.Safe();
        }

        _logger.LogDebug("Moderating input content (length: {Length})", content.Length);

        var violations = new List<string>();

        // Check for NSFW content
        if (_settings.BlockNsfw && ContainsBlockedKeywords(content, _nsfwKeywords))
        {
            violations.Add("NSFW content detected");
            _logger.LogWarning("NSFW content blocked in player input");
        }

        // Check for harassment
        if (_settings.BlockHarassment && ContainsBlockedKeywords(content, _harassmentKeywords))
        {
            violations.Add("Harassment or hate speech detected");
            _logger.LogWarning("Harassment content blocked in player input");
        }

        // Check content length
        if (content.Length > _settings.MaxInputLength)
        {
            violations.Add($"Input too long (max {_settings.MaxInputLength} characters)");
        }

        await Task.CompletedTask; // For async interface compliance

        if (violations.Any())
        {
            return ModerationResult.Unsafe(violations.ToArray());
        }

        return ModerationResult.Safe();
    }

    public async Task<ModerationResult> ModerateOutputAsync(
        string content,
        CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled)
        {
            return ModerationResult.Safe();
        }

        _logger.LogDebug("Moderating output content (length: {Length})", content.Length);

        var violations = new List<string>();

        // Check for inappropriate LLM responses
        if (_settings.BlockNsfw && ContainsBlockedKeywords(content, _nsfwKeywords))
        {
            violations.Add("Inappropriate content in LLM response");
            _logger.LogWarning("NSFW content detected in LLM output");
        }

        // Check for accidentally generated harassment
        if (_settings.BlockHarassment && ContainsBlockedKeywords(content, _harassmentKeywords))
        {
            violations.Add("Potentially harmful content in LLM response");
            _logger.LogWarning("Harassment content detected in LLM output");
        }

        await Task.CompletedTask; // For async interface compliance

        if (violations.Any())
        {
            // For output, we can try to sanitize instead of just blocking
            var sanitized = await SanitizeContentAsync(content, cancellationToken);
            return ModerationResult.Sanitized(sanitized, violations.ToArray());
        }

        return ModerationResult.Safe();
    }

    public async Task<string> SanitizeContentAsync(
        string content,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Sanitizing content (length: {Length})", content.Length);

        var sanitized = content;

        // Replace blocked keywords with [REDACTED]
        foreach (var keyword in _nsfwKeywords.Concat(_harassmentKeywords))
        {
            var pattern = $@"\b{Regex.Escape(keyword)}\b";
            sanitized = Regex.Replace(
                sanitized,
                pattern,
                "[REDACTED]",
                RegexOptions.IgnoreCase);
        }

        // Remove excessive profanity markers if any slipped through
        sanitized = Regex.Replace(sanitized, @"\*{3,}", "***");

        await Task.CompletedTask; // For async interface compliance

        if (sanitized != content)
        {
            _logger.LogInformation(
                "Content sanitized. Original length: {Original}, Sanitized length: {Sanitized}",
                content.Length,
                sanitized.Length);
        }

        return sanitized;
    }

    private bool ContainsBlockedKeywords(string content, HashSet<string> keywords)
    {
        var contentLower = content.ToLower();
        
        foreach (var keyword in keywords)
        {
            // Use word boundary matching to avoid false positives
            var pattern = $@"\b{Regex.Escape(keyword)}\b";
            if (Regex.IsMatch(contentLower, pattern, RegexOptions.IgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}

/// <summary>
/// Configuration settings for content moderation.
/// </summary>
public class ContentModerationSettings
{
    public const string SectionName = "ContentModeration";

    public bool Enabled { get; set; } = true;
    public bool BlockNsfw { get; set; } = true;
    public bool BlockHarassment { get; set; } = true;
    public int MaxInputLength { get; set; } = 5000;
}
