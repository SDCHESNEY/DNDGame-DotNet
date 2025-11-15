namespace DNDGame.Web.Models;

/// <summary>
/// Mutable form model for character creation to work with Blazor EditForm binding
/// </summary>
public class CharacterFormModel
{
    public int PlayerId { get; set; } = 1;
    public string Name { get; set; } = string.Empty;
    public string Class { get; set; } = "Fighter";
    public int Level { get; set; } = 1;
    
    // Individual ability scores for form binding
    public int Strength { get; set; } = 10;
    public int Dexterity { get; set; } = 10;
    public int Constitution { get; set; } = 10;
    public int Intelligence { get; set; } = 10;
    public int Wisdom { get; set; } = 10;
    public int Charisma { get; set; } = 10;
}
