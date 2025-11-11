using DNDGame.Core.Entities;

namespace DNDGame.Core.Interfaces;

public interface ISessionRepository
{
    Task<Session?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<Session>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Session> AddAsync(Session session, CancellationToken cancellationToken = default);
    Task UpdateAsync(Session session, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
