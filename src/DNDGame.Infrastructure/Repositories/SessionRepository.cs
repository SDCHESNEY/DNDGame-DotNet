using Microsoft.EntityFrameworkCore;
using DNDGame.Core.Entities;
using DNDGame.Core.Interfaces;
using DNDGame.Infrastructure.Data;

namespace DNDGame.Infrastructure.Repositories;

public class SessionRepository : ISessionRepository
{
    private readonly DndGameContext _context;

    public SessionRepository(DndGameContext context)
    {
        _context = context;
    }

    public async Task<Session?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Sessions
            .Include(s => s.Messages)
            .Include(s => s.DiceRolls)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<List<Session>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Sessions
            .OrderByDescending(s => s.LastActivityAt ?? s.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Session> AddAsync(Session session, CancellationToken cancellationToken = default)
    {
        _context.Sessions.Add(session);
        await _context.SaveChangesAsync(cancellationToken);
        return session;
    }

    public async Task UpdateAsync(Session session, CancellationToken cancellationToken = default)
    {
        _context.Sessions.Update(session);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var session = await _context.Sessions.FindAsync(new object[] { id }, cancellationToken);
        if (session != null)
        {
            _context.Sessions.Remove(session);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
