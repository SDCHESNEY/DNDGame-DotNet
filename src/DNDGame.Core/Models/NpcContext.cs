namespace DNDGame.Core.Models;

/// <summary>
/// Represents the context of an NPC for generating dialogue.
/// </summary>
public record NpcContext(
    string Name,
    string PersonalityTraits,
    string? Occupation,
    string? CurrentMood,
    Dictionary<string, object> Metadata)
{
    /// <summary>
    /// Gets whether the NPC has a defined occupation.
    /// </summary>
    public bool HasOccupation => !string.IsNullOrEmpty(Occupation);

    /// <summary>
    /// Gets whether the NPC has a current mood specified.
    /// </summary>
    public bool HasMood => !string.IsNullOrEmpty(CurrentMood);

    /// <summary>
    /// Gets metadata value by key.
    /// </summary>
    public T? GetMetadata<T>(string key) where T : class
    {
        return Metadata.TryGetValue(key, out var value) ? value as T : null;
    }

    /// <summary>
    /// Creates a simple NPC context with minimal information.
    /// </summary>
    public static NpcContext Simple(string name, string personalityTraits)
    {
        return new NpcContext(
            name,
            personalityTraits,
            null,
            null,
            new Dictionary<string, object>());
    }
}
