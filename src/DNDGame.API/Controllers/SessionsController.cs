using DNDGame.Application.DTOs;
using DNDGame.Application.Validators;
using DNDGame.Core.Enums;
using DNDGame.Core.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace DNDGame.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class SessionsController : ControllerBase
{
    private readonly ISessionService _sessionService;
    private readonly IValidator<CreateSessionRequest> _createValidator;
    private readonly ILogger<SessionsController> _logger;

    public SessionsController(
        ISessionService sessionService,
        IValidator<CreateSessionRequest> createValidator,
        ILogger<SessionsController> logger)
    {
        _sessionService = sessionService;
        _createValidator = createValidator;
        _logger = logger;
    }

    /// <summary>
    /// Get a session by ID
    /// </summary>
    /// <param name="id">The session ID</param>
    /// <returns>The session details</returns>
    /// <response code="200">Returns the session</response>
    /// <response code="404">Session not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SessionDto>> GetSession(int id)
    {
        _logger.LogInformation("Fetching session with ID {SessionId}", id);

        var session = await _sessionService.GetSessionAsync(id);
        
        if (session == null)
        {
            _logger.LogWarning("Session with ID {SessionId} not found", id);
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Session not found",
                Detail = $"Session with ID {id} does not exist"
            });
        }

        return Ok(session);
    }

    /// <summary>
    /// Get all sessions
    /// </summary>
    /// <returns>List of all sessions</returns>
    /// <response code="200">Returns the list of sessions</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SessionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SessionDto>>> GetAllSessions()
    {
        _logger.LogInformation("Fetching all sessions");

        var sessions = await _sessionService.GetAllSessionsAsync();
        return Ok(sessions);
    }

    /// <summary>
    /// Create a new session
    /// </summary>
    /// <param name="request">Session creation details</param>
    /// <returns>The created session</returns>
    /// <response code="201">Session created successfully</response>
    /// <response code="400">Invalid request data</response>
    [HttpPost]
    [ProducesResponseType(typeof(SessionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SessionDto>> CreateSession([FromBody] CreateSessionRequest request)
    {
        _logger.LogInformation("Creating new session");

        var validationResult = await _createValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Session creation validation failed");
            
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
            var result = await _sessionService.CreateSessionAsync(request);
            var session = (SessionDto)result;
            _logger.LogInformation("Session created successfully with ID {SessionId}", session.Id);

            return CreatedAtAction(
                nameof(GetSession),
                new { id = session.Id },
                session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating session");
            
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An error occurred",
                Detail = "Failed to create session"
            });
        }
    }

    /// <summary>
    /// Update session state
    /// </summary>
    /// <param name="id">The session ID</param>
    /// <param name="state">The new session state</param>
    /// <returns>The updated session</returns>
    /// <response code="200">Session state updated successfully</response>
    /// <response code="404">Session not found</response>
    [HttpPatch("{id}/state")]
    [ProducesResponseType(typeof(SessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SessionDto>> UpdateSessionState(
        int id,
        [FromBody] SessionState state)
    {
        _logger.LogInformation("Updating state for session ID {SessionId} to {State}", id, state);

        try
        {
            var session = await _sessionService.UpdateSessionStateAsync(id, state);
            
            if (session == null)
            {
                _logger.LogWarning("Session with ID {SessionId} not found for state update", id);
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Session not found",
                    Detail = $"Session with ID {id} does not exist"
                });
            }

            _logger.LogInformation("Session ID {SessionId} state updated to {State}", id, state);
            return Ok(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating state for session ID {SessionId}", id);
            
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An error occurred",
                Detail = "Failed to update session state"
            });
        }
    }

    /// <summary>
    /// Delete a session
    /// </summary>
    /// <param name="id">The session ID</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Session deleted successfully</response>
    /// <response code="404">Session not found</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSession(int id)
    {
        _logger.LogInformation("Deleting session with ID {SessionId}", id);

        try
        {
            var deleted = await _sessionService.DeleteSessionAsync(id);
            
            if (!deleted)
            {
                _logger.LogWarning("Session with ID {SessionId} not found for deletion", id);
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Session not found",
                    Detail = $"Session with ID {id} does not exist"
                });
            }

            _logger.LogInformation("Session with ID {SessionId} deleted successfully", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting session with ID {SessionId}", id);
            
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An error occurred",
                Detail = "Failed to delete session"
            });
        }
    }
}
