using DNDGame.Core.Entities;

namespace DNDGame.Core.Models;

/// <summary>
/// Represents the context of a game session for LLM processing.
/// Contains all information needed for the DM to generate appropriate responses.
/// </summary>
public record SessionContext(
    int SessionId,
    List<Message> RecentMessages,
    List<Character> ActiveCharacters,
    string? CurrentScene,
    Dictionary<string, object> WorldFlags)
{
    /// <summary>
    /// Gets the most recent message from the session.
    /// </summary>
    public Message? LastMessage => RecentMessages.LastOrDefault();

    /// <summary>
    /// Gets the number of messages in the context.
    /// </summary>
    public int MessageCount => RecentMessages.Count;

    /// <summary>
    /// Gets the number of active characters in the session.
    /// </summary>
    public int CharacterCount => ActiveCharacters.Count;

    /// <summary>
    /// Checks if a world flag exists with the given key.
    /// </summary>
    public bool HasWorldFlag(string key) => WorldFlags.ContainsKey(key);

    /// <summary>
    /// Gets a world flag value by key.
    /// </summary>
    public T? GetWorldFlag<T>(string key) where T : class
    {
        return WorldFlags.TryGetValue(key, out var value) ? value as T : null;
    }
}
