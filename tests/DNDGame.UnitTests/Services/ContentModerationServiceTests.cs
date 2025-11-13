using DNDGame.Application.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace DNDGame.UnitTests.Services;

public class ContentModerationServiceTests
{
    private readonly Mock<ILogger<ContentModerationService>> _mockLogger;
    private readonly ContentModerationService _sut;

    public ContentModerationServiceTests()
    {
        _mockLogger = new Mock<ILogger<ContentModerationService>>();
        
        var settings = new ContentModerationSettings
        {
            Enabled = true,
            BlockNsfw = true,
            BlockHarassment = true,
            MaxInputLength = 5000
        };

        var mockOptions = new Mock<IOptions<ContentModerationSettings>>();
        mockOptions.Setup(o => o.Value).Returns(settings);

        _sut = new ContentModerationService(mockOptions.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task ModerateInputAsync_WithSafeContent_ShouldReturnSafe()
    {
        // Arrange
        var input = "I would like to explore the ancient ruins.";

        // Act
        var result = await _sut.ModerateInputAsync(input);

        // Assert
        result.IsSafe.Should().BeTrue();
        result.Violations.Should().BeEmpty();
    }

    [Fact]
    public async Task ModerateInputAsync_WithNsfwContent_ShouldBlockWhenEnabled()
    {
        // Arrange
        var input = "This contains explicit nsfw content here.";

        // Act
        var result = await _sut.ModerateInputAsync(input);

        // Assert
        result.IsSafe.Should().BeFalse();
        result.Violations.Should().Contain("NSFW content detected");
    }

    [Fact]
    public async Task ModerateInputAsync_WithHarassmentContent_ShouldBlock()
    {
        // Arrange
        var input = "This has racist and hate content in it.";

        // Act
        var result = await _sut.ModerateInputAsync(input);

        // Assert
        result.IsSafe.Should().BeFalse();
        result.Violations.Should().Contain(v => v.Contains("Harassment") || v.Contains("detected"));
    }    [Fact]
    public async Task ModerateInputAsync_WithTooLongInput_ShouldBlock()
    {
        // Arrange
        var input = new string('a', 5001);

        // Act
        var result = await _sut.ModerateInputAsync(input);

        // Assert
        result.IsSafe.Should().BeFalse();
        result.Violations.Should().Contain(v => v.Contains("too long"));
    }

    [Fact]
    public async Task ModerateInputAsync_WithModerationDisabled_ShouldAllowAll()
    {
        // Arrange
        var settings = new ContentModerationSettings { Enabled = false };
        var mockOptions = new Mock<IOptions<ContentModerationSettings>>();
        mockOptions.Setup(o => o.Value).Returns(settings);
        var service = new ContentModerationService(mockOptions.Object, _mockLogger.Object);

        var input = "nsfw explicit content";

        // Act
        var result = await service.ModerateInputAsync(input);

        // Assert
        result.IsSafe.Should().BeTrue();
    }

    [Fact]
    public async Task ModerateOutputAsync_WithSafeContent_ShouldReturnSafe()
    {
        // Arrange
        var output = "The dragon spreads its wings and roars.";

        // Act
        var result = await _sut.ModerateOutputAsync(output);

        // Assert
        result.IsSafe.Should().BeTrue();
        result.WasSanitized.Should().BeFalse();
    }

    [Fact]
    public async Task ModerateOutputAsync_WithBlockedWords_ShouldSanitize()
    {
        // Arrange
        var output = "The dragon has explicit sexual content in it.";

        // Act
        var result = await _sut.ModerateOutputAsync(output);

        // Assert
        result.WasSanitized.Should().BeTrue();
        result.IsSafe.Should().BeTrue(); // Sanitized content is considered safe
        result.SanitizedContent.Should().Contain("[REDACTED]");
        result.SanitizedContent.Should().NotContain("explicit");
        result.SanitizedContent.Should().NotContain("sexual");
    }

    [Fact]
    public async Task SanitizeContentAsync_WithBlockedWords_ShouldReplaceWithRedacted()
    {
        // Arrange
        var content = "This has explicit content in it.";

        // Act
        var result = await _sut.SanitizeContentAsync(content);

        // Assert
        result.Should().Contain("[REDACTED]");
        result.Should().NotContain("explicit");
    }

    [Fact]
    public async Task SanitizeContentAsync_WithMultipleBlockedWords_ShouldReplaceAll()
    {
        // Arrange
        var content = "Contains nsfw and explicit words.";

        // Act
        var result = await _sut.SanitizeContentAsync(content);

        // Assert
        result.Split("[REDACTED]").Length.Should().BeGreaterThan(1);
    }

    [Fact]
    public async Task SanitizeContentAsync_WithCaseInsensitiveMatch_ShouldStillReplace()
    {
        // Arrange
        var content = "This has EXPLICIT and Explicit content.";

        // Act
        var result = await _sut.SanitizeContentAsync(content);

        // Assert
        result.Should().Contain("[REDACTED]");
        result.Should().NotContainAny("EXPLICIT", "Explicit", "explicit");
    }

    [Fact]
    public async Task ModerateInputAsync_WithNsfwDisabled_ShouldAllowNsfwContent()
    {
        // Arrange
        var settings = new ContentModerationSettings
        {
            Enabled = true,
            BlockNsfw = false,
            BlockHarassment = true,
            MaxInputLength = 5000
        };
        var mockOptions = new Mock<IOptions<ContentModerationSettings>>();
        mockOptions.Setup(o => o.Value).Returns(settings);
        var service = new ContentModerationService(mockOptions.Object, _mockLogger.Object);

        var input = "This contains nsfw content.";

        // Act
        var result = await service.ModerateInputAsync(input);

        // Assert
        result.IsSafe.Should().BeTrue();
    }
}
