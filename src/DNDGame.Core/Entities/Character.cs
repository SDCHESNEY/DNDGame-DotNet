using DNDGame.Core.Enums;
using DNDGame.Core.ValueObjects;

namespace DNDGame.Core.Entities;

public class Character
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public required string Name { get; set; }
    public CharacterClass Class { get; set; }
    public int Level { get; set; } = 1;
    public required AbilityScores AbilityScores { get; set; }
    public int HitPoints { get; set; }
    public int MaxHitPoints { get; set; }
    public int ArmorClass { get; set; }
    public int ProficiencyBonus => 2 + (Level - 1) / 4; // D&D 5e formula
    public List<string> Skills { get; set; } = [];
    public List<string> Inventory { get; set; } = [];
    public string? PersonalityTraits { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Player Player { get; set; } = null!;
}
