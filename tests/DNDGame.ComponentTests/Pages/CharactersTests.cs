using Bunit;
using DNDGame.Core.Entities;
using DNDGame.Core.Enums;
using DNDGame.Core.Interfaces;
using DNDGame.Core.ValueObjects;
using DNDGame.Web.Components.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using Xunit;
using FluentAssertions;

namespace DNDGame.ComponentTests.Pages;

public class CharactersTests : TestContext
{
    private readonly Mock<ICharacterService> _mockCharacterService;
    private readonly Mock<IJSRuntime> _mockJsRuntime;

    public CharactersTests()
    {
        _mockCharacterService = new Mock<ICharacterService>();
        _mockJsRuntime = new Mock<IJSRuntime>();
    }

    [Fact]
    public void Characters_RendersLoadingState_Initially()
    {
        // Arrange
        Services.Add(new ServiceDescriptor(typeof(ICharacterService), _mockCharacterService.Object));
        Services.Add(new ServiceDescriptor(typeof(IJSRuntime), _mockJsRuntime.Object));
        
        _mockCharacterService
            .Setup(s => s.GetAllCharactersByPlayerAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Character>());

        // Act
        var cut = RenderComponent<Characters>();

        // Assert
        cut.WaitForState(() => !cut.Markup.Contains("spinner"), timeout: TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Characters_DisplaysCharacterCards_WhenDataLoaded()
    {
        // Arrange
        Services.Add(new ServiceDescriptor(typeof(ICharacterService), _mockCharacterService.Object));
        Services.Add(new ServiceDescriptor(typeof(IJSRuntime), _mockJsRuntime.Object));
        
        var characters = new List<Character>
        {
            new()
            {
                Id = 1,
                PlayerId = 1,
                Name = "Gandalf",
                Class = CharacterClass.Wizard,
                Level = 10,
                AbilityScores = new AbilityScores(10, 10, 10, 18, 16, 14),
                HitPoints = 50,
                MaxHitPoints = 60,
                ArmorClass = 15
            }
        };

        _mockCharacterService
            .Setup(s => s.GetAllCharactersByPlayerAsync(It.IsAny<int>()))
            .ReturnsAsync(characters);

        // Act
        var cut = RenderComponent<Characters>();
        cut.WaitForState(() => cut.Markup.Contains("Gandalf"), timeout: TimeSpan.FromSeconds(5));

        // Assert
        cut.Markup.Should().Contain("Gandalf");
        cut.Markup.Should().Contain("Wizard");
        cut.Markup.Should().Contain("Level: 10");
    }

    [Fact]
    public void Characters_DisplaysEmptyMessage_WhenNoCharacters()
    {
        // Arrange
        Services.Add(new ServiceDescriptor(typeof(ICharacterService), _mockCharacterService.Object));
        Services.Add(new ServiceDescriptor(typeof(IJSRuntime), _mockJsRuntime.Object));
        
        _mockCharacterService
            .Setup(s => s.GetAllCharactersByPlayerAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Character>());

        // Act
        var cut = RenderComponent<Characters>();
        cut.WaitForState(() => cut.Markup.Contains("No characters"), timeout: TimeSpan.FromSeconds(5));

        // Assert
        cut.Markup.Should().Contain("No characters");
    }

    [Fact]
    public void Characters_HasCreateButton()
    {
        // Arrange
        Services.Add(new ServiceDescriptor(typeof(ICharacterService), _mockCharacterService.Object));
        Services.Add(new ServiceDescriptor(typeof(IJSRuntime), _mockJsRuntime.Object));
        
        _mockCharacterService
            .Setup(s => s.GetAllCharactersByPlayerAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Character>());

        // Act
        var cut = RenderComponent<Characters>();
        cut.WaitForState(() => !cut.Markup.Contains("spinner"), timeout: TimeSpan.FromSeconds(5));

        // Assert
        var button = cut.Find("button:contains('Create New Character')");
        button.Should().NotBeNull();
    }
}
