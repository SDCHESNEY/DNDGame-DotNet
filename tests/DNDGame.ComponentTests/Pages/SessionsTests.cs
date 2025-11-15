using Bunit;
using DNDGame.Core.Entities;
using DNDGame.Core.Enums;
using DNDGame.Core.Interfaces;
using DNDGame.Web.Components.Pages;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using FluentAssertions;

namespace DNDGame.ComponentTests.Pages;

public class SessionsTests : TestContext
{
    private readonly Mock<ISessionService> _mockSessionService;

    public SessionsTests()
    {
        _mockSessionService = new Mock<ISessionService>();
    }

    [Fact]
    public void Sessions_RendersLoadingState_Initially()
    {
        // Arrange
        Services.Add(new ServiceDescriptor(typeof(ISessionService), _mockSessionService.Object));
        
        _mockSessionService
            .Setup(s => s.GetAllSessionsAsync())
            .ReturnsAsync(new List<Session>());

        // Act
        var cut = RenderComponent<Sessions>();

        // Assert
        cut.WaitForState(() => !cut.Markup.Contains("spinner"), timeout: TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Sessions_DisplaysSessionCards_WhenDataLoaded()
    {
        // Arrange
        Services.Add(new ServiceDescriptor(typeof(ISessionService), _mockSessionService.Object));
        
        var sessions = new List<Session>
        {
            new()
            {
                Id = 1,
                Title = "Forest Adventure",
                State = SessionState.InProgress,
                Mode = SessionMode.Solo,
                CurrentScene = "Forest Entrance",
                CreatedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow
            }
        };

        _mockSessionService
            .Setup(s => s.GetAllSessionsAsync())
            .ReturnsAsync(sessions);

        // Act
        var cut = RenderComponent<Sessions>();
        cut.WaitForState(() => cut.Markup.Contains("Forest Entrance"), timeout: TimeSpan.FromSeconds(5));

        // Assert
        cut.Markup.Should().Contain("Forest Entrance");
        cut.Markup.Should().Contain("InProgress");
        cut.Markup.Should().Contain("Solo");
    }

    [Fact]
    public void Sessions_DisplaysEmptyMessage_WhenNoSessions()
    {
        // Arrange
        Services.Add(new ServiceDescriptor(typeof(ISessionService), _mockSessionService.Object));
        
        _mockSessionService
            .Setup(s => s.GetAllSessionsAsync())
            .ReturnsAsync(new List<Session>());

        // Act
        var cut = RenderComponent<Sessions>();
        cut.WaitForState(() => cut.Markup.Contains("No active sessions"), timeout: TimeSpan.FromSeconds(5));

        // Assert
        cut.Markup.Should().Contain("No active sessions");
    }

    [Fact]
    public void Sessions_HasCreateButton()
    {
        // Arrange
        Services.Add(new ServiceDescriptor(typeof(ISessionService), _mockSessionService.Object));
        
        _mockSessionService
            .Setup(s => s.GetAllSessionsAsync())
            .ReturnsAsync(new List<Session>());

        // Act
        var cut = RenderComponent<Sessions>();
        cut.WaitForState(() => !cut.Markup.Contains("spinner"), timeout: TimeSpan.FromSeconds(5));

        // Assert
        var button = cut.Find("button:contains('Create New Session')");
        button.Should().NotBeNull();
    }

    [Fact]
    public void Sessions_ShowsStateBadges()
    {
        // Arrange
        Services.Add(new ServiceDescriptor(typeof(ISessionService), _mockSessionService.Object));
        
        var sessions = new List<Session>
        {
            new() { Id = 1, Title = "Start Adventure", State = SessionState.Created, Mode = SessionMode.Solo, CurrentScene = "Start", CreatedAt = DateTime.UtcNow },
            new() { Id = 2, Title = "Combat Session", State = SessionState.InProgress, Mode = SessionMode.Multiplayer, CurrentScene = "Combat", CreatedAt = DateTime.UtcNow }
        };

        _mockSessionService
            .Setup(s => s.GetAllSessionsAsync())
            .ReturnsAsync(sessions);

        // Act
        var cut = RenderComponent<Sessions>();
        cut.WaitForState(() => cut.Markup.Contains("Created"), timeout: TimeSpan.FromSeconds(5));

        // Assert
        cut.Markup.Should().Contain("Created");
        cut.Markup.Should().Contain("InProgress");
    }

    [Fact]
    public void Sessions_ShowsModeIcons()
    {
        // Arrange
        Services.Add(new ServiceDescriptor(typeof(ISessionService), _mockSessionService.Object));
        
        var sessions = new List<Session>
        {
            new() { Id = 1, Title = "Dungeon Delve", State = SessionState.InProgress, Mode = SessionMode.Solo, CurrentScene = "Dungeon", CreatedAt = DateTime.UtcNow },
            new() { Id = 2, Title = "Tavern Tales", State = SessionState.InProgress, Mode = SessionMode.Multiplayer, CurrentScene = "Tavern", CreatedAt = DateTime.UtcNow }
        };

        _mockSessionService
            .Setup(s => s.GetAllSessionsAsync())
            .ReturnsAsync(sessions);

        // Act
        var cut = RenderComponent<Sessions>();
        cut.WaitForState(() => cut.Markup.Contains("Dungeon"), timeout: TimeSpan.FromSeconds(5));

        // Assert
        cut.Markup.Should().Contain("bi-person"); // Solo icon
        cut.Markup.Should().Contain("bi-people"); // Multiplayer icon
    }

    [Fact]
    public void Sessions_DisplaysLastActivity()
    {
        // Arrange
        Services.Add(new ServiceDescriptor(typeof(ISessionService), _mockSessionService.Object));
        
        var sessions = new List<Session>
        {
            new()
            {
                Id = 1,
                Title = "Cave Exploration",
                State = SessionState.InProgress,
                Mode = SessionMode.Solo,
                CurrentScene = "Cave",
                CreatedAt = DateTime.UtcNow.AddHours(-2),
                LastActivityAt = DateTime.UtcNow.AddMinutes(-5)
            }
        };

        _mockSessionService
            .Setup(s => s.GetAllSessionsAsync())
            .ReturnsAsync(sessions);

        // Act
        var cut = RenderComponent<Sessions>();
        cut.WaitForState(() => cut.Markup.Contains("Cave"), timeout: TimeSpan.FromSeconds(5));

        // Assert
        cut.Markup.Should().Contain("Last Activity");
    }
}
