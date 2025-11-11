namespace DNDGame.Core.ValueObjects;

public record AbilityScores(
    int Strength,
    int Dexterity,
    int Constitution,
    int Intelligence,
    int Wisdom,
    int Charisma)
{
    public int GetModifier(int score) => (score - 10) / 2;

    public int StrengthModifier => GetModifier(Strength);
    public int DexterityModifier => GetModifier(Dexterity);
    public int ConstitutionModifier => GetModifier(Constitution);
    public int IntelligenceModifier => GetModifier(Intelligence);
    public int WisdomModifier => GetModifier(Wisdom);
    public int CharismaModifier => GetModifier(Charisma);
}
