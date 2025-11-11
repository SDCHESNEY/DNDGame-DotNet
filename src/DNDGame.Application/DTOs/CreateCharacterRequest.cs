using DNDGame.Core.Enums;
using DNDGame.Core.ValueObjects;

namespace DNDGame.Application.DTOs;

public record CreateCharacterRequest(
    int PlayerId,
    string Name,
    CharacterClass Class,
    int Level,
    AbilityScores AbilityScores,
    int MaxHitPoints,
    int ArmorClass,
    List<string>? Skills = null,
    List<string>? Inventory = null,
    string? PersonalityTraits = null
);
