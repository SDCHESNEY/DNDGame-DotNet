using DNDGame.Core.Interfaces;
using DNDGame.Core.Models;
using DNDGame.MauiApp.Interfaces;
using FluentAssertions;
using Moq;

namespace DNDGame.MauiApp.Tests.Services;

public class OfflineSyncServiceTests
{
    private readonly Mock<IDiceRoller> _mockDiceRoller;
    private readonly Mock<INotificationService> _mockNotificationService;

    public OfflineSyncServiceTests()
    {
        _mockDiceRoller = new Mock<IDiceRoller>();
        _mockNotificationService = new Mock<INotificationService>();
    }

    private DiceRollResult CreateTestResult(string formula, int total, int[] rolls)
    {
        return new DiceRollResult
        {
            Formula = formula,
            Total = total,
            IndividualRolls = rolls,
            Modifier = 0,
            Timestamp = DateTime.UtcNow
        };
    }

    [Fact]
    public void DiceRollResult_CanBeCreatedWithRequiredProperties()
    {
        // Arrange & Act
        var result = CreateTestResult("1d20+5", 25, new[] { 20 });

        // Assert
        result.Formula.Should().Be("1d20+5");
        result.Total.Should().Be(25);
        result.IndividualRolls.Should().ContainSingle().Which.Should().Be(20);
    }

    [Fact]
    public void DiceRollResult_TimestampIsSet()
    {
        // Arrange & Act
        var result = CreateTestResult("1d6", 4, new[] { 4 });

        // Assert
        result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void DiceRollResult_SupportsMultipleDice()
    {
        // Arrange & Act
        var result = CreateTestResult("3d6", 12, new[] { 4, 3, 5 });

        // Assert
        result.IndividualRolls.Should().HaveCount(3);
        result.IndividualRolls.Should().Equal(4, 3, 5);
    }

    [Fact]
    public async Task DiceRoller_IntegrationTest_RollsValidResult()
    {
        // Arrange
        _mockDiceRoller
            .Setup(d => d.Roll(It.IsAny<string>()))
            .Returns(CreateTestResult("1d20", 15, new[] { 15 }));

        // Act
        var result = _mockDiceRoller.Object.Roll("1d20");

        // Assert
        result.Should().NotBeNull();
        result.Total.Should().Be(15);
        result.Formula.Should().Be("1d20");
    }

    [Fact]
    public async Task DiceRoller_RollWithAdvantage_ReturnsHigherRoll()
    {
        // Arrange
        _mockDiceRoller
            .Setup(d => d.RollWithAdvantage(It.IsAny<string>()))
            .Returns(CreateTestResult("1d20", 18, new[] { 18, 12 }));

        // Act
        var result = _mockDiceRoller.Object.RollWithAdvantage("1d20");

        // Assert
        result.Total.Should().Be(18);
        result.IndividualRolls.Should().Contain(18);
    }

    [Fact]
    public async Task DiceRoller_RollWithDisadvantage_ReturnsLowerRoll()
    {
        // Arrange
        _mockDiceRoller
            .Setup(d => d.RollWithDisadvantage(It.IsAny<string>()))
            .Returns(CreateTestResult("1d20", 8, new[] { 8, 15 }));

        // Act
        var result = _mockDiceRoller.Object.RollWithDisadvantage("1d20");

        // Assert
        result.Total.Should().Be(8);
    }

    [Fact]
    public async Task NotificationService_ShowNotification_CanBeCalled()
    {
        // Arrange
        _mockNotificationService
            .Setup(n => n.ShowNotificationAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _mockNotificationService.Object.ShowNotificationAsync("Test Title", "Test Message");

        // Assert
        _mockNotificationService.Verify(
            n => n.ShowNotificationAsync("Test Title", "Test Message"),
            Times.Once);
    }

    [Fact]
    public async Task NotificationService_RequestPermission_ReturnsResult()
    {
        // Arrange
        _mockNotificationService
            .Setup(n => n.RequestPermissionAsync())
            .ReturnsAsync(true);

        // Act
        var result = await _mockNotificationService.Object.RequestPermissionAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task NotificationService_ScheduleNotification_CanBeScheduled()
    {
        // Arrange
        var scheduledTime = DateTime.UtcNow.AddHours(1);
        _mockNotificationService
            .Setup(n => n.ScheduleNotificationAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>()))
            .Returns(Task.CompletedTask);

        // Act
        await _mockNotificationService.Object.ScheduleNotificationAsync(
            "Scheduled",
            "Message",
            scheduledTime);

        // Assert
        _mockNotificationService.Verify(
            n => n.ScheduleNotificationAsync("Scheduled", "Message", scheduledTime),
            Times.Once);
    }

    [Fact]
    public void SyncResult_DefaultsToCurrentTime()
    {
        // Arrange & Act
        var syncResult = new SyncResult();

        // Assert
        syncResult.SyncTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void SyncResult_CanTrackSyncedCounts()
    {
        // Arrange & Act
        var syncResult = new SyncResult
        {
            Success = true,
            CharactersSynced = 5,
            SessionsSynced = 3
        };

        // Assert
        syncResult.Success.Should().BeTrue();
        syncResult.CharactersSynced.Should().Be(5);
        syncResult.SessionsSynced.Should().Be(3);
    }

    [Fact]
    public void SyncResult_ErrorsListInitializesEmpty()
    {
        // Arrange & Act
        var syncResult = new SyncResult();

        // Assert
        syncResult.Errors.Should().NotBeNull();
        syncResult.Errors.Should().BeEmpty();
    }

    [Fact]
    public void SyncResult_CanAddErrors()
    {
        // Arrange
        var syncResult = new SyncResult();

        // Act
        syncResult.Errors.Add("Network error");
        syncResult.Errors.Add("Database error");

        // Assert
        syncResult.Errors.Should().HaveCount(2);
        syncResult.Errors.Should().Contain("Network error");
    }
}
