using DNDGame.Core.Enums;
using DNDGame.Core.Interfaces;
using DNDGame.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace DNDGame.API.Controllers;

/// <summary>
/// Controller for dice rolling operations.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class DiceController : ControllerBase
{
    private readonly IDiceRoller _diceRoller;
    private readonly ILogger<DiceController> _logger;

    public DiceController(IDiceRoller diceRoller, ILogger<DiceController> logger)
    {
        _diceRoller = diceRoller;
        _logger = logger;
    }

    /// <summary>
    /// Rolls dice according to the specified formula.
    /// </summary>
    /// <param name="formula">The dice formula (e.g., "2d6+3", "1d20").</param>
    /// <param name="advantage">Optional advantage type (Normal, Advantage, Disadvantage).</param>
    /// <returns>The result of the dice roll.</returns>
    /// <response code="200">Returns the dice roll result.</response>
    /// <response code="400">If the formula is invalid.</response>
    [HttpPost("roll")]
    [ProducesResponseType(typeof(DiceRollResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<DiceRollResult> RollDice(
        [FromQuery] string formula,
        [FromQuery] AdvantageType advantage = AdvantageType.Normal)
    {
        try
        {
            _logger.LogInformation("Rolling dice: {Formula} with {Advantage}", formula, advantage);
            
            var result = _diceRoller.Roll(formula, advantage);
            
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid dice formula: {Formula}", formula);
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid dice formula",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    /// <summary>
    /// Rolls dice with advantage (roll twice, take higher).
    /// </summary>
    /// <param name="formula">The dice formula.</param>
    /// <returns>The result of the dice roll.</returns>
    /// <response code="200">Returns the dice roll result.</response>
    /// <response code="400">If the formula is invalid.</response>
    [HttpPost("roll/advantage")]
    [ProducesResponseType(typeof(DiceRollResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<DiceRollResult> RollWithAdvantage([FromQuery] string formula)
    {
        return RollDice(formula, AdvantageType.Advantage);
    }

    /// <summary>
    /// Rolls dice with disadvantage (roll twice, take lower).
    /// </summary>
    /// <param name="formula">The dice formula.</param>
    /// <returns>The result of the dice roll.</returns>
    /// <response code="200">Returns the dice roll result.</response>
    /// <response code="400">If the formula is invalid.</response>
    [HttpPost("roll/disadvantage")]
    [ProducesResponseType(typeof(DiceRollResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<DiceRollResult> RollWithDisadvantage([FromQuery] string formula)
    {
        return RollDice(formula, AdvantageType.Disadvantage);
    }
}
