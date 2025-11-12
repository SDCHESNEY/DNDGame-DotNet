using DNDGame.Core.Entities;
using DNDGame.Core.Enums;
using DNDGame.Core.Models;

namespace DNDGame.Core.Interfaces;

/// <summary>
/// Service for resolving D&D 5e rules and mechanics.
/// </summary>
public interface IRulesEngine
{
    /// <summary>
    /// Resolves an ability check against a difficulty class.
    /// </summary>
    /// <param name="abilityScore">The character's ability score.</param>
    /// <param name="dc">The difficulty class to beat.</param>
    /// <param name="proficient">Whether the character is proficient in the skill.</param>
    /// <param name="proficiencyBonus">The character's proficiency bonus.</param>
    /// <param name="advantageType">The type of advantage for the roll.</param>
    /// <returns>The result of the ability check.</returns>
    CheckResult ResolveAbilityCheck(
        int abilityScore,
        int dc,
        bool proficient = false,
        int proficiencyBonus = 0,
        AdvantageType advantageType = AdvantageType.Normal);

    /// <summary>
    /// Resolves a saving throw for a character.
    /// </summary>
    /// <param name="character">The character making the save.</param>
    /// <param name="ability">The ability to use for the save.</param>
    /// <param name="dc">The difficulty class to beat.</param>
    /// <param name="advantageType">The type of advantage for the roll.</param>
    /// <returns>The result of the saving throw.</returns>
    CheckResult ResolveSavingThrow(
        Character character,
        AbilityType ability,
        int dc,
        AdvantageType advantageType = AdvantageType.Normal);

    /// <summary>
    /// Resolves an attack roll against a target.
    /// </summary>
    /// <param name="attackBonus">The attacker's attack bonus.</param>
    /// <param name="targetAC">The target's armor class.</param>
    /// <param name="advantageType">The type of advantage for the attack.</param>
    /// <returns>The result of the attack roll.</returns>
    AttackResult ResolveAttack(
        int attackBonus,
        int targetAC,
        AdvantageType advantageType = AdvantageType.Normal);

    /// <summary>
    /// Calculates damage from a damage formula.
    /// </summary>
    /// <param name="damageFormula">The damage formula (e.g., "2d6+3").</param>
    /// <param name="critical">Whether this is a critical hit (doubles dice, not modifiers).</param>
    /// <returns>The total damage.</returns>
    int CalculateDamage(string damageFormula, bool critical = false);
}
