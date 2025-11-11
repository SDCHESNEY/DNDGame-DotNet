namespace DNDGame.Core.Interfaces;

public interface ICharacterService
{
    Task<object?> GetCharacterAsync(int id);
    Task<IEnumerable<object>> GetAllCharactersByPlayerAsync(int playerId);
    Task<object> CreateCharacterAsync(int playerId, object request);
    Task<object?> UpdateCharacterAsync(int id, object request);
    Task<bool> DeleteCharacterAsync(int id);
}
