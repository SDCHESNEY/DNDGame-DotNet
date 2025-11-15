namespace DNDGame.API.Hubs;

using DNDGame.Application.DTOs;
using DNDGame.Core.Enums;
using DNDGame.Core.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

/// <summary>
/// SignalR hub for real-time game session communication.
/// </summary>
public class GameSessionHub : Hub<IGameSessionClient>
{
    private readonly ISessionService _sessionService;
    private readonly IPresenceService _presenceService;
    private readonly IDiceRoller _diceRoller;
    private readonly ICombatService _combatService;
    private readonly ILogger<GameSessionHub> _logger;
    
    public GameSessionHub(
        ISessionService sessionService,
        IPresenceService presenceService,
        IDiceRoller diceRoller,
        ICombatService combatService,
        ILogger<GameSessionHub> logger)
    {
        _sessionService = sessionService;
        _presenceService = presenceService;
        _diceRoller = diceRoller;
        _combatService = combatService;
        _logger = logger;
    }
    
    /// <summary>
    /// Join a game session with a character.
    /// </summary>
    public async Task JoinSession(int sessionId, int characterId, int playerId)
    {
        try
        {
            // Verify session exists
            var session = await _sessionService.GetSessionAsync(sessionId);
            if (session == null)
            {
                await Clients.Caller.Error("Session not found");
                return;
            }
            
            // Add to session group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"session-{sessionId}");
            
            // Track presence
            await _presenceService.TrackConnectionAsync(sessionId, playerId, Context.ConnectionId);
            
            // Join session
            var success = await _sessionService.JoinSessionAsync(sessionId, characterId);
            if (!success)
            {
                await Clients.Caller.Error("Failed to join session");
                return;
            }
            
            // Get character name (simplified - would normally query from repository)
            var characterName = $"Character {characterId}";
            
            // Notify all clients in the session
            await Clients.Group($"session-{sessionId}")
                .PlayerJoined(Context.ConnectionId, characterId, characterName);
            
            _logger.LogInformation(
                "Player {PlayerId} joined session {SessionId} with character {CharacterId}",
                playerId, sessionId, characterId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining session {SessionId}", sessionId);
            await Clients.Caller.Error("An error occurred while joining the session");
        }
    }
    
    /// <summary>
    /// Leave the current game session.
    /// </summary>
    public async Task LeaveSession(int sessionId, int characterId)
    {
        try
        {
            // Get character name (simplified)
            var characterName = $"Character {characterId}";
            
            // Leave session
            var success = await _sessionService.LeaveSessionAsync(sessionId, characterId);
            
            // Remove from group
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"session-{sessionId}");
            
            // Remove presence
            await _presenceService.RemoveConnectionAsync(Context.ConnectionId);
            
            // Notify all clients
            await Clients.Group($"session-{sessionId}")
                .PlayerLeft(Context.ConnectionId, characterId, characterName);
            
            _logger.LogInformation(
                "Character {CharacterId} left session {SessionId}",
                characterId, sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving session {SessionId}", sessionId);
            await Clients.Caller.Error("An error occurred while leaving the session");
        }
    }
    
    /// <summary>
    /// Send a message to all players in the session.
    /// </summary>
    public async Task SendMessage(int sessionId, string content, int characterId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                await Clients.Caller.Error("Message cannot be empty");
                return;
            }
            
            // Save message
            var message = await _sessionService.SaveMessageAsync(
                sessionId, 
                content, 
                MessageRole.Player);
            
            // Create DTO
            var messageDto = new MessageDto
            {
                Id = message.Id,
                SessionId = sessionId,
                Role = MessageRole.Player,
                Content = content,
                Timestamp = message.Timestamp,
                PlayerName = $"Character {characterId}",
                CharacterId = characterId
            };
            
            // Broadcast to all clients in session
            await Clients.Group($"session-{sessionId}").ReceiveMessage(messageDto);
            
            _logger.LogInformation(
                "Message sent in session {SessionId} by character {CharacterId}",
                sessionId, characterId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message in session {SessionId}", sessionId);
            await Clients.Caller.Error("Failed to send message");
        }
    }
    
