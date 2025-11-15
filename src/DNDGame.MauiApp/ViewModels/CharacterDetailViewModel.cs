using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DNDGame.Core.Entities;
using DNDGame.Core.Interfaces;
using DNDGame.MauiApp.Interfaces;

namespace DNDGame.MauiApp.ViewModels;

public partial class CharacterDetailViewModel : ObservableObject, IQueryAttributable
{
    private readonly ICharacterService _characterService;
    private readonly INavigationService _navigationService;
    private readonly INotificationService _notificationService;
    private readonly IOfflineSyncService _offlineSyncService;

    [ObservableProperty]
    private Character? character;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string? errorMessage;

    [ObservableProperty]
    private bool isEditing;

    public CharacterDetailViewModel(
        ICharacterService characterService,
        INavigationService navigationService,
        INotificationService notificationService,
        IOfflineSyncService offlineSyncService)
    {
        _characterService = characterService;
        _navigationService = navigationService;
        _notificationService = notificationService;
        _offlineSyncService = offlineSyncService;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("CharacterId", out var characterIdObj) && 
            int.TryParse(characterIdObj.ToString(), out var characterId))
        {
            _ = LoadCharacterAsync(characterId);
        }
    }

    [RelayCommand]
    private async Task LoadCharacterAsync(int characterId)
    {
        if (IsLoading) return;

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            // Try to load from API first, fallback to offline
            object? characterResult;
            
            try
            {
                characterResult = await _characterService.GetCharacterAsync(characterId);
            }
            catch (HttpRequestException)
            {
                // Fallback to offline data
                characterResult = await _offlineSyncService.GetCharacterOfflineAsync(characterId);
            }

            Character = characterResult as Character;
            
            if (Character == null)
            {
                ErrorMessage = "Character not found";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load character: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ToggleEditMode()
    {
        IsEditing = !IsEditing;
    }

    [RelayCommand]
    private async Task SaveCharacterAsync()
    {
        if (Character == null) return;

        try
        {
            // Save to offline storage first
            await _offlineSyncService.SaveCharacterOfflineAsync(Character);

            // Try to sync to API
            try
            {
                await _characterService.UpdateCharacterAsync(Character.Id, Character);
                await _notificationService.ShowNotificationAsync(
                    "Character Saved",
                    $"{Character.Name} has been saved successfully.");
            }
            catch (HttpRequestException)
            {
                // Will sync later when online
                await _notificationService.ShowNotificationAsync(
                    "Saved Offline",
                    $"{Character.Name} saved offline. Will sync when online.");
            }

            IsEditing = false;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to save character: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task LevelUpAsync()
    {
        if (Character == null) return;

        // Implement level up logic
        Character.Level++;
        Character.MaxHitPoints += 10; // Simplified HP gain

        await SaveCharacterAsync();
        
        await _notificationService.ShowNotificationAsync(
            "Level Up!",
            $"{Character.Name} is now level {Character.Level}!");
    }

    [RelayCommand]
    private async Task RestoreHitPointsAsync()
    {
        if (Character == null) return;

        Character.HitPoints = Character.MaxHitPoints;
        await SaveCharacterAsync();
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        if (IsEditing)
        {
            IsEditing = false;
        }
        else
        {
            await _navigationService.GoBackAsync();
        }
    }
}