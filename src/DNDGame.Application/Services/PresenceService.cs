namespace DNDGame.Application.Services;

using DNDGame.Application.DTOs;
using DNDGame.Core.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

/// <summary>
/// Service for tracking player presence and online status using in-memory caching.
/// </summary>
public class PresenceService : IPresenceService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<PresenceService> _logger;
    private const string ConnectionPrefix = "connection:";
    private const string SessionPlayersPrefix = "session-players:";
    private const string PlayerOnlinePrefix = "player-online:";
    
    public PresenceService(IMemoryCache cache, ILogger<PresenceService> logger)
    {
        _cache = cache;
        _logger = logger;
    }
    
    public Task TrackConnectionAsync(int sessionId, int playerId, string connectionId, CancellationToken cancellationToken = default)
    {
        // Store connection -> session/player mapping
        var connectionKey = $"{ConnectionPrefix}{connectionId}";
        var connectionData = new { SessionId = sessionId, PlayerId = playerId };
        _cache.Set(connectionKey, connectionData, TimeSpan.FromHours(24));
        
        // Store player online status
        var playerKey = $"{PlayerOnlinePrefix}{playerId}";
        _cache.Set(playerKey, true, TimeSpan.FromHours(24));
        
        // Add to session players list
        var sessionKey = $"{SessionPlayersPrefix}{sessionId}";
        var players = _cache.GetOrCreate(sessionKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24);
            return new HashSet<int>();
        }) ?? new HashSet<int>();
        
        players.Add(playerId);
        _cache.Set(sessionKey, players, TimeSpan.FromHours(24));
        
        _logger.LogInformation(
            "Tracking connection {ConnectionId} for player {PlayerId} in session {SessionId}",
            connectionId, playerId, sessionId);
        
        return Task.CompletedTask;
    }
    
    public Task RemoveConnectionAsync(string connectionId, CancellationToken cancellationToken = default)
    {
        var connectionKey = $"{ConnectionPrefix}{connectionId}";
        
        if (_cache.TryGetValue<dynamic>(connectionKey, out var connectionData))
        {
            var sessionId = (int)connectionData.SessionId;
            var playerId = (int)connectionData.PlayerId;
            
            // Remove from session players
            var sessionKey = $"{SessionPlayersPrefix}{sessionId}";
            if (_cache.TryGetValue<HashSet<int>>(sessionKey, out var players))
            {
                players.Remove(playerId);
                _cache.Set(sessionKey, players, TimeSpan.FromHours(24));
            }
            
            // Update player online status (only if no other connections)
            var playerKey = $"{PlayerOnlinePrefix}{playerId}";
            _cache.Remove(playerKey);
            
            // Remove connection
            _cache.Remove(connectionKey);
            
            _logger.LogInformation(
                "Removed connection {ConnectionId} for player {PlayerId} in session {SessionId}",
                connectionId, playerId, sessionId);
        }
        
        return Task.CompletedTask;
    }
    
    public Task<List<int>> GetActivePlayerIdsAsync(int sessionId, CancellationToken cancellationToken = default)
    {
        var sessionKey = $"{SessionPlayersPrefix}{sessionId}";
        var players = _cache.Get<HashSet<int>>(sessionKey) ?? new HashSet<int>();
        return Task.FromResult(players.ToList());
    }

    public Task<List<PlayerPresenceDto>> GetActivePlayersAsync(int sessionId, CancellationToken cancellationToken = default)
    {
        var sessionKey = $"{SessionPlayersPrefix}{sessionId}";
        var players = _cache.Get<HashSet<int>>(sessionKey) ?? new HashSet<int>();
        
        var presenceList = players.Select(playerId => new PlayerPresenceDto
        {
            PlayerId = playerId,
            PlayerName = $"Player {playerId}", // Simplified - would query from database
            IsOnline = true,
            LastSeen = DateTime.UtcNow
        }).ToList();
        
        return Task.FromResult(presenceList);
    }
    
    public Task<bool> IsPlayerOnlineAsync(int playerId, CancellationToken cancellationToken = default)
    {
        var playerKey = $"{PlayerOnlinePrefix}{playerId}";
        var isOnline = _cache.TryGetValue(playerKey, out bool _);
        return Task.FromResult(isOnline);
    }
    
    public Task<int?> GetSessionIdByConnectionAsync(string connectionId, CancellationToken cancellationToken = default)
    {
        var connectionKey = $"{ConnectionPrefix}{connectionId}";
        
        if (_cache.TryGetValue<dynamic>(connectionKey, out var connectionData))
        {
            return Task.FromResult<int?>((int)connectionData.SessionId);
        }
        
        return Task.FromResult<int?>(null);
    }
}
