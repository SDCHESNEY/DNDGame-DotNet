using System.Text.RegularExpressions;

namespace DNDGame.Core.ValueObjects;

/// <summary>
/// Represents a dice formula in D&D notation (e.g., "2d6+3").
/// </summary>
public record DiceFormula
{
    private static readonly Regex FormulaRegex = new(
        @"^(\d+)d(\d+)([+-]\d+)?$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Gets the number of dice to roll.
    /// </summary>
    public int Count { get; init; }

    /// <summary>
    /// Gets the number of sides on each die.
    /// </summary>
    public int Sides { get; init; }

    /// <summary>
    /// Gets the modifier to add to the total roll.
    /// </summary>
    public int Modifier { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DiceFormula"/> record.
    /// </summary>
    /// <param name="count">The number of dice to roll.</param>
    /// <param name="sides">The number of sides on each die.</param>
    /// <param name="modifier">The modifier to add to the total roll.</param>
    public DiceFormula(int count, int sides, int modifier = 0)
    {
        if (count < 1)
            throw new ArgumentException("Count must be at least 1.", nameof(count));
        
        if (sides < 2)
            throw new ArgumentException("Sides must be at least 2.", nameof(sides));

        Count = count;
        Sides = sides;
        Modifier = modifier;
    }

    /// <summary>
    /// Parses a dice formula string into a <see cref="DiceFormula"/> instance.
    /// </summary>
    /// <param name="formula">The formula string (e.g., "2d6+3", "1d20", "3d8-1").</param>
    /// <returns>A new <see cref="DiceFormula"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when the formula is invalid.</exception>
    public static DiceFormula Parse(string formula)
    {
        if (string.IsNullOrWhiteSpace(formula))
            throw new ArgumentException("Formula cannot be null or empty.", nameof(formula));

        var match = FormulaRegex.Match(formula.Trim());
        if (!match.Success)
            throw new ArgumentException($"Invalid dice formula: {formula}", nameof(formula));

        var count = int.Parse(match.Groups[1].Value);
        var sides = int.Parse(match.Groups[2].Value);
        var modifier = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : 0;

        return new DiceFormula(count, sides, modifier);
    }

    /// <summary>
    /// Tries to parse a dice formula string into a <see cref="DiceFormula"/> instance.
    /// </summary>
    /// <param name="formula">The formula string.</param>
    /// <param name="result">The parsed formula if successful.</param>
    /// <returns>True if parsing succeeded; otherwise, false.</returns>
    public static bool TryParse(string formula, out DiceFormula? result)
    {
        try
        {
            result = Parse(formula);
            return true;
        }
        catch
        {
            result = null;
            return false;
        }
    }

    /// <summary>
    /// Returns the string representation of this dice formula.
    /// </summary>
    public override string ToString()
    {
        var modifierStr = Modifier switch
        {
            > 0 => $"+{Modifier}",
            < 0 => Modifier.ToString(),
            _ => string.Empty
        };

        return $"{Count}d{Sides}{modifierStr}";
    }
}
