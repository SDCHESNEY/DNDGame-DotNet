using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DNDGame.Core.Entities;
using DNDGame.Core.Enums;
using DNDGame.Core.Interfaces;
using DNDGame.MauiApp.Interfaces;
using System.Collections.ObjectModel;

namespace DNDGame.MauiApp.ViewModels;

public partial class SessionDetailViewModel : ObservableObject, IQueryAttributable
{
    private readonly ISessionService _sessionService;
    private readonly INavigationService _navigationService;
    private readonly INotificationService _notificationService;
    private readonly IConnectivityService _connectivityService;
    private int _sessionId;

    [ObservableProperty]
    private Session? session;

    [ObservableProperty]
    private ObservableCollection<Message> messages = [];

    [ObservableProperty]
    private ObservableCollection<DiceRoll> diceRolls = [];

    [ObservableProperty]
    private string messageText = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string? errorMessage;

    [ObservableProperty]
    private bool isConnected;

    public SessionDetailViewModel(
        ISessionService sessionService,
        INavigationService navigationService,
        INotificationService notificationService,
        IConnectivityService connectivityService)
    {
        _sessionService = sessionService;
        _navigationService = navigationService;
        _notificationService = notificationService;
        _connectivityService = connectivityService;

        IsConnected = _connectivityService.IsConnected;
        _connectivityService.ConnectivityChanged += OnConnectivityChanged;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("SessionId"))
        {
            _sessionId = (int)query["SessionId"];
            Task.Run(async () => await LoadSessionAsync());
        }
    }

    [RelayCommand]
    private async Task LoadSessionAsync()
    {
        if (IsLoading)
            return;

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var sessionData = await _sessionService.GetSessionByIdAsync(_sessionId);
            Session = sessionData as Session;

            if (Session != null)
            {
                Messages = new ObservableCollection<Message>(Session.Messages);
                DiceRolls = new ObservableCollection<DiceRoll>(Session.DiceRolls);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading session: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(MessageText) || Session == null)
            return;

        try
        {
            var message = new Message
            {
                SessionId = Session.Id,
                Content = MessageText,
                Role = MessageRole.User,
                Timestamp = DateTime.UtcNow
            };

            // In a real implementation, this would send through SignalR
            Messages.Add(message);
            MessageText = string.Empty;

            await _notificationService.ShowNotificationAsync("Message Sent", "Your message was sent successfully");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error sending message: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task StartSessionAsync()
    {
        if (Session == null)
            return;

        try
        {
            Session.State = SessionState.InProgress;
            // In a real implementation, this would call the API to update session state
            await _notificationService.ShowNotificationAsync("Session Started", $"Session '{Session.Title}' has started!");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error starting session: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task EndSessionAsync()
    {
        if (Session == null)
            return;

        try
        {
            var confirm = await Microsoft.Maui.Controls.Application.Current?.MainPage!.DisplayAlert(
                "End Session",
                "Are you sure you want to end this session?",
                "Yes",
                "No") ?? false;

            if (!confirm)
                return;

            Session.State = SessionState.Completed;
            await _notificationService.ShowNotificationAsync("Session Ended", $"Session '{Session.Title}' has ended");
            await _navigationService.GoBackAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error ending session: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await _navigationService.GoBackAsync();
    }

    private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        IsConnected = e.IsConnected;
        
        if (IsConnected)
        {
            Task.Run(async () => await LoadSessionAsync());
        }
    }
}
