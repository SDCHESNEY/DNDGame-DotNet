namespace DNDGame.API.Hubs;

using DNDGame.Application.DTOs;
using DNDGame.Core.Enums;
using DNDGame.Core.Models;

/// <summary>
/// Client-side methods that the SignalR hub can invoke.
/// </summary>
public interface IGameSessionClient
{
    /// <summary>
    /// Notify clients when a player joins the session.
    /// </summary>
    Task PlayerJoined(string connectionId, int characterId, string characterName);
    
    /// <summary>
    /// Notify clients when a player leaves the session.
    /// </summary>
    Task PlayerLeft(string connectionId, int characterId, string characterName);
    
    /// <summary>
    /// Receive a chat message.
    /// </summary>
    Task ReceiveMessage(MessageDto message);
    
    /// <summary>
    /// Receive a response from the Dungeon Master.
    /// </summary>
    Task DungeonMasterResponse(string content, bool wasModerated);
    
    /// <summary>
    /// Notify clients of a dice roll result.
    /// </summary>
    Task DiceRolled(string playerName, DiceRollResult result);
    
    /// <summary>
    /// Notify clients of initiative order after rolling.
    /// </summary>
    Task InitiativeRolled(List<InitiativeEntryDto> order);
    
    /// <summary>
    /// Notify clients when the turn changes in combat.
    /// </summary>
    Task TurnChanged(int currentCharacterId, string characterName);
    
    /// <summary>
    /// Notify clients when the session state changes.
    /// </summary>
    Task SessionStateChanged(SessionState newState);
    
    /// <summary>
    /// Notify clients when combat starts.
    /// </summary>
    Task CombatStarted(int sessionId);
    
    /// <summary>
    /// Notify clients when combat ends.
    /// </summary>
    Task CombatEnded(int sessionId);
    
    /// <summary>
    /// Send an error message to the client.
    /// </summary>
    Task Error(string message);
}
