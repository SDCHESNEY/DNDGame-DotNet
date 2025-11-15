namespace DNDGame.UnitTests.Services;

using DNDGame.Application.Services;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class PresenceServiceTests
{
    private readonly IMemoryCache _cache;
    private readonly Mock<ILogger<PresenceService>> _mockLogger;
    private readonly PresenceService _sut;

    public PresenceServiceTests()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
        _mockLogger = new Mock<ILogger<PresenceService>>();
        _sut = new PresenceService(_cache, _mockLogger.Object);
    }

    [Fact]
    public async Task TrackConnectionAsync_WithValidData_StoresConnection()
    {
        // Act
        await _sut.TrackConnectionAsync(1, 100, "conn-123");

        // Assert
        var sessionId = await _sut.GetSessionIdByConnectionAsync("conn-123");
        sessionId.Should().Be(1);

        var isOnline = await _sut.IsPlayerOnlineAsync(100);
        isOnline.Should().BeTrue();
    }

    [Fact]
    public async Task RemoveConnectionAsync_WithExistingConnection_RemovesConnection()
    {
        // Arrange
        await _sut.TrackConnectionAsync(1, 100, "conn-123");

        // Act
        await _sut.RemoveConnectionAsync("conn-123");

        // Assert
        var sessionId = await _sut.GetSessionIdByConnectionAsync("conn-123");
        sessionId.Should().BeNull();
    }

    [Fact]
    public async Task GetActivePlayersAsync_WithMultiplePlayers_ReturnsAllPlayers()
    {
        // Arrange
        await _sut.TrackConnectionAsync(1, 100, "conn-1");
        await _sut.TrackConnectionAsync(1, 101, "conn-2");
        await _sut.TrackConnectionAsync(1, 102, "conn-3");

        // Act
        var players = await _sut.GetActivePlayersAsync(1);

        // Assert
        players.Should().HaveCount(3);
        players.Should().Contain(p => p.PlayerId == 100);
        players.Should().Contain(p => p.PlayerId == 101);
        players.Should().Contain(p => p.PlayerId == 102);
        players.Should().OnlyContain(p => p.IsOnline);
    }

    [Fact]
    public async Task GetActivePlayersAsync_WithNoPlayers_ReturnsEmptyList()
    {
        // Act
        var players = await _sut.GetActivePlayersAsync(999);

        // Assert
        players.Should().BeEmpty();
    }

    [Fact]
    public async Task IsPlayerOnlineAsync_WithOnlinePlayer_ReturnsTrue()
    {
        // Arrange
        await _sut.TrackConnectionAsync(1, 100, "conn-123");

        // Act
        var isOnline = await _sut.IsPlayerOnlineAsync(100);

        // Assert
        isOnline.Should().BeTrue();
    }

    [Fact]
    public async Task IsPlayerOnlineAsync_WithOfflinePlayer_ReturnsFalse()
    {
        // Act
        var isOnline = await _sut.IsPlayerOnlineAsync(999);

        // Assert
        isOnline.Should().BeFalse();
    }

    [Fact]
    public async Task GetSessionIdByConnectionAsync_WithExistingConnection_ReturnsSessionId()
    {
        // Arrange
        await _sut.TrackConnectionAsync(1, 100, "conn-123");

        // Act
        var sessionId = await _sut.GetSessionIdByConnectionAsync("conn-123");

        // Assert
        sessionId.Should().Be(1);
    }

    [Fact]
    public async Task GetSessionIdByConnectionAsync_WithNonExistentConnection_ReturnsNull()
    {
        // Act
        var sessionId = await _sut.GetSessionIdByConnectionAsync("conn-999");

        // Assert
        sessionId.Should().BeNull();
    }

    [Fact]
    public async Task TrackConnectionAsync_WithMultipleConnectionsForSamePlayer_MaintainsOnlineStatus()
    {
        // Arrange & Act
        await _sut.TrackConnectionAsync(1, 100, "conn-1");
        await _sut.TrackConnectionAsync(1, 100, "conn-2");

        // Assert
        var isOnline = await _sut.IsPlayerOnlineAsync(100);
        isOnline.Should().BeTrue();
    }

    [Fact]
    public async Task RemoveConnectionAsync_WithPlayerHavingMultipleConnections_RemovesPlayer()
    {
        // Arrange
        await _sut.TrackConnectionAsync(1, 100, "conn-1");
        await _sut.TrackConnectionAsync(1, 100, "conn-2");

        // Act - Remove first connection (simplified implementation removes player from session)
        await _sut.RemoveConnectionAsync("conn-1");

        // Assert - In a simplified implementation, player is removed when any connection closes
        // A production implementation would track multiple connections per player
        var sessionId = await _sut.GetSessionIdByConnectionAsync("conn-1");
        sessionId.Should().BeNull();
    }

    [Fact]
    public async Task TrackConnectionAsync_WithDifferentSessions_IsolatesPlayers()
    {
        // Arrange
        await _sut.TrackConnectionAsync(1, 100, "conn-1");
        await _sut.TrackConnectionAsync(2, 101, "conn-2");

        // Act
        var session1Players = await _sut.GetActivePlayersAsync(1);
        var session2Players = await _sut.GetActivePlayersAsync(2);

        // Assert
        session1Players.Should().HaveCount(1);
        session1Players.Should().Contain(p => p.PlayerId == 100);
        
        session2Players.Should().HaveCount(1);
        session2Players.Should().Contain(p => p.PlayerId == 101);
    }

    [Fact]
    public async Task GetActivePlayersAsync_ReturnsPlayersWithCorrectPresenceData()
    {
        // Arrange
        await _sut.TrackConnectionAsync(1, 100, "conn-1");

        // Act
        var players = await _sut.GetActivePlayersAsync(1);

        // Assert
        var player = players.Should().ContainSingle().Subject;
        player.PlayerId.Should().Be(100);
        player.IsOnline.Should().BeTrue();
        player.LastSeen.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
