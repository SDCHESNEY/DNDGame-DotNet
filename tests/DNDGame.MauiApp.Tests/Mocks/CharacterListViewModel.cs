using System.Collections.ObjectModel;
using DNDGame.Core.Entities;
using DNDGame.Core.Interfaces;
using DNDGame.MauiApp.Interfaces;

namespace DNDGame.MauiApp.Tests.Mocks;

// Simplified ViewModel for testing without CommunityToolkit.Mvvm dependency
public class CharacterListViewModel
{
    private readonly ICharacterService _characterService;
    private readonly INavigationService _navigationService;
    private readonly IOfflineSyncService _offlineSyncService;
    private readonly INotificationService _notificationService;

    public ObservableCollection<Character> Characters { get; set; } = new();
    public bool IsLoading { get; set; }
    public bool IsRefreshing { get; set; }
    public string? ErrorMessage { get; set; }
    public Character? SelectedCharacter { get; set; }
    public string SearchText { get; set; } = string.Empty;

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

    public async Task LoadCharactersAsync()
    {
        if (IsLoading) return;

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            List<Character> loadedCharacters;
            
            try
            {
                var charactersResult = await _characterService.GetAllCharactersByPlayerAsync(1);
                loadedCharacters = charactersResult.Cast<Character>().ToList();
            }
            catch (HttpRequestException)
            {
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

    public async Task RefreshCharactersAsync()
    {
        if (IsRefreshing) return;

        IsRefreshing = true;
        await LoadCharactersAsync();
        IsRefreshing = false;
    }

    public async Task SelectCharacterAsync(Character? character)
    {
        if (character == null) return;

        SelectedCharacter = character;
        var parameters = new Dictionary<string, object>
        {
            { "CharacterId", character.Id }
        };
        
        await _navigationService.NavigateToAsync("character-detail", parameters);
    }

    public async Task CreateCharacterAsync()
    {
        await _navigationService.NavigateToAsync("character-create");
    }

    public async Task DeleteCharacterAsync(Character? character)
    {
        if (character == null) return;

        try
        {
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
}
