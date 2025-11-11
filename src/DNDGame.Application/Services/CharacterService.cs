using DNDGame.Application.DTOs;
using DNDGame.Core.Entities;
using DNDGame.Core.Interfaces;

namespace DNDGame.Application.Services;

public class CharacterService : ICharacterService
{
    private readonly ICharacterRepository _characterRepository;

    public CharacterService(ICharacterRepository characterRepository)
    {
        _characterRepository = characterRepository;
    }

    public async Task<object?> GetCharacterAsync(int id)
    {
        var character = await _characterRepository.GetByIdAsync(id);
        return character == null ? null : MapToDto(character);
    }

    public async Task<IEnumerable<object>> GetAllCharactersByPlayerAsync(int playerId)
    {
        var characters = await _characterRepository.GetAllByPlayerIdAsync(playerId);
        return characters.Select(c => (object)MapToDto(c));
    }

    public async Task<object> CreateCharacterAsync(int playerId, object request)
    {
        var dto = (CreateCharacterRequest)request;
        var character = new Character
        {
            PlayerId = playerId,
            Name = dto.Name,
            Class = dto.Class,
            Level = dto.Level,
            AbilityScores = dto.AbilityScores,
            MaxHitPoints = dto.MaxHitPoints,
            HitPoints = dto.MaxHitPoints, // Start at full health
            ArmorClass = dto.ArmorClass,
            Skills = dto.Skills ?? new List<string>(),
            Inventory = dto.Inventory ?? new List<string>(),
            PersonalityTraits = dto.PersonalityTraits,
            CreatedAt = DateTime.UtcNow
        };

        await _characterRepository.AddAsync(character);
        return MapToDto(character);
    }

    public async Task<object?> UpdateCharacterAsync(int id, object request)
    {
        var dto = (CreateCharacterRequest)request;
        var character = await _characterRepository.GetByIdAsync(id);
        if (character == null)
            return null;

        character.Name = dto.Name;
        character.Class = dto.Class;
        character.Level = dto.Level;
        character.AbilityScores = dto.AbilityScores;
        character.MaxHitPoints = dto.MaxHitPoints;
        character.ArmorClass = dto.ArmorClass;
        character.Skills = dto.Skills ?? character.Skills;
        character.Inventory = dto.Inventory ?? character.Inventory;
        character.PersonalityTraits = dto.PersonalityTraits;

        await _characterRepository.UpdateAsync(character);
        return MapToDto(character);
    }

    public async Task<bool> DeleteCharacterAsync(int id)
    {
        var character = await _characterRepository.GetByIdAsync(id);
        if (character == null)
            return false;

        await _characterRepository.DeleteAsync(id);
        return true;
    }

    private static CharacterDto MapToDto(Character character)
    {
        return new CharacterDto(
            character.Id,
            character.PlayerId,
            character.Name,
            character.Class,
            character.Level,
            character.AbilityScores,
            character.HitPoints,
            character.MaxHitPoints,
            character.ArmorClass,
            character.ProficiencyBonus,
            character.Skills,
            character.Inventory,
            character.PersonalityTraits,
            character.CreatedAt
        );
    }
}
