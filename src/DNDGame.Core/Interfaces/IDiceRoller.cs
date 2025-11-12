using DNDGame.Core.Enums;
using DNDGame.Core.Models;

namespace DNDGame.Core.Interfaces;

/// <summary>
/// Service for rolling dice with cryptographic randomness.
/// </summary>
public interface IDiceRoller
{
    /// <summary>
    /// Rolls dice according to the specified formula.
    /// </summary>
    /// <param name="formula">The dice formula (e.g., "2d6+3", "1d20").</param>
    /// <returns>The result of the dice roll.</returns>
    DiceRollResult Roll(string formula);

    /// <summary>
    /// Rolls dice with advantage (roll twice, take higher).
    /// </summary>
    /// <param name="formula">The dice formula.</param>
    /// <returns>The result of the dice roll.</returns>
    DiceRollResult RollWithAdvantage(string formula);

    /// <summary>
    /// Rolls dice with disadvantage (roll twice, take lower).
    /// </summary>
    /// <param name="formula">The dice formula.</param>
    /// <returns>The result of the dice roll.</returns>
    DiceRollResult RollWithDisadvantage(string formula);

    /// <summary>
    /// Rolls dice with the specified advantage type.
    /// </summary>
    /// <param name="formula">The dice formula.</param>
    /// <param name="advantageType">The type of advantage.</param>
    /// <returns>The result of the dice roll.</returns>
    DiceRollResult Roll(string formula, AdvantageType advantageType);
}
