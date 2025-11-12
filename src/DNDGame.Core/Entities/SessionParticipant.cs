namespace DNDGame.Core.Entities;

/// <summary>
/// Represents a character participating in a game session.
/// </summary>
public class SessionParticipant
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the session ID.
    /// </summary>
    public int SessionId { get; set; }

    /// <summary>
    /// Gets or sets the session.
    /// </summary>
    public Session Session { get; set; } = null!;

    /// <summary>
    /// Gets or sets the character ID.
    /// </summary>
    public int CharacterId { get; set; }

    /// <summary>
    /// Gets or sets the character.
    /// </summary>
    public Character Character { get; set; } = null!;

    /// <summary>
    /// Gets or sets when the character joined the session.
    /// </summary>
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets whether the participant is active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
