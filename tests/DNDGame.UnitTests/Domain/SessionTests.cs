using DNDGame.Core.Entities;
using DNDGame.Core.Enums;
using FluentAssertions;
using Xunit;

namespace DNDGame.UnitTests.Domain;

public class SessionTests
{
    [Fact]
    public void Session_InitializesWithDefaultState()
    {
        // Arrange & Act
        var session = new Session
        {
            Title = "Test Campaign"
        };

        // Assert
        session.State.Should().Be(SessionState.Created);
        session.Messages.Should().BeEmpty();
        session.DiceRolls.Should().BeEmpty();
        session.WorldFlags.Should().Be("{}");
        session.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Session_AcceptsAllProperties()
    {
        // Arrange
        var session = new Session
        {
            Title = "Epic Adventure",
            Mode = SessionMode.Multiplayer,
            State = SessionState.InProgress,
            CurrentScene = "The Dark Forest",
            CurrentTurnCharacterId = 5
        };

        // Assert
        session.Title.Should().Be("Epic Adventure");
        session.Mode.Should().Be(SessionMode.Multiplayer);
        session.State.Should().Be(SessionState.InProgress);
        session.CurrentScene.Should().Be("The Dark Forest");
        session.CurrentTurnCharacterId.Should().Be(5);
    }
}
