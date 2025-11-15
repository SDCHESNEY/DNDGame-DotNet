using DNDGame.Core.Enums;
using DNDGame.Core.Entities;
using DNDGame.Core.Models;

namespace DNDGame.Core.Interfaces;

public interface ISessionService
{
    Task<Session?> GetSessionAsync(int id);
    Task<IEnumerable<object>> GetAllSessionsAsync();
    Task<object> CreateSessionAsync(object request);
    Task<Session?> UpdateSessionStateAsync(int id, SessionState state);
    Task<bool> DeleteSessionAsync(int id);
    
    // Real-time features
    Task<bool> JoinSessionAsync(int sessionId, int characterId);
    Task<bool> LeaveSessionAsync(int sessionId, int characterId);
    Task<Message> SaveMessageAsync(int sessionId, string content, MessageRole role);
    Task<DiceRoll> SaveDiceRollAsync(int sessionId, string formula, DiceRollResult result);
}