    /// <summary>
    /// Send a player action to the session.
    /// </summary>
    public async Task SendAction(int sessionId, PlayerActionDto action)
    {
        try
        {
            // Save action as message
            var actionMessage = $"{action.ActionType}: {action.Description}";
            var message = await _sessionService.SaveMessageAsync(
                sessionId,
                actionMessage,
                MessageRole.System);
            
            // Create message DTO
            var messageDto = new MessageDto
            {
                Id = message.Id,
                SessionId = sessionId,
                Role = MessageRole.System,
                Content = actionMessage,
                Timestamp = message.Timestamp,
                CharacterId = action.CharacterId
            };
            
            // Broadcast action to all clients
            await Clients.Group($"session-{sessionId}").ReceiveMessage(messageDto);
            
            _logger.LogInformation(
                "Action {ActionType} performed in session {SessionId}",
                action.ActionType, sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing action in session {SessionId}", sessionId);
            await Clients.Caller.Error("Failed to process action");
        }
    }
    
    /// <summary>
    /// Roll dice and broadcast the result.
    /// </summary>
    public async Task RollDice(int sessionId, string formula, string playerName)
    {
        try
        {
            // Roll dice
            var result = _diceRoller.Roll(formula);
            
            // Save as dice roll
            await _sessionService.SaveDiceRollAsync(sessionId, formula, result);
            
            // Broadcast to all clients
            await Clients.Group($"session-{sessionId}").DiceRolled(playerName, result);
            
            _logger.LogInformation(
                "Dice rolled in session {SessionId}: {Formula} = {Total}",
                sessionId, formula, result.Total);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rolling dice in session {SessionId}: {Formula}", sessionId, formula);
            await Clients.Caller.Error($"Invalid dice formula: {formula}");
        }
    }
    
    /// <summary>
    /// Roll initiative for all characters in the session.
    /// </summary>
    public async Task RequestInitiative(int sessionId)
    {
        try
        {
            // Get initiative order
            var initiativeOrder = _combatService.RollInitiative(sessionId);
            
            // Convert to DTOs
            var initiativeDtos = initiativeOrder.Entries
                .Select(e => new InitiativeEntryDto
                {
                    CharacterId = e.CharacterId,
                    CharacterName = e.CharacterName,
                    InitiativeRoll = e.InitiativeRoll,
                    CurrentHP = e.CurrentHP,
                    MaxHP = e.MaxHP,
                    Conditions = e.Conditions
                })
                .ToList();
            
            // Broadcast to all clients
            await Clients.Group($"session-{sessionId}").InitiativeRolled(initiativeDtos);
            
            // Notify combat started
            await Clients.Group($"session-{sessionId}").CombatStarted(sessionId);
            
            // Set first turn
            if (initiativeDtos.Any())
            {
                var firstCharacter = initiativeDtos.First();
                await Clients.Group($"session-{sessionId}")
                    .TurnChanged(firstCharacter.CharacterId, firstCharacter.CharacterName);
            }
            
            _logger.LogInformation("Initiative rolled for session {SessionId}", sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rolling initiative for session {SessionId}", sessionId);
            await Clients.Caller.Error("Failed to roll initiative");
        }
    }
    
    /// <summary>
    /// End the current character's turn.
    /// </summary>
    public async Task EndTurn(int sessionId, int currentCharacterId)
    {
        try
        {
            // Get next character in initiative order (simplified)
            // In a real implementation, this would track turn order in the session state
            
            var nextCharacterId = currentCharacterId + 1; // Simplified
            var nextCharacterName = $"Character {nextCharacterId}";
            
            await Clients.Group($"session-{sessionId}")
                .TurnChanged(nextCharacterId, nextCharacterName);
            
            _logger.LogInformation(
                "Turn ended for character {CharacterId} in session {SessionId}",
                currentCharacterId, sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending turn in session {SessionId}", sessionId);
            await Clients.Caller.Error("Failed to end turn");
        }
    }
    
    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            // Get session ID from presence service
            var sessionId = await _presenceService.GetSessionIdByConnectionAsync(Context.ConnectionId);
            
            if (sessionId.HasValue)
            {
                // Remove presence
                await _presenceService.RemoveConnectionAsync(Context.ConnectionId);
                
                _logger.LogInformation(
                    "Client disconnected from session {SessionId}: {ConnectionId}",
                    sessionId.Value, Context.ConnectionId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling disconnection for {ConnectionId}", Context.ConnectionId);
        }
        
        await base.OnDisconnectedAsync(exception);
    }
}
