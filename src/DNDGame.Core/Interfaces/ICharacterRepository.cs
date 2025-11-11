using DNDGame.Core.Entities;

namespace DNDGame.Core.Interfaces;

public interface ICharacterRepository
{
    Task<Character?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<Character>> GetAllByPlayerIdAsync(int playerId, CancellationToken cancellationToken = default);
    Task<Character> AddAsync(Character character, CancellationToken cancellationToken = default);
    Task UpdateAsync(Character character, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
