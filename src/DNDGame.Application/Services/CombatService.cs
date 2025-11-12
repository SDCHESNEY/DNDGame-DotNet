using DNDGame.Core.Enums;
using DNDGame.Core.Interfaces;
using DNDGame.Core.Models;
using Microsoft.Extensions.Logging;

namespace DNDGame.Application.Services;

/// <summary>
/// Service for managing combat mechanics and actions.
/// </summary>
public class CombatService : ICombatService
{
    private readonly ICharacterRepository _characterRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly IDiceRoller _diceRoller;
    private readonly IRulesEngine _rulesEngine;
    private readonly ILogger<CombatService> _logger;

    public CombatService(
        ICharacterRepository characterRepository,
        ISessionRepository sessionRepository,
        IDiceRoller diceRoller,
        IRulesEngine rulesEngine,
        ILogger<CombatService> logger)
    {
        _characterRepository = characterRepository;
        _sessionRepository = sessionRepository;
        _diceRoller = diceRoller;
        _rulesEngine = rulesEngine;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<List<InitiativeEntry>> RollInitiativeAsync(int sessionId)
    {
        _logger.LogInformation("Rolling initiative for session {SessionId}", sessionId);

        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session == null)
        {
            throw new ArgumentException($"Session {sessionId} not found", nameof(sessionId));
        }

        // Get all characters in the session
        var characterIds = session.Participants.Select(p => p.CharacterId).ToList();
        var initiativeEntries = new List<InitiativeEntry>();

        foreach (var characterId in characterIds)
        {
            var character = await _characterRepository.GetByIdAsync(characterId);
            if (character == null) continue;

            // Roll initiative: 1d20 + Dexterity modifier
            var dexModifier = character.AbilityScores.DexterityModifier;
            var roll = _diceRoller.Roll("1d20");
            var initiativeRoll = roll.IndividualRolls[0] + dexModifier;

            initiativeEntries.Add(new InitiativeEntry
            {
                CharacterId = character.Id,
                CharacterName = character.Name,
                InitiativeRoll = initiativeRoll,
                CurrentHP = character.HitPoints,
                MaxHP = character.MaxHitPoints,
                Conditions = [] // TODO: Load from conditions table
            });
        }

        // Sort by initiative (highest first)
        var sortedInitiative = initiativeEntries
            .OrderByDescending(e => e.InitiativeRoll)
            .ToList();

        _logger.LogInformation("Initiative rolled for {Count} characters", sortedInitiative.Count);
        return sortedInitiative;
    }

    /// <inheritdoc/>
    public async Task<AttackResult> ResolveAttackAsync(int attackerId, int defenderId, string damageFormula)
    {
        _logger.LogInformation(
            "Resolving attack from {AttackerId} to {DefenderId}",
            attackerId, defenderId);

        var attacker = await _characterRepository.GetByIdAsync(attackerId);
        var defender = await _characterRepository.GetByIdAsync(defenderId);

        if (attacker == null)
            throw new ArgumentException($"Attacker {attackerId} not found", nameof(attackerId));

        if (defender == null)
            throw new ArgumentException($"Defender {defenderId} not found", nameof(defenderId));

        // Calculate attack bonus (Strength or Dexterity + Proficiency)
        var attackBonus = Math.Max(
            attacker.AbilityScores.StrengthModifier,
            attacker.AbilityScores.DexterityModifier) + attacker.ProficiencyBonus;

        // Resolve the attack roll
        var attackResult = _rulesEngine.ResolveAttack(attackBonus, defender.ArmorClass);

        // If hit, calculate damage
        if (attackResult.Hit)
        {
            var damage = _rulesEngine.CalculateDamage(damageFormula, attackResult.IsCritical);
            attackResult = attackResult with { Damage = damage };

            _logger.LogInformation(
                "Attack hit! Damage: {Damage} (Critical: {IsCritical})",
                damage, attackResult.IsCritical);
        }
        else
        {
            _logger.LogInformation("Attack missed");
        }

        return attackResult;
    }

    /// <inheritdoc/>
    public async Task<bool> ApplyDamageAsync(int characterId, int damage)
    {
        _logger.LogInformation("Applying {Damage} damage to character {CharacterId}", damage, characterId);

        var character = await _characterRepository.GetByIdAsync(characterId);
        if (character == null)
            throw new ArgumentException($"Character {characterId} not found", nameof(characterId));

        // Reduce hit points, but not below 0
        character.HitPoints = Math.Max(0, character.HitPoints - damage);
        
        await _characterRepository.UpdateAsync(character);

        var isConscious = character.HitPoints > 0;
        _logger.LogInformation(
            "Character {CharacterId} now has {HitPoints}/{MaxHitPoints} HP (Conscious: {IsConscious})",
            characterId, character.HitPoints, character.MaxHitPoints, isConscious);

        return isConscious;
    }

    /// <inheritdoc/>
    public async Task<int> ApplyHealingAsync(int characterId, int healing)
    {
        _logger.LogInformation("Applying {Healing} healing to character {CharacterId}", healing, characterId);

        var character = await _characterRepository.GetByIdAsync(characterId);
        if (character == null)
            throw new ArgumentException($"Character {characterId} not found", nameof(characterId));

        // Increase hit points, but not above maximum
        character.HitPoints = Math.Min(character.MaxHitPoints, character.HitPoints + healing);
        
        await _characterRepository.UpdateAsync(character);

        _logger.LogInformation(
            "Character {CharacterId} now has {HitPoints}/{MaxHitPoints} HP",
            characterId, character.HitPoints, character.MaxHitPoints);

        return character.HitPoints;
    }
}
