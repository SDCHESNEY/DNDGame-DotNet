namespace DNDGame.Core.Enums;

/// <summary>
/// D&D 5e condition types that can affect characters.
/// </summary>
public enum ConditionType
{
    /// <summary>
    /// A blinded creature can't see and automatically fails any ability check that requires sight.
    /// </summary>
    Blinded,
    
    /// <summary>
    /// A charmed creature can't attack the charmer or target the charmer with harmful abilities.
    /// </summary>
    Charmed,
    
    /// <summary>
    /// A deafened creature can't hear and automatically fails any ability check that requires hearing.
    /// </summary>
    Deafened,
    
    /// <summary>
    /// A frightened creature has disadvantage on ability checks and attack rolls while the source is in sight.
    /// </summary>
    Frightened,
    
    /// <summary>
    /// A grappled creature's speed becomes 0 and can't benefit from any bonus to its speed.
    /// </summary>
    Grappled,
    
    /// <summary>
    /// An incapacitated creature can't take actions or reactions.
    /// </summary>
    Incapacitated,
    
    /// <summary>
    /// An invisible creature is impossible to see without special sense and is heavily obscured.
    /// </summary>
    Invisible,
    
    /// <summary>
    /// A paralyzed creature is incapacitated and can't move or speak.
    /// </summary>
    Paralyzed,
    
    /// <summary>
    /// A petrified creature is transformed along with objects into a solid inanimate substance.
    /// </summary>
    Petrified,
    
    /// <summary>
    /// A poisoned creature has disadvantage on attack rolls and ability checks.
    /// </summary>
    Poisoned,
    
    /// <summary>
    /// A prone creature's only movement option is to crawl unless it stands up.
    /// </summary>
    Prone,
    
    /// <summary>
    /// A restrained creature's speed becomes 0 and has disadvantage on Dexterity saving throws.
    /// </summary>
    Restrained,
    
    /// <summary>
    /// A stunned creature is incapacitated, can't move, and can speak only falteringly.
    /// </summary>
    Stunned,
    
    /// <summary>
    /// An unconscious creature is incapacitated, can't move or speak, and is unaware of its surroundings.
    /// </summary>
    Unconscious,
    
    /// <summary>
    /// Exhaustion is measured in six levels with cumulative effects.
    /// </summary>
    Exhausted
}
