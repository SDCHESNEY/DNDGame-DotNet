using Microsoft.EntityFrameworkCore;
using DNDGame.Core.Entities;
using DNDGame.Core.Interfaces;
using DNDGame.Infrastructure.Data;

namespace DNDGame.Infrastructure.Repositories;

public class CharacterRepository : ICharacterRepository
{
    private readonly DndGameContext _context;

    public CharacterRepository(DndGameContext context)
    {
        _context = context;
    }

    public async Task<Character?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Characters
            .Include(c => c.Player)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<List<Character>> GetAllByPlayerIdAsync(int playerId, CancellationToken cancellationToken = default)
    {
        return await _context.Characters
            .Where(c => c.PlayerId == playerId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Character> AddAsync(Character character, CancellationToken cancellationToken = default)
    {
        _context.Characters.Add(character);
        await _context.SaveChangesAsync(cancellationToken);
        return character;
    }

    public async Task UpdateAsync(Character character, CancellationToken cancellationToken = default)
    {
        _context.Characters.Update(character);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var character = await _context.Characters.FindAsync(new object[] { id }, cancellationToken);
        if (character != null)
        {
            _context.Characters.Remove(character);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
