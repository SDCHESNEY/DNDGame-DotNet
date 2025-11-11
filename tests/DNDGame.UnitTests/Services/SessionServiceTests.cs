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
    private readonly SessionService _sut;

    public SessionServiceTests()
    {
        _mockRepository = new Mock<ISessionRepository>();
        _sut = new SessionService(_mockRepository.Object);
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
        var dto = result as SessionDto;
        dto.Should().NotBeNull();
        dto!.Id.Should().Be(1);
        dto.Title.Should().Be("Test Campaign");
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
        var dto = result as SessionDto;
        dto.Should().NotBeNull();
        dto!.State.Should().Be(SessionState.InProgress);
        
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

    private static Session CreateTestSession(int id, string title)
    {
        return new Session
        {
            Id = id,
            Title = title,
            Mode = SessionMode.Multiplayer,
            State = SessionState.Created
        };
    }
}
