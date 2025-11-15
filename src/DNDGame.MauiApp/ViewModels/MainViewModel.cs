using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using DNDGame.Core.Entities;
using DNDGame.Core.Interfaces;
using DNDGame.MauiApp.Interfaces;

namespace DNDGame.MauiApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IConnectivityService _connectivityService;
    private readonly IOfflineSyncService _offlineSyncService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private bool isConnected;

    [ObservableProperty]
    private bool isSyncing;

    [ObservableProperty]
    private string syncStatus = "Ready";

    [ObservableProperty]
    private DateTime lastSyncTime;

    public MainViewModel(
        IConnectivityService connectivityService,
        IOfflineSyncService offlineSyncService,
        INavigationService navigationService)
    {
        _connectivityService = connectivityService;
        _offlineSyncService = offlineSyncService;
        _navigationService = navigationService;

        IsConnected = _connectivityService.IsConnected;
        _connectivityService.ConnectivityChanged += OnConnectivityChanged;
    }

    [RelayCommand]
    private async Task NavigateToCharactersAsync()
    {
        await _navigationService.NavigateToAsync("characters");
    }

    [RelayCommand]
    private async Task NavigateToSessionsAsync()
    {
        await _navigationService.NavigateToAsync("sessions");
    }

    [RelayCommand]
    private async Task NavigateToDiceAsync()
    {
        await _navigationService.NavigateToAsync("dice");
    }

    [RelayCommand]
    private async Task SyncDataAsync()
    {
        if (!IsConnected)
        {
            SyncStatus = "No internet connection";
            return;
        }

        IsSyncing = true;
        SyncStatus = "Syncing...";

        try
        {
            var result = await _offlineSyncService.PerformFullSyncAsync();
            
            if (result.Success)
            {
                SyncStatus = $"Synced {result.CharactersSynced} characters, {result.SessionsSynced} sessions";
                LastSyncTime = result.SyncTime;
            }
            else
            {
                SyncStatus = $"Sync failed: {string.Join(", ", result.Errors)}";
            }
        }
        catch (Exception ex)
        {
            SyncStatus = $"Sync error: {ex.Message}";
        }
        finally
        {
            IsSyncing = false;
        }
    }

    private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        IsConnected = e.IsConnected;
        
        if (IsConnected && !IsSyncing)
        {
            // Auto-sync when connection is restored
            _ = Task.Run(async () => await SyncDataAsync());
        }
    }
}