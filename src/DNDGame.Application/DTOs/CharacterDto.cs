using DNDGame.Core.Enums;
using DNDGame.Core.ValueObjects;

namespace DNDGame.Application.DTOs;

public record CharacterDto(
    int Id,
    int PlayerId,
    string Name,
    CharacterClass Class,
    int Level,
    AbilityScores AbilityScores,
    int HitPoints,
    int MaxHitPoints,
    int ArmorClass,
    int ProficiencyBonus,
    List<string> Skills,
    List<string> Inventory,
    string? PersonalityTraits,
    DateTime CreatedAt
);
