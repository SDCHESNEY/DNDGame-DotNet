using DNDGame.Core.Entities;

namespace DNDGame.MauiApp.Interfaces;

public interface IOfflineSyncService
{
    Task<bool> SyncCharactersAsync();
    Task<bool> SyncSessionsAsync();
    Task<Character?> GetCharacterOfflineAsync(int id);
    Task<List<Character>> GetAllCharactersOfflineAsync();
    Task SaveCharacterOfflineAsync(Character character);
    Task<bool> HasPendingSyncData();
    Task<SyncResult> PerformFullSyncAsync();
}

public class SyncResult
{
    public bool Success { get; set; }
    public int CharactersSynced { get; set; }
    public int SessionsSynced { get; set; }
    public List<string> Errors { get; set; } = new();
    public DateTime SyncTime { get; set; } = DateTime.UtcNow;
}
