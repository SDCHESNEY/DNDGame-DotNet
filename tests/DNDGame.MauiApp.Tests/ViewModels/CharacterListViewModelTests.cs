using DNDGame.Core.Entities;
using DNDGame.Core.Enums;
using DNDGame.Core.Interfaces;
using DNDGame.Core.ValueObjects;
using DNDGame.MauiApp.Interfaces;
using DNDGame.MauiApp.Tests.Mocks;
using FluentAssertions;
using Moq;

namespace DNDGame.MauiApp.Tests.ViewModels;

public class CharacterListViewModelTests
{
    private readonly Mock<ICharacterService> _mockCharacterService;
    private readonly Mock<INavigationService> _mockNavigationService;
    private readonly Mock<IOfflineSyncService> _mockOfflineSyncService;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly CharacterListViewModel _viewModel;

    public CharacterListViewModelTests()
    {
        _mockCharacterService = new Mock<ICharacterService>();
        _mockNavigationService = new Mock<INavigationService>();
        _mockOfflineSyncService = new Mock<IOfflineSyncService>();
        _mockNotificationService = new Mock<INotificationService>();

        _viewModel = new CharacterListViewModel(
            _mockCharacterService.Object,
            _mockNavigationService.Object,
            _mockOfflineSyncService.Object,
            _mockNotificationService.Object);
    }

    private Character CreateTestCharacter(int id = 1, string name = "Gandalf")
    {
        return new Character
        {
            Id = id,
            PlayerId = 1,
            Name = name,
            Class = CharacterClass.Wizard,
            Level = 5,
            AbilityScores = new AbilityScores(10, 14, 12, 18, 16, 12),
            MaxHitPoints = 30,
            HitPoints = 30,
            ArmorClass = 12
        };
    }

    [Fact]
    public async Task LoadCharactersAsync_WithValidData_LoadsCharacters()
    {
        // Arrange
        var characters = new List<Character>
        {
            CreateTestCharacter(1, "Gandalf"),
            CreateTestCharacter(2, "Aragorn")
        };

        _mockCharacterService
            .Setup(s => s.GetAllCharactersByPlayerAsync(It.IsAny<int>()))
            .ReturnsAsync(characters);

        // Act
        await _viewModel.LoadCharactersAsync();

        // Assert
        _viewModel.Characters.Should().HaveCount(2);
        _viewModel.IsLoading.Should().BeFalse();
        _viewModel.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task LoadCharactersAsync_WhenApiThrows_FallsBackToOffline()
    {
        // Arrange
        var offlineCharacters = new List<Character>
        {
            CreateTestCharacter(1, "Offline Character")
        };

        _mockCharacterService
            .Setup(s => s.GetAllCharactersByPlayerAsync(It.IsAny<int>()))
            .ThrowsAsync(new HttpRequestException("Network error"));

        _mockOfflineSyncService
            .Setup(s => s.GetAllCharactersOfflineAsync())
            .ReturnsAsync(offlineCharacters);

        // Act
        await _viewModel.LoadCharactersAsync();

        // Assert
        _viewModel.Characters.Should().HaveCount(1);
        _viewModel.Characters.First().Name.Should().Be("Offline Character");
        _viewModel.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task LoadCharactersAsync_OrdersCharactersByName()
    {
        // Arrange
        var characters = new List<Character>
        {
            CreateTestCharacter(1, "Zelda"),
            CreateTestCharacter(2, "Aragorn"),
            CreateTestCharacter(3, "Merlin")
        };

        _mockCharacterService
            .Setup(s => s.GetAllCharactersByPlayerAsync(It.IsAny<int>()))
            .ReturnsAsync(characters);

        // Act
        await _viewModel.LoadCharactersAsync();

        // Assert
        _viewModel.Characters.Should().HaveCount(3);
        _viewModel.Characters[0].Name.Should().Be("Aragorn");
        _viewModel.Characters[1].Name.Should().Be("Merlin");
        _viewModel.Characters[2].Name.Should().Be("Zelda");
    }

    [Fact]
    public async Task LoadCharactersAsync_WithException_SetsErrorMessage()
    {
        // Arrange
        _mockCharacterService
            .Setup(s => s.GetAllCharactersByPlayerAsync(It.IsAny<int>()))
            .ThrowsAsync(new HttpRequestException("Network error"));

        _mockOfflineSyncService
            .Setup(s => s.GetAllCharactersOfflineAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        await _viewModel.LoadCharactersAsync();

        // Assert
        _viewModel.ErrorMessage.Should().NotBeNull();
        _viewModel.ErrorMessage.Should().Contain("Failed to load characters");
        _viewModel.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task LoadCharactersAsync_WhenAlreadyLoading_DoesNotStartNewLoad()
    {
        // Arrange
        _mockCharacterService
            .Setup(s => s.GetAllCharactersByPlayerAsync(It.IsAny<int>()))
            .Returns(async () =>
            {
                await Task.Delay(100);
                return new List<Character>();
            });

        // Act
        var task1 = _viewModel.LoadCharactersAsync();
        var task2 = _viewModel.LoadCharactersAsync();

        await Task.WhenAll(task1, task2);

        // Assert
        _mockCharacterService.Verify(
            s => s.GetAllCharactersByPlayerAsync(It.IsAny<int>()),
            Times.Once);
    }

    [Fact]
    public async Task SelectCharacterAsync_WithValidCharacter_NavigatesToDetail()
    {
        // Arrange
        var character = CreateTestCharacter(5, "Gandalf");

        // Act
        await _viewModel.SelectCharacterAsync(character);

        // Assert
        _viewModel.SelectedCharacter.Should().Be(character);
        _mockNavigationService.Verify(
            n => n.NavigateToAsync(
                "character-detail",
                It.Is<Dictionary<string, object>>(d => d.ContainsKey("CharacterId") && (int)d["CharacterId"] == 5)),
            Times.Once);
    }

    [Fact]
    public async Task SelectCharacterAsync_WithNullCharacter_DoesNotNavigate()
    {
        // Act
        await _viewModel.SelectCharacterAsync(null);

        // Assert
        _viewModel.SelectedCharacter.Should().BeNull();
        _mockNavigationService.Verify(
            n => n.NavigateToAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateCharacterAsync_NavigatesToCreatePage()
    {
        // Act
        await _viewModel.CreateCharacterAsync();

        // Assert
        _mockNavigationService.Verify(
            n => n.NavigateToAsync("character-create", null),
            Times.Once);
    }

    [Fact]
    public async Task RefreshCharactersAsync_SetsRefreshingFlag()
    {
        // Arrange
        _mockCharacterService
            .Setup(s => s.GetAllCharactersByPlayerAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Character>());

        // Act
        await _viewModel.RefreshCharactersAsync();

        // Assert
        _viewModel.IsRefreshing.Should().BeFalse();
        _mockCharacterService.Verify(
            s => s.GetAllCharactersByPlayerAsync(It.IsAny<int>()),
            Times.Once);
    }

    [Fact]
    public async Task RefreshCharactersAsync_WhenAlreadyRefreshing_DoesNotStartNewRefresh()
    {
        // Arrange
        _mockCharacterService
            .Setup(s => s.GetAllCharactersByPlayerAsync(It.IsAny<int>()))
            .Returns(async () =>
            {
                await Task.Delay(100);
                return new List<Character>();
            });

        // Act
        var task1 = _viewModel.RefreshCharactersAsync();
        var task2 = _viewModel.RefreshCharactersAsync();

        await Task.WhenAll(task1, task2);

        // Assert
        _mockCharacterService.Verify(
            s => s.GetAllCharactersByPlayerAsync(It.IsAny<int>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteCharacterAsync_WithValidCharacter_RemovesCharacter()
    {
        // Arrange
        var character = CreateTestCharacter(1, "Gandalf");
        _viewModel.Characters.Add(character);

        _mockCharacterService
            .Setup(s => s.DeleteCharacterAsync(It.IsAny<int>()))
            .ReturnsAsync(true);

        // Act
        await _viewModel.DeleteCharacterAsync(character);

        // Assert
        _viewModel.Characters.Should().BeEmpty();
        _mockCharacterService.Verify(s => s.DeleteCharacterAsync(1), Times.Once);
        _mockNotificationService.Verify(
            n => n.ShowNotificationAsync("Character Deleted", It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteCharacterAsync_WithNull_DoesNothing()
    {
        // Act
        await _viewModel.DeleteCharacterAsync(null);

        // Assert
        _mockCharacterService.Verify(s => s.DeleteCharacterAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void SearchText_CanBeSetAndRetrieved()
    {
        // Act
        _viewModel.SearchText = "Gandalf";

        // Assert
        _viewModel.SearchText.Should().Be("Gandalf");
    }

    [Fact]
    public void Characters_InitiallyEmpty()
    {
        // Assert
        _viewModel.Characters.Should().BeEmpty();
    }

    [Fact]
    public void ErrorMessage_InitiallyNull()
    {
        // Assert
        _viewModel.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void IsLoading_InitiallyFalse()
    {
        // Assert
        _viewModel.IsLoading.Should().BeFalse();
    }
}
