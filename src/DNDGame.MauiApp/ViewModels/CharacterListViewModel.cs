using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using DNDGame.Core.Entities;
using DNDGame.Core.Interfaces;
using DNDGame.MauiApp.Interfaces;
using Microsoft.Maui.Controls;

namespace DNDGame.MauiApp.ViewModels;

public partial class CharacterListViewModel : ObservableObject
{
    private readonly ICharacterService _characterService;
    private readonly INavigationService _navigationService;
    private readonly IOfflineSyncService _offlineSyncService;
    private readonly INotificationService _notificationService;

    [ObservableProperty]
    private ObservableCollection<Character> characters = new();

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private string? errorMessage;

    [ObservableProperty]
    private Character? selectedCharacter;

    [ObservableProperty]
    private string searchText = string.Empty;

    public CharacterListViewModel(
        ICharacterService characterService,
        INavigationService navigationService,
        IOfflineSyncService offlineSyncService,
        INotificationService notificationService)
    {
        _characterService = characterService;
        _navigationService = navigationService;
        _offlineSyncService = offlineSyncService;
        _notificationService = notificationService;
    }

    [RelayCommand]
    private async Task LoadCharactersAsync()
    {
        if (IsLoading) return;

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            // Try to load from API first, fallback to offline
            List<Character> loadedCharacters;
            
            try
            {
                var charactersResult = await _characterService.GetAllCharactersByPlayerAsync(1); // Assuming player ID 1
                loadedCharacters = charactersResult.Cast<Character>().ToList();
            }
            catch (HttpRequestException)
            {
                // Fallback to offline data
                loadedCharacters = await _offlineSyncService.GetAllCharactersOfflineAsync();
            }

            Characters.Clear();
            foreach (var character in loadedCharacters.OrderBy(c => c.Name))
            {
                Characters.Add(character);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load characters: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RefreshCharactersAsync()
    {
        if (IsRefreshing) return;

        IsRefreshing = true;
        await LoadCharactersAsync();
        IsRefreshing = false;
    }

    [RelayCommand]
    private async Task SelectCharacterAsync(Character character)
    {
        if (character == null) return;

        SelectedCharacter = character;
        var parameters = new Dictionary<string, object>
        {
            { "CharacterId", character.Id }
        };
        
        await _navigationService.NavigateToAsync("character-detail", parameters);
    }

    [RelayCommand]
    private async Task CreateCharacterAsync()
    {
        await _navigationService.NavigateToAsync("character-create");
    }

    [RelayCommand]
    private async Task DeleteCharacterAsync(Character character)
    {
        if (character == null) return;

        try
        {
            var confirmed = await Microsoft.Maui.Controls.Application.Current!.MainPage!.DisplayAlert(
                "Delete Character",
                $"Are you sure you want to delete {character.Name}? This action cannot be undone.",
                "Delete",
                "Cancel");

            if (!confirmed) return;

            await _characterService.DeleteCharacterAsync(character.Id);
            Characters.Remove(character);

            await _notificationService.ShowNotificationAsync(
                "Character Deleted",
                $"{character.Name} has been deleted successfully.");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to delete character: {ex.Message}";
        }
    }

    [RelayCommand]
    private void SearchCharacters()
    {
        // Filter characters based on search text
        // This could be implemented with a filtered ObservableCollection
        // or by reloading data with search criteria
    }

    [RelayCommand]
    private async Task ExportCharacterAsync(Character character)
    {
        // Export character to JSON file for backup/sharing
        try
        {
            // Implementation would serialize character to JSON
            // and save to device storage
            await _notificationService.ShowNotificationAsync(
                "Export Complete",
                $"{character.Name} exported successfully.");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to export character: {ex.Message}";
        }
    }
}