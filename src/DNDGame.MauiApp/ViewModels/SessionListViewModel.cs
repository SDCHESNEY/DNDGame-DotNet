using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DNDGame.Core.Entities;
using DNDGame.Core.Interfaces;
using DNDGame.MauiApp.Interfaces;
using System.Collections.ObjectModel;

namespace DNDGame.MauiApp.ViewModels;

public partial class SessionListViewModel : ObservableObject
{
    private readonly ISessionService _sessionService;
    private readonly IOfflineSyncService _offlineSyncService;
    private readonly INavigationService _navigationService;
    private readonly INotificationService _notificationService;
    private readonly IConnectivityService _connectivityService;

    [ObservableProperty]
    private ObservableCollection<Session> sessions = [];

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private string? errorMessage;

    [ObservableProperty]
    private Session? selectedSession;

    public SessionListViewModel(
        ISessionService sessionService,
        IOfflineSyncService offlineSyncService,
        INavigationService navigationService,
        INotificationService notificationService,
        IConnectivityService connectivityService)
    {
        _sessionService = sessionService;
        _offlineSyncService = offlineSyncService;
        _navigationService = navigationService;
        _notificationService = notificationService;
        _connectivityService = connectivityService;
    }

    [RelayCommand]
    private async Task LoadSessionsAsync()
    {
        if (IsLoading)
            return;

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            if (_connectivityService.IsConnected)
            {
                var apiSessions = await _sessionService.GetAllSessionsAsync();
                Sessions = new ObservableCollection<Session>(apiSessions.Cast<Session>());
            }
            else
            {
                // Load from local database when offline
                var localSessions = await _offlineSyncService.GetAllCharactersOfflineAsync();
                ErrorMessage = "Offline mode - showing cached data";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading sessions: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RefreshSessionsAsync()
    {
        IsRefreshing = true;
        await LoadSessionsAsync();
        IsRefreshing = false;
    }

    [RelayCommand]
    private async Task SelectSessionAsync(Session session)
    {
        SelectedSession = session;
        await _navigationService.NavigateToAsync($"session-detail", new Dictionary<string, object>
        {
            { "SessionId", session.Id }
        });
    }

    [RelayCommand]
    private async Task CreateSessionAsync()
    {
        await _navigationService.NavigateToAsync("session-create");
    }

    [RelayCommand]
    private async Task DeleteSessionAsync(int sessionId)
    {
        try
        {
            if (Microsoft.Maui.Controls.Application.Current?.MainPage == null)
                return;

            var confirm = await Microsoft.Maui.Controls.Application.Current.MainPage.DisplayAlert(
                "Delete Session",
                "Are you sure you want to delete this session?",
                "Yes",
                "No");

            if (!confirm)
                return;

            await _sessionService.DeleteSessionAsync(sessionId);
            await LoadSessionsAsync();
            await _notificationService.ShowNotificationAsync("Success", "Session deleted successfully");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error deleting session: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task JoinSessionAsync(int sessionId)
    {
        try
        {
            // Navigate to session detail to join
            await _navigationService.NavigateToAsync($"session-detail", new Dictionary<string, object>
            {
                { "SessionId", sessionId }
            });
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error joining session: {ex.Message}";
        }
    }
}
