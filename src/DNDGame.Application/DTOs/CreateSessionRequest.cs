using DNDGame.Core.Enums;

namespace DNDGame.Application.DTOs;

public record CreateSessionRequest(
    string Title,
    SessionMode Mode
);
