using DNDGame.MauiApp.Interfaces;
using DNDGame.MauiApp.Data;
using DNDGame.Core.Interfaces;
using DNDGame.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace DNDGame.MauiApp.Services;

public class OfflineSyncService : IOfflineSyncService
{
    private readonly LocalDatabaseContext _localDb;
    private readonly ICharacterService _characterService;
    private readonly ISessionService _sessionService;
    private readonly IConnectivityService _connectivityService;

    public OfflineSyncService(
        LocalDatabaseContext localDb,
        ICharacterService characterService,
        ISessionService sessionService,
        IConnectivityService connectivityService)
    {
        _localDb = localDb;
        _characterService = characterService;
        _sessionService = sessionService;
        _connectivityService = connectivityService;
    }

    public async Task<bool> SyncCharactersAsync()
    {
        if (!_connectivityService.IsConnected)
        {
            return false;
        }

        try
        {
            // Get all characters from API (assuming player ID = 1 for now)
            var apiCharacters = await _characterService.GetAllCharactersByPlayerAsync(1);
            var characters = apiCharacters.Cast<Character>().ToList();

            // Clear local characters and replace with API data
            _localDb.Characters.RemoveRange(_localDb.Characters);
            await _localDb.Characters.AddRangeAsync(characters);
            await _localDb.SaveChangesAsync();

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> SyncSessionsAsync()
    {
        if (!_connectivityService.IsConnected)
        {
            return false;
        }

        try
        {
            // Get all sessions from API
            var apiSessions = await _sessionService.GetAllSessionsAsync();
            var sessions = apiSessions.Cast<Session>().ToList();

            // Clear local sessions and replace with API data
            _localDb.Sessions.RemoveRange(_localDb.Sessions);
            await _localDb.Sessions.AddRangeAsync(sessions);
            await _localDb.SaveChangesAsync();

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<Character?> GetCharacterOfflineAsync(int characterId)
    {
        return await _localDb.Characters
            .FirstOrDefaultAsync(c => c.Id == characterId);
    }

    public async Task<List<Character>> GetAllCharactersOfflineAsync()
    {
        return await _localDb.Characters.ToListAsync();
    }

    public async Task SaveCharacterOfflineAsync(Character character)
    {
        var existing = await _localDb.Characters.FindAsync(character.Id);

        if (existing != null)
        {
            _localDb.Entry(existing).CurrentValues.SetValues(character);
        }
        else
        {
            await _localDb.Characters.AddAsync(character);
        }

        await _localDb.SaveChangesAsync();
    }

    public async Task<bool> HasPendingSyncData()
    {
        // Check if we have any local data that might need syncing
        var hasCharacters = await _localDb.Characters.AnyAsync();
        var hasSessions = await _localDb.Sessions.AnyAsync();

        return hasCharacters || hasSessions;
    }

    public async Task<SyncResult> PerformFullSyncAsync()
    {
        var result = new SyncResult { SyncTime = DateTime.UtcNow };

        try
        {
            var characterSuccess = await SyncCharactersAsync();
            var sessionSuccess = await SyncSessionsAsync();

            result.Success = characterSuccess && sessionSuccess;
            
            if (characterSuccess)
            {
                result.CharactersSynced = await _localDb.Characters.CountAsync();
            }
            else
            {
                result.Errors.Add("Failed to sync characters");
            }

            if (sessionSuccess)
            {
                result.SessionsSynced = await _localDb.Sessions.CountAsync();
            }
            else
            {
                result.Errors.Add("Failed to sync sessions");
            }
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Sync failed: {ex.Message}");
            result.Success = false;
        }

        return result;
    }
}
