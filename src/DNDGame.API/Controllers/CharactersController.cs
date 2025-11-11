using DNDGame.Application.DTOs;
using DNDGame.Application.Validators;
using DNDGame.Core.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace DNDGame.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CharactersController : ControllerBase
{
    private readonly ICharacterService _characterService;
    private readonly IValidator<CreateCharacterRequest> _createValidator;
    private readonly ILogger<CharactersController> _logger;

    public CharactersController(
        ICharacterService characterService,
        IValidator<CreateCharacterRequest> createValidator,
        ILogger<CharactersController> logger)
    {
        _characterService = characterService;
        _createValidator = createValidator;
        _logger = logger;
    }

    /// <summary>
    /// Get a character by ID
    /// </summary>
    /// <param name="id">The character ID</param>
    /// <returns>The character details</returns>
    /// <response code="200">Returns the character</response>
    /// <response code="404">Character not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CharacterDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CharacterDto>> GetCharacter(int id)
    {
        _logger.LogInformation("Fetching character with ID {CharacterId}", id);

        var character = await _characterService.GetCharacterAsync(id);
        
        if (character == null)
        {
            _logger.LogWarning("Character with ID {CharacterId} not found", id);
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Character not found",
                Detail = $"Character with ID {id} does not exist"
            });
        }

        return Ok(character);
    }

    /// <summary>
    /// Get all characters for a player
    /// </summary>
    /// <param name="playerId">The player ID</param>
    /// <returns>List of characters</returns>
    /// <response code="200">Returns the list of characters</response>
    [HttpGet("player/{playerId}")]
    [ProducesResponseType(typeof(IEnumerable<CharacterDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CharacterDto>>> GetPlayerCharacters(int playerId)
    {
        _logger.LogInformation("Fetching characters for player ID {PlayerId}", playerId);

        var characters = await _characterService.GetAllCharactersByPlayerAsync(playerId);
        return Ok(characters);
    }

    /// <summary>
    /// Create a new character
    /// </summary>
    /// <param name="playerId">The player ID who owns the character</param>
    /// <param name="request">Character creation details</param>
    /// <returns>The created character</returns>
    /// <response code="201">Character created successfully</response>
    /// <response code="400">Invalid request data</response>
    [HttpPost("player/{playerId}")]
    [ProducesResponseType(typeof(CharacterDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CharacterDto>> CreateCharacter(
        int playerId,
        [FromBody] CreateCharacterRequest request)
    {
        _logger.LogInformation("Creating character for player ID {PlayerId}", playerId);

        var validationResult = await _createValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Character creation validation failed for player ID {PlayerId}", playerId);
            
            return BadRequest(new ValidationProblemDetails(
                validationResult.Errors.ToDictionary(
                    e => e.PropertyName,
                    e => new[] { e.ErrorMessage }))
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed",
                Detail = "One or more validation errors occurred"
            });
        }

        try
        {
            var result = await _characterService.CreateCharacterAsync(playerId, request);
            var character = (CharacterDto)result;
            _logger.LogInformation("Character created successfully with ID {CharacterId}", character.Id);

            return CreatedAtAction(
                nameof(GetCharacter),
                new { id = character.Id },
                character);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating character for player ID {PlayerId}", playerId);
            
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An error occurred",
                Detail = "Failed to create character"
            });
        }
    }

    /// <summary>
    /// Update an existing character
    /// </summary>
    /// <param name="id">The character ID</param>
    /// <param name="request">Updated character details</param>
    /// <returns>The updated character</returns>
    /// <response code="200">Character updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Character not found</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CharacterDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CharacterDto>> UpdateCharacter(
        int id,
        [FromBody] CreateCharacterRequest request)
    {
        _logger.LogInformation("Updating character with ID {CharacterId}", id);

        var validationResult = await _createValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Character update validation failed for ID {CharacterId}", id);
            
            return BadRequest(new ValidationProblemDetails(
                validationResult.Errors.ToDictionary(
                    e => e.PropertyName,
                    e => new[] { e.ErrorMessage }))
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed",
                Detail = "One or more validation errors occurred"
            });
        }

        try
        {
            var character = await _characterService.UpdateCharacterAsync(id, request);
            
            if (character == null)
            {
                _logger.LogWarning("Character with ID {CharacterId} not found for update", id);
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Character not found",
                    Detail = $"Character with ID {id} does not exist"
                });
            }

            _logger.LogInformation("Character with ID {CharacterId} updated successfully", id);
            return Ok(character);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating character with ID {CharacterId}", id);
            
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An error occurred",
                Detail = "Failed to update character"
            });
        }
    }

    /// <summary>
    /// Delete a character
    /// </summary>
    /// <param name="id">The character ID</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Character deleted successfully</response>
    /// <response code="404">Character not found</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCharacter(int id)
    {
        _logger.LogInformation("Deleting character with ID {CharacterId}", id);

        try
        {
            var deleted = await _characterService.DeleteCharacterAsync(id);
            
            if (!deleted)
            {
                _logger.LogWarning("Character with ID {CharacterId} not found for deletion", id);
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Character not found",
                    Detail = $"Character with ID {id} does not exist"
                });
            }

            _logger.LogInformation("Character with ID {CharacterId} deleted successfully", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting character with ID {CharacterId}", id);
            
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An error occurred",
                Detail = "Failed to delete character"
            });
        }
    }
}
