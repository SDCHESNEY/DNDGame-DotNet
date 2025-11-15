using DNDGame.Core.Models;

namespace DNDGame.Core.Interfaces;

/// <summary>
/// Service for managing combat mechanics.
/// </summary>
public interface ICombatService
{
    /// <summary>
    /// Rolls initiative for all characters in a session.
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <returns>The initiative order.</returns>
    InitiativeOrder RollInitiative(int sessionId);
    
    /// <summary>
    /// Rolls initiative for all characters in a session (async version).
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <returns>The initiative order.</returns>
    Task<List<InitiativeEntry>> RollInitiativeAsync(int sessionId);

    /// <summary>
    /// Resolves an attack between two characters.
    /// </summary>
    /// <param name="attackerId">The attacking character's ID.</param>
    /// <param name="defenderId">The defending character's ID.</param>
    /// <param name="damageFormula">The damage formula (e.g., "1d8+3").</param>
    /// <returns>The result of the attack.</returns>
    Task<AttackResult> ResolveAttackAsync(int attackerId, int defenderId, string damageFormula);

    /// <summary>
    /// Applies damage to a character.
    /// </summary>
    /// <param name="characterId">The character's ID.</param>
    /// <param name="damage">The amount of damage to apply.</param>
    /// <returns>True if the character is still conscious; otherwise, false.</returns>
    Task<bool> ApplyDamageAsync(int characterId, int damage);

    /// <summary>
    /// Applies healing to a character.
    /// </summary>
    /// <param name="characterId">The character's ID.</param>
    /// <param name="healing">The amount of healing to apply.</param>
    /// <returns>The character's new hit points.</returns>
    Task<int> ApplyHealingAsync(int characterId, int healing);
}
