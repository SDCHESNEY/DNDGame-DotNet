using System.Security.Cryptography;
using DNDGame.Core.Enums;
using DNDGame.Core.Interfaces;
using DNDGame.Core.Models;
using DNDGame.Core.ValueObjects;

namespace DNDGame.Application.Services;

/// <summary>
/// Service for rolling dice using cryptographically secure random number generation.
/// </summary>
public class DiceRollerService : IDiceRoller
{
    /// <inheritdoc/>
    public DiceRollResult Roll(string formula)
    {
        return Roll(formula, AdvantageType.Normal);
    }

    /// <inheritdoc/>
    public DiceRollResult RollWithAdvantage(string formula)
    {
        return Roll(formula, AdvantageType.Advantage);
    }

    /// <inheritdoc/>
    public DiceRollResult RollWithDisadvantage(string formula)
    {
        return Roll(formula, AdvantageType.Disadvantage);
    }

    /// <inheritdoc/>
    public DiceRollResult Roll(string formula, AdvantageType advantageType)
    {
        var diceFormula = DiceFormula.Parse(formula);

        // For advantage/disadvantage, only apply to single d20 rolls
        if (advantageType != AdvantageType.Normal && diceFormula.Count == 1 && diceFormula.Sides == 20)
        {
            return RollWithAdvantageDisadvantage(diceFormula, advantageType);
        }

        var rolls = RollDice(diceFormula.Count, diceFormula.Sides);
        var sum = rolls.Sum();
        var total = sum + diceFormula.Modifier;

        // Check for critical/fumble on single d20
        var isCritical = diceFormula.Count == 1 && diceFormula.Sides == 20 && rolls[0] == 20;
        var isFumble = diceFormula.Count == 1 && diceFormula.Sides == 20 && rolls[0] == 1;

        return new DiceRollResult
        {
            Formula = formula,
            Total = total,
            IndividualRolls = rolls,
            Modifier = diceFormula.Modifier,
            IsCritical = isCritical,
            IsFumble = isFumble
        };
    }

    private DiceRollResult RollWithAdvantageDisadvantage(DiceFormula formula, AdvantageType advantageType)
    {
        // Roll twice
        var roll1 = RollDice(1, 20)[0];
        var roll2 = RollDice(1, 20)[0];

        // Select the appropriate roll based on advantage type
        var selectedRoll = advantageType == AdvantageType.Advantage
            ? Math.Max(roll1, roll2)
            : Math.Min(roll1, roll2);

        var total = selectedRoll + formula.Modifier;

        // Keep both rolls to show advantage/disadvantage was applied
        return new DiceRollResult
        {
            Formula = formula.ToString(),
            Total = total,
            IndividualRolls = [roll1, roll2],
            Modifier = formula.Modifier,
            IsCritical = selectedRoll == 20,
            IsFumble = selectedRoll == 1
        };
    }

    private static int[] RollDice(int count, int sides)
    {
        var rolls = new int[count];
        
        for (int i = 0; i < count; i++)
        {
            rolls[i] = RandomNumberGenerator.GetInt32(1, sides + 1);
        }

        return rolls;
    }
}
