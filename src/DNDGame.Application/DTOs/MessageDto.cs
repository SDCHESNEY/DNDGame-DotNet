namespace DNDGame.Application.DTOs;

using DNDGame.Core.Enums;

/// <summary>
/// Data transfer object for chat messages.
/// </summary>
public record MessageDto
{
    public int Id { get; init; }
    public int SessionId { get; init; }
    public MessageRole Role { get; init; }
    public string Content { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
    public string? PlayerName { get; init; }
    public int? CharacterId { get; init; }
}
