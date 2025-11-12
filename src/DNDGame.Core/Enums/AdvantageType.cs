namespace DNDGame.Core.Enums;

/// <summary>
/// Types of advantage for dice rolls in D&D 5e.
/// </summary>
public enum AdvantageType
{
    /// <summary>
    /// Normal roll with no advantage or disadvantage.
    /// </summary>
    Normal,
    
    /// <summary>
    /// Roll twice and take the higher result.
    /// </summary>
    Advantage,
    
    /// <summary>
    /// Roll twice and take the lower result.
    /// </summary>
    Disadvantage
}
