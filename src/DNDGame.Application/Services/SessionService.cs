using DNDGame.Application.DTOs;
using DNDGame.Core.Entities;
using DNDGame.Core.Enums;
using DNDGame.Core.Interfaces;
using DNDGame.Core.Models;

namespace DNDGame.Application.Services;

public class SessionService : ISessionService
{
    private readonly ISessionRepository _sessionRepository;
    private readonly ICharacterRepository _characterRepository;

    public SessionService(
        ISessionRepository sessionRepository,
        ICharacterRepository characterRepository)
    {
        _sessionRepository = sessionRepository;
        _characterRepository = characterRepository;
    }

    public async Task<Session?> GetSessionAsync(int id)
    {
        return await _sessionRepository.GetByIdAsync(id);
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

    public async Task<Session?> UpdateSessionStateAsync(int id, SessionState state)
    {
        var session = await _sessionRepository.GetByIdAsync(id);
        if (session == null)
            return null;

        session.State = state;
        await _sessionRepository.UpdateAsync(session);
        return session;
    }

    public async Task<bool> DeleteSessionAsync(int id)
    {
        var session = await _sessionRepository.GetByIdAsync(id);
        if (session == null)
            return false;

        await _sessionRepository.DeleteAsync(id);
        return true;
    }

    public async Task<bool> JoinSessionAsync(int sessionId, int characterId)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session == null)
            return false;

        var character = await _characterRepository.GetByIdAsync(characterId);
        if (character == null)
            return false;

        // In a real implementation, this would add to a SessionParticipants table
        // For now, we'll just verify both exist
        return true;
    }

    public async Task<bool> LeaveSessionAsync(int sessionId, int characterId)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session == null)
            return false;

        // In a real implementation, this would remove from SessionParticipants table
        return true;
    }

    public async Task<Message> SaveMessageAsync(int sessionId, string content, MessageRole role)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session == null)
            throw new InvalidOperationException($"Session {sessionId} not found");

        var message = new Message
        {
            SessionId = sessionId,
            AuthorId = "system", // Default to system, would be actual user ID in real impl
            Role = role,
            Content = content,
            Timestamp = DateTime.UtcNow
        };

        session.Messages.Add(message);
        session.LastActivityAt = DateTime.UtcNow;
        await _sessionRepository.UpdateAsync(session);

        return message;
    }

    public async Task<DiceRoll> SaveDiceRollAsync(int sessionId, string formula, DiceRollResult result)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session == null)
            throw new InvalidOperationException($"Session {sessionId} not found");

        var diceRoll = new DiceRoll
        {
            SessionId = sessionId,
            RollerId = "system", // Default to system, would be actual user ID in real impl
            Formula = formula,
            Total = result.Total,
            Modifier = result.Modifier,
            IndividualRolls = System.Text.Json.JsonSerializer.Serialize(result.IndividualRolls),
            Type = DiceRollType.Custom,
            Timestamp = DateTime.UtcNow
        };

        session.DiceRolls.Add(diceRoll);
        session.LastActivityAt = DateTime.UtcNow;
        await _sessionRepository.UpdateAsync(session);

        return diceRoll;
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
