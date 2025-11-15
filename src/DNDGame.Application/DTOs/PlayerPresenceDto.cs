namespace DNDGame.Application.DTOs;

/// <summary>
/// Represents a player's online/offline status.
/// </summary>
public record PlayerPresenceDto
{
    public int PlayerId { get; init; }
    public string PlayerName { get; init; } = string.Empty;
    public int? CharacterId { get; init; }
    public bool IsOnline { get; init; }
    public DateTime LastSeen { get; init; }
    public string? ConnectionId { get; init; }
}
