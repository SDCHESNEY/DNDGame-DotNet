namespace DNDGame.Core.Models;

/// <summary>
/// Represents the result of content moderation.
/// </summary>
public record ModerationResult(
    bool IsSafe,
    List<string> Violations,
    string? SanitizedContent = null)
{
    /// <summary>
    /// Gets whether any violations were detected.
    /// </summary>
    public bool HasViolations => Violations.Any();

    /// <summary>
    /// Gets the number of violations detected.
    /// </summary>
    public int ViolationCount => Violations.Count;

    /// <summary>
    /// Gets whether the content was sanitized.
    /// </summary>
    public bool WasSanitized => !string.IsNullOrEmpty(SanitizedContent);

    /// <summary>
    /// Creates a safe moderation result.
    /// </summary>
    public static ModerationResult Safe()
    {
        return new ModerationResult(true, new List<string>());
    }

    /// <summary>
    /// Creates an unsafe moderation result with violations.
    /// </summary>
    public static ModerationResult Unsafe(params string[] violations)
    {
        return new ModerationResult(false, violations.ToList());
    }

    /// <summary>
    /// Creates a sanitized result.
    /// </summary>
    public static ModerationResult Sanitized(string sanitizedContent, params string[] violations)
    {
        return new ModerationResult(true, violations.ToList(), sanitizedContent);
    }
}
