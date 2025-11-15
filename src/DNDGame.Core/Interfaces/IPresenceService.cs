namespace DNDGame.Core.Interfaces;

/// <summary>
/// Service for tracking player presence and online status.
/// </summary>
public interface IPresenceService
{
    /// <summary>
    /// Track a new connection for a player in a session.
    /// </summary>
    Task TrackConnectionAsync(int sessionId, int playerId, string connectionId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Remove a connection when a player disconnects.
    /// </summary>
    Task RemoveConnectionAsync(string connectionId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all active players in a session. Returns list of player IDs.
    /// </summary>
    Task<List<int>> GetActivePlayerIdsAsync(int sessionId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Check if a player is currently online.
    /// </summary>
    Task<bool> IsPlayerOnlineAsync(int playerId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get the session ID for a connection ID.
    /// </summary>
    Task<int?> GetSessionIdByConnectionAsync(string connectionId, CancellationToken cancellationToken = default);
}
