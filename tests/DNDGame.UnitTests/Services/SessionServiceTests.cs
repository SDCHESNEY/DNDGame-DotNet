using DNDGame.Application.DTOs;
using DNDGame.Application.Services;
using DNDGame.Core.Entities;
using DNDGame.Core.Enums;
using DNDGame.Core.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace DNDGame.UnitTests.Services;

public class SessionServiceTests
{
    private readonly Mock<ISessionRepository> _mockRepository;
    private readonly Mock<ICharacterRepository> _mockCharacterRepository;
    private readonly SessionService _sut;

    public SessionServiceTests()
    {
        _mockRepository = new Mock<ISessionRepository>();
        _mockCharacterRepository = new Mock<ICharacterRepository>();
        _sut = new SessionService(_mockRepository.Object, _mockCharacterRepository.Object);
    }

    [Fact]
    public async Task GetSessionAsync_WithValidId_ReturnsSessionDto()
    {
        // Arrange
        var session = CreateTestSession(1, "Test Campaign");
        _mockRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(session);

        // Act
        var result = await _sut.GetSessionAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Title.Should().Be("Test Campaign");
    }

    [Fact]
    public async Task GetSessionAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Session?)null);

        // Act
        var result = await _sut.GetSessionAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateSessionAsync_WithValidRequest_CreatesSession()
    {
        // Arrange
        var request = new CreateSessionRequest(
            Title: "Epic Adventure",
            Mode: SessionMode.Multiplayer
        );

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Session>(), It.IsAny<CancellationToken>()))
            .Callback<Session, CancellationToken>((s, _) => s.Id = 1)
            .ReturnsAsync((Session s, CancellationToken _) => s);

        // Act
        var result = await _sut.CreateSessionAsync(request);

        // Assert
        result.Should().NotBeNull();
        var dto = result as SessionDto;
        dto.Should().NotBeNull();
        dto!.Title.Should().Be("Epic Adventure");
        dto.Mode.Should().Be(SessionMode.Multiplayer);
        dto.State.Should().Be(SessionState.Created);
        
        _mockRepository.Verify(r => r.AddAsync(It.Is<Session>(s => 
            s.Title == "Epic Adventure" && 
            s.Mode == SessionMode.Multiplayer &&
            s.State == SessionState.Created), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateSessionStateAsync_WithValidId_UpdatesState()
    {
        // Arrange
        var session = CreateTestSession(1, "Test Session");
        _mockRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Session>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateSessionStateAsync(1, SessionState.InProgress);

        // Assert
        result.Should().NotBeNull();
        result!.State.Should().Be(SessionState.InProgress);
        
        _mockRepository.Verify(r => r.UpdateAsync(It.Is<Session>(s => 
            s.State == SessionState.InProgress), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateSessionStateAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Session?)null);

        // Act
        var result = await _sut.UpdateSessionStateAsync(999, SessionState.InProgress);

        // Assert
        result.Should().BeNull();
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Session>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteSessionAsync_WithValidId_DeletesSession()
    {
        // Arrange
        var session = CreateTestSession(1, "Test Session");
        _mockRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        _mockRepository.Setup(r => r.DeleteAsync(1, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.DeleteSessionAsync(1);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteSessionAsync_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Session?)null);

        // Act
        var result = await _sut.DeleteSessionAsync(999);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // Real-Time Feature Tests

    [Fact]
    public async Task JoinSessionAsync_WithValidSessionAndCharacter_ReturnsTrue()
    {
        // Arrange
        var session = CreateTestSession(1, "Test");
        var character = CreateTestCharacter(1, "Test Character");
        
        _mockRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        _mockCharacterRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(character);

        // Act
        var result = await _sut.JoinSessionAsync(1, 1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task JoinSessionAsync_WithInvalidSession_ReturnsFalse()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Session?)null);
        _mockCharacterRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(CreateTestCharacter(1, "Test"));

        // Act
        var result = await _sut.JoinSessionAsync(999, 1);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task JoinSessionAsync_WithInvalidCharacter_ReturnsFalse()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(CreateTestSession(1, "Test"));
        _mockCharacterRepository.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Character?)null);

        // Act
        var result = await _sut.JoinSessionAsync(1, 999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task LeaveSessionAsync_WithValidSession_ReturnsTrue()
    {
        // Arrange
        var session = CreateTestSession(1, "Test");
        _mockRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(session);

        // Act
        var result = await _sut.LeaveSessionAsync(1, 1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task LeaveSessionAsync_WithInvalidSession_ReturnsFalse()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Session?)null);

        // Act
        var result = await _sut.LeaveSessionAsync(999, 1);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SaveMessageAsync_WithValidSession_SavesAndReturnsMessage()
    {
        // Arrange
        var session = CreateTestSession(1, "Test");
        session.Messages = new List<Message>();
        
        _mockRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Session>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.SaveMessageAsync(1, "Test message", MessageRole.Player);

        // Assert
        result.Should().NotBeNull();
        result.Content.Should().Be("Test message");
        result.Role.Should().Be(MessageRole.Player);
        result.SessionId.Should().Be(1);
        session.Messages.Should().Contain(result);
        _mockRepository.Verify(r => r.UpdateAsync(session, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SaveMessageAsync_WithInvalidSession_ThrowsException()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Session?)null);

        // Act
        var act = async () => await _sut.SaveMessageAsync(999, "Test", MessageRole.Player);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Session 999 not found");
    }

    [Fact]
    public async Task SaveDiceRollAsync_WithValidSession_SavesAndReturnsDiceRoll()
    {
        // Arrange
        var session = CreateTestSession(1, "Test");
        session.DiceRolls = new List<DiceRoll>();
        
        var rollResult = new Core.Models.DiceRollResult
        {
            Formula = "1d20",
            Total = 15,
            IndividualRolls = [15],
            Modifier = 0
        };
        
        _mockRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Session>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.SaveDiceRollAsync(1, "1d20", rollResult);

        // Assert
        result.Should().NotBeNull();
        result.Formula.Should().Be("1d20");
        result.Total.Should().Be(15);
        result.SessionId.Should().Be(1);
        session.DiceRolls.Should().Contain(result);
        _mockRepository.Verify(r => r.UpdateAsync(session, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SaveDiceRollAsync_WithInvalidSession_ThrowsException()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Session?)null);
        var rollResult = new Core.Models.DiceRollResult
        {
            Formula = "1d20",
            Total = 15,
            IndividualRolls = [15],
            Modifier = 0
        };

        // Act
        var act = async () => await _sut.SaveDiceRollAsync(999, "1d20", rollResult);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Session 999 not found");
    }

    private static Session CreateTestSession(int id, string title)
    {
        return new Session
        {
            Id = id,
            Title = title,
            Mode = SessionMode.Multiplayer,
            State = SessionState.Created,
            Messages = new List<Message>(),
            DiceRolls = new List<DiceRoll>()
        };
    }

    private static Character CreateTestCharacter(int id, string name)
    {
        return new Character
        {
            Id = id,
            Name = name,
            Class = CharacterClass.Fighter,
            Level = 1,
            AbilityScores = new Core.ValueObjects.AbilityScores(10, 10, 10, 10, 10, 10)
        };
    }
}
