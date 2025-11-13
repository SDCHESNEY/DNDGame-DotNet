using Microsoft.AspNetCore.Mvc;
using DNDGame.Application.DTOs;
using DNDGame.Core.Interfaces;
using DNDGame.Core.Models;
using DNDGame.Core.Enums;
using FluentValidation;

namespace DNDGame.API.Controllers;

/// <summary>
/// API endpoints for AI Dungeon Master interactions
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class DmController : ControllerBase
{
    private readonly ILlmDmService _dmService;
    private readonly ISessionRepository _sessionRepository;
    private readonly ILogger<DmController> _logger;

    public DmController(
        ILlmDmService dmService,
        ISessionRepository sessionRepository,
        ILogger<DmController> logger)
    {
        _dmService = dmService;
        _sessionRepository = sessionRepository;
        _logger = logger;
    }

    /// <summary>
    /// Generate a Dungeon Master response to player actions
    /// </summary>
    /// <param name="request">The DM response request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The DM's narrative response with suggestions</returns>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(DmResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DmResponseDto>> GenerateResponse(
        [FromBody] GenerateDmResponseRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate session exists
            var session = await _sessionRepository.GetByIdAsync(request.SessionId);
            if (session is null)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Session Not Found",
                    Detail = $"Session with ID {request.SessionId} does not exist."
                });
            }

            // Build session context
            var context = await BuildSessionContextAsync(request.SessionId, cancellationToken);

            // Generate DM response
            var dmResponse = await _dmService.GenerateResponseAsync(
                context,
                request.PlayerMessage,
                cancellationToken);

            // Map to DTO
            var responseDto = new DmResponseDto
            {
                Content = dmResponse.Content,
                SuggestedActions = dmResponse.SuggestedActions,
                TokensUsed = dmResponse.TokensUsed,
                ResponseTimeMs = Convert.ToInt64(dmResponse.ResponseTimeMs),
                EstimatedCost = dmResponse.EstimatedCost,
                WasModerated = false // TODO: Track moderation in DmResponse
            };

            _logger.LogInformation(
                "Generated DM response for session {SessionId} - {TokenCount} tokens, {ResponseTime}ms",
                request.SessionId,
                dmResponse.TokensUsed,
                dmResponse.ResponseTimeMs);

            return Ok(responseDto);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error",
                Detail = ex.Message
            });
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("DM response generation cancelled for session {SessionId}", request.SessionId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating DM response for session {SessionId}", request.SessionId);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = "An error occurred while generating the DM response."
            });
        }
    }

    /// <summary>
    /// Stream a Dungeon Master response in real-time
    /// </summary>
    /// <param name="request">The DM response request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Server-sent events stream of the DM's response</returns>
    [HttpPost("stream")]
    [Produces("text/event-stream")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task StreamResponse(
        [FromBody] GenerateDmResponseRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate session exists
            var session = await _sessionRepository.GetByIdAsync(request.SessionId);
            if (session is null)
            {
                Response.StatusCode = StatusCodes.Status404NotFound;
                await Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Session Not Found",
                    Detail = $"Session with ID {request.SessionId} does not exist."
                }, cancellationToken);
                return;
            }

            // Set response headers for SSE
            Response.ContentType = "text/event-stream";
            Response.Headers.Append("Cache-Control", "no-cache");
            Response.Headers.Append("X-Accel-Buffering", "no");

            // Build session context
            var context = await BuildSessionContextAsync(request.SessionId, cancellationToken);

            // Stream the response
            await foreach (var chunk in _dmService.StreamResponseAsync(
                context,
                request.PlayerMessage,
                cancellationToken))
            {
                // Send SSE event
                await Response.WriteAsync($"data: {chunk}\n\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }

            // Send completion event
            await Response.WriteAsync("data: [DONE]\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);

            _logger.LogInformation(
                "Streamed DM response for session {SessionId}",
                request.SessionId);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("DM response streaming cancelled for session {SessionId}", request.SessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error streaming DM response for session {SessionId}", request.SessionId);
            Response.StatusCode = StatusCodes.Status500InternalServerError;
            await Response.WriteAsync($"data: {{\"error\": \"An error occurred\"}}\n\n", cancellationToken);
        }
    }

    /// <summary>
    /// Generate NPC dialogue
    /// </summary>
    /// <param name="request">The NPC dialogue request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The NPC's dialogue response</returns>
    [HttpPost("npc")]
    [ProducesResponseType(typeof(DmResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DmResponseDto>> GenerateNpcDialogue(
        [FromBody] GenerateNpcDialogueRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate session exists
            var session = await _sessionRepository.GetByIdAsync(request.SessionId);
            if (session is null)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Session Not Found",
                    Detail = $"Session with ID {request.SessionId} does not exist."
                });
            }

            // Build session context
            var context = await BuildSessionContextAsync(request.SessionId, cancellationToken);

            // Build NPC context
            var npcContext = new NpcContext(
                request.NpcName,
                request.Personality,
                request.Occupation,
                request.Mood,
                request.Metadata?.ToDictionary(k => k.Key, k => (object)k.Value) ?? new Dictionary<string, object>());

            // Generate NPC dialogue
            var dmResponse = await _dmService.GenerateNpcDialogueAsync(
                context,
                npcContext,
                request.PlayerMessage,
                cancellationToken);

            var responseDto = new DmResponseDto
            {
                Content = dmResponse.Content,
                SuggestedActions = dmResponse.SuggestedActions,
                TokensUsed = dmResponse.TokensUsed,
                ResponseTimeMs = Convert.ToInt64(dmResponse.ResponseTimeMs),
                EstimatedCost = dmResponse.EstimatedCost,
                WasModerated = false
            };

            _logger.LogInformation(
                "Generated NPC dialogue for {NpcName} in session {SessionId}",
                request.NpcName,
                request.SessionId);

            return Ok(responseDto);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error",
                Detail = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating NPC dialogue for session {SessionId}", request.SessionId);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = "An error occurred while generating NPC dialogue."
            });
        }
    }

    /// <summary>
    /// Generate a scene description
    /// </summary>
    /// <param name="request">The scene description request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A vivid scene description</returns>
    [HttpPost("scene")]
    [ProducesResponseType(typeof(DmResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DmResponseDto>> GenerateSceneDescription(
        [FromBody] GenerateSceneDescriptionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate session exists
            var session = await _sessionRepository.GetByIdAsync(request.SessionId);
            if (session is null)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Session Not Found",
                    Detail = $"Session with ID {request.SessionId} does not exist."
                });
            }

            // Build session context
            var context = await BuildSessionContextAsync(request.SessionId, cancellationToken);

            // Build location context
            var locationContext = new LocationContext(
                request.LocationName,
                request.LocationType,
                request.Description,
                request.Features ?? [],
                request.NpcsPresent ?? [],
                request.Details?.ToDictionary(k => k.Key, k => (object)k.Value) ?? new Dictionary<string, object>());

            // Generate scene description
            var dmResponse = await _dmService.DescribeSceneAsync(
                context,
                locationContext,
                cancellationToken);

            var responseDto = new DmResponseDto
            {
                Content = dmResponse.Content,
                SuggestedActions = dmResponse.SuggestedActions,
                TokensUsed = dmResponse.TokensUsed,
                ResponseTimeMs = Convert.ToInt64(dmResponse.ResponseTimeMs),
                EstimatedCost = dmResponse.EstimatedCost,
                WasModerated = false
            };

            _logger.LogInformation(
                "Generated scene description for {LocationName} in session {SessionId}",
                request.LocationName,
                request.SessionId);

            return Ok(responseDto);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error",
                Detail = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating scene description for session {SessionId}", request.SessionId);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = "An error occurred while generating the scene description."
            });
        }
    }

    /// <summary>
    /// Build session context for LLM processing
    /// </summary>
    private async Task<SessionContext> BuildSessionContextAsync(int sessionId, CancellationToken cancellationToken)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken);
        if (session is null)
        {
            throw new InvalidOperationException($"Session {sessionId} not found");
        }

        // Get recent messages (last 20)
        var recentMessages = session.Messages
            .OrderByDescending(m => m.Timestamp)
            .Take(20)
            .OrderBy(m => m.Timestamp)
            .ToList();

        // Get active characters from participants
        var activeCharacters = session.Participants
            .Where(p => p.IsActive)
            .Select(p => p.Character)
            .ToList();

        // Build world flags from session state
        var worldFlags = new Dictionary<string, object>
        {
            ["InCombat"] = session.State == SessionState.InProgress, // Simplified
            ["SessionMode"] = session.Mode.ToString()
        };

        return new SessionContext(
            sessionId,
            recentMessages,
            activeCharacters,
            session.CurrentScene, // Use the session's current scene
            worldFlags);
    }
}
