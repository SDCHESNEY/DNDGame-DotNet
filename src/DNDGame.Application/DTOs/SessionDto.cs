using DNDGame.Core.Enums;

namespace DNDGame.Application.DTOs;

public record SessionDto(
    int Id,
    string Title,
    SessionMode Mode,
    SessionState State,
    string? CurrentScene,
    int? CurrentTurnCharacterId,
    DateTime CreatedAt,
    DateTime? LastActivityAt
);
