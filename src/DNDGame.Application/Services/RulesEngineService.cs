using DNDGame.Core.Entities;
using DNDGame.Core.Enums;
using DNDGame.Core.Interfaces;
using DNDGame.Core.Models;

namespace DNDGame.Application.Services;

/// <summary>
/// Service for resolving D&D 5e game rules and mechanics.
/// </summary>
public class RulesEngineService : IRulesEngine
{
    private readonly IDiceRoller _diceRoller;

    public RulesEngineService(IDiceRoller diceRoller)
    {
        _diceRoller = diceRoller;
    }

    /// <inheritdoc/>
    public CheckResult ResolveAbilityCheck(
        int abilityScore,
        int dc,
        bool proficient = false,
        int proficiencyBonus = 0,
        AdvantageType advantageType = AdvantageType.Normal)
    {
        var abilityModifier = CalculateAbilityModifier(abilityScore);
        var bonus = proficient ? proficiencyBonus : 0;

        var rollResult = _diceRoller.Roll("1d20", advantageType);
        var roll = rollResult.IndividualRolls[0];
        var total = roll + abilityModifier + bonus;

        return new CheckResult
        {
            Total = total,
            Roll = roll,
            AbilityModifier = abilityModifier,
            ProficiencyBonus = bonus,
            DifficultyClass = dc,
            Success = total >= dc,
            IsCritical = roll == 20,
            IsFumble = roll == 1
        };
    }

    /// <inheritdoc/>
    public CheckResult ResolveSavingThrow(
        Character character,
        AbilityType ability,
        int dc,
        AdvantageType advantageType = AdvantageType.Normal)
    {
        var abilityScore = GetAbilityScore(character, ability);
        var proficiencyBonus = character.ProficiencyBonus;

        // In a full implementation, we'd check if the character is proficient in this saving throw
        // For now, we'll assume no proficiency in saves
        return ResolveAbilityCheck(abilityScore, dc, false, proficiencyBonus, advantageType);
    }

    /// <inheritdoc/>
    public AttackResult ResolveAttack(
        int attackBonus,
        int targetAC,
        AdvantageType advantageType = AdvantageType.Normal)
    {
        var rollResult = _diceRoller.Roll("1d20", advantageType);
        var roll = rollResult.IndividualRolls[0];
        var attackRoll = roll + attackBonus;

        // Natural 20 always hits, Natural 1 always misses
        var hit = roll == 20 || (roll != 1 && attackRoll >= targetAC);

        return new AttackResult
        {
            AttackRoll = attackRoll,
            TargetAC = targetAC,
            Hit = hit,
            IsCritical = roll == 20,
            IsFumble = roll == 1
        };
    }

    /// <inheritdoc/>
    public int CalculateDamage(string damageFormula, bool critical = false)
    {
        var rollResult = _diceRoller.Roll(damageFormula);
        
        if (!critical)
        {
            return rollResult.Total;
        }

        // Critical hit: double the dice, not the modifier
        var diceFormula = Core.ValueObjects.DiceFormula.Parse(damageFormula);
        var criticalFormula = new Core.ValueObjects.DiceFormula(
            diceFormula.Count * 2,
            diceFormula.Sides,
            diceFormula.Modifier);

        var criticalResult = _diceRoller.Roll(criticalFormula.ToString());
        return criticalResult.Total;
    }

    public static int CalculateAbilityModifier(int abilityScore)
    {
        return (abilityScore - 10) / 2;
    }

    private static int GetAbilityScore(Character character, AbilityType ability)
    {
        return ability switch
        {
            AbilityType.Strength => character.AbilityScores.Strength,
            AbilityType.Dexterity => character.AbilityScores.Dexterity,
            AbilityType.Constitution => character.AbilityScores.Constitution,
            AbilityType.Intelligence => character.AbilityScores.Intelligence,
            AbilityType.Wisdom => character.AbilityScores.Wisdom,
            AbilityType.Charisma => character.AbilityScores.Charisma,
            _ => throw new ArgumentException($"Invalid ability type: {ability}", nameof(ability))
        };
    }
}
