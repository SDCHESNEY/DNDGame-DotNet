using DNDGame.Core.Enums;
using DNDGame.Core.Interfaces;
using DNDGame.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace DNDGame.API.Controllers;

/// <summary>
/// Controller for combat operations.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class CombatController : ControllerBase
{
    private readonly ICombatService _combatService;
    private readonly ILogger<CombatController> _logger;

    public CombatController(ICombatService combatService, ILogger<CombatController> logger)
    {
        _combatService = combatService;
        _logger = logger;
    }

    /// <summary>
    /// Rolls initiative for all characters in a session.
    /// </summary>
    /// <param name="sessionId">The ID of the session.</param>
    /// <returns>The initiative order with all participants.</returns>
    /// <response code="200">Returns the initiative order.</response>
    /// <response code="404">If the session is not found.</response>
    [HttpPost("initiative/{sessionId:int}")]
    [ProducesResponseType(typeof(List<InitiativeEntry>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<InitiativeEntry>>> RollInitiative(int sessionId)
    {
        try
        {
            _logger.LogInformation("Rolling initiative for session {SessionId}", sessionId);
            
            var initiative = await _combatService.RollInitiativeAsync(sessionId);
            
            return Ok(initiative);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to roll initiative for session {SessionId}", sessionId);
            return NotFound(new ProblemDetails
            {
                Title = "Session not found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
    }

    /// <summary>
    /// Resolves an attack from one character against another.
    /// </summary>
    /// <param name="request">The attack request details.</param>
    /// <returns>The result of the attack.</returns>
    /// <response code="200">Returns the attack result.</response>
    /// <response code="404">If the attacker or target is not found.</response>
    [HttpPost("attack")]
    [ProducesResponseType(typeof(AttackResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AttackResult>> ResolveAttack([FromBody] AttackRequest request)
    {
        try
        {
            _logger.LogInformation(
                "Resolving attack: {AttackerId} -> {TargetId}",
                request.AttackerId,
                request.TargetId);
            
            var result = await _combatService.ResolveAttackAsync(
                request.AttackerId,
                request.TargetId,
                request.DamageDice);
            
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to resolve attack");
            return NotFound(new ProblemDetails
            {
                Title = "Character not found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
    }

    /// <summary>
    /// Applies damage to a character.
    /// </summary>
    /// <param name="characterId">The ID of the character.</param>
    /// <param name="request">The damage details.</param>
    /// <returns>Whether the character is still conscious.</returns>
    /// <response code="200">Returns the consciousness status.</response>
    /// <response code="404">If the character is not found.</response>
    [HttpPost("{characterId:int}/damage")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> ApplyDamage(int characterId, [FromBody] DamageRequest request)
    {
        try
        {
            _logger.LogInformation("Applying {Damage} damage to character {CharacterId}", request.Damage, characterId);
            
            var isConscious = await _combatService.ApplyDamageAsync(characterId, request.Damage);
            
            return Ok(isConscious);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to apply damage to character {CharacterId}", characterId);
            return NotFound(new ProblemDetails
            {
                Title = "Character not found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
    }

    /// <summary>
    /// Applies healing to a character.
    /// </summary>
    /// <param name="characterId">The ID of the character.</param>
    /// <param name="request">The healing details.</param>
    /// <returns>The character's new hit points.</returns>
    /// <response code="200">Returns the new hit points.</response>
    /// <response code="404">If the character is not found.</response>
    [HttpPost("{characterId:int}/healing")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<int>> ApplyHealing(int characterId, [FromBody] HealingRequest request)
    {
        try
        {
            _logger.LogInformation("Applying {Healing} healing to character {CharacterId}", request.Healing, characterId);
            
            var newHp = await _combatService.ApplyHealingAsync(characterId, request.Healing);
            
            return Ok(newHp);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to apply healing to character {CharacterId}", characterId);
            return NotFound(new ProblemDetails
            {
                Title = "Character not found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
    }
}

/// <summary>
/// Request model for attack actions.
/// </summary>
public record AttackRequest(
    int AttackerId,
    int TargetId,
    string DamageDice);

/// <summary>
/// Request model for damage application.
/// </summary>
public record DamageRequest(int Damage);

/// <summary>
/// Request model for healing application.
/// </summary>
public record HealingRequest(int Healing);
