using DNDGame.Core.Enums;

namespace DNDGame.Core.Interfaces;

public interface ISessionService
{
    Task<object?> GetSessionAsync(int id);
    Task<IEnumerable<object>> GetAllSessionsAsync();
    Task<object> CreateSessionAsync(object request);
    Task<object?> UpdateSessionStateAsync(int id, SessionState state);
    Task<bool> DeleteSessionAsync(int id);
}
