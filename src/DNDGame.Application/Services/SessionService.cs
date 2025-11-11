using DNDGame.Application.DTOs;
using DNDGame.Core.Entities;
using DNDGame.Core.Enums;
using DNDGame.Core.Interfaces;

namespace DNDGame.Application.Services;

public class SessionService : ISessionService
{
    private readonly ISessionRepository _sessionRepository;

    public SessionService(ISessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public async Task<object?> GetSessionAsync(int id)
    {
        var session = await _sessionRepository.GetByIdAsync(id);
        return session == null ? null : MapToDto(session);
    }

    public async Task<IEnumerable<object>> GetAllSessionsAsync()
    {
        var sessions = await _sessionRepository.GetAllAsync();
        return sessions.Select(s => (object)MapToDto(s));
    }

    public async Task<object> CreateSessionAsync(object request)
    {
        var dto = (CreateSessionRequest)request;
        var session = new Session
        {
            Title = dto.Title,
            Mode = dto.Mode,
            State = SessionState.Created,
            CreatedAt = DateTime.UtcNow,
            Messages = new List<Message>(),
            DiceRolls = new List<DiceRoll>(),
            WorldFlags = "{}"
        };

        await _sessionRepository.AddAsync(session);
        return MapToDto(session);
    }

    public async Task<object?> UpdateSessionStateAsync(int id, SessionState state)
    {
        var session = await _sessionRepository.GetByIdAsync(id);
        if (session == null)
            return null;

        session.State = state;
        await _sessionRepository.UpdateAsync(session);
        return MapToDto(session);
    }

    public async Task<bool> DeleteSessionAsync(int id)
    {
        var session = await _sessionRepository.GetByIdAsync(id);
        if (session == null)
            return false;

        await _sessionRepository.DeleteAsync(id);
        return true;
    }

    private static SessionDto MapToDto(Session session)
    {
        return new SessionDto(
            session.Id,
            session.Title,
            session.Mode,
            session.State,
            session.CurrentScene,
            session.CurrentTurnCharacterId,
            session.CreatedAt,
            session.LastActivityAt
        );
    }
}
