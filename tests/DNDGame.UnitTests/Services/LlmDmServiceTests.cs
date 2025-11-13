using DNDGame.Core.Entities;
using DNDGame.Core.Enums;
using DNDGame.Core.Interfaces;
using DNDGame.Core.Models;
using DNDGame.Core.ValueObjects;
using DNDGame.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DNDGame.UnitTests.Services;

public class LlmDmServiceTests
{
    private readonly Mock<ILlmProvider> _mockProvider;
    private readonly Mock<IPromptTemplateService> _mockTemplateService;
    private readonly Mock<IContentModerationService> _mockModerationService;
    private readonly Mock<ILogger<LlmDmService>> _mockLogger;
    private readonly LlmDmService _sut;

    public LlmDmServiceTests()
    {
        _mockProvider = new Mock<ILlmProvider>();
        _mockTemplateService = new Mock<IPromptTemplateService>();
        _mockModerationService = new Mock<IContentModerationService>();
        _mockLogger = new Mock<ILogger<LlmDmService>>();

        _sut = new LlmDmService(
            _mockProvider.Object,
            _mockTemplateService.Object,
            _mockModerationService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task GenerateResponseAsync_WithSafeInput_ShouldReturnDmResponse()
    {
        // Arrange
        var context = CreateTestContext();
        var playerAction = "I search the room";

        _mockModerationService
            .Setup(m => m.ModerateInputAsync(playerAction, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ModerationResult.Safe());

        _mockTemplateService
            .Setup(t => t.GetSystemPrompt(It.IsAny<SessionMode>()))
            .Returns("System prompt");

        _mockTemplateService
            .Setup(t => t.FormatContext(context))
            .Returns("Context");

        _mockProvider
            .Setup(p => p.CompleteAsync("System prompt", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(("You find a hidden door!", 50));

        _mockModerationService
            .Setup(m => m.ModerateOutputAsync("You find a hidden door!", It.IsAny<CancellationToken>()))
            .ReturnsAsync(ModerationResult.Safe());

        // Act
        var result = await _sut.GenerateResponseAsync(context, playerAction);

        // Assert
        result.Should().NotBeNull();
        result.Content.Should().Be("You find a hidden door!");
        result.TokensUsed.Should().Be(50);
        result.Role.Should().Be(MessageRole.DungeonMaster);
    }

    [Fact]
    public async Task GenerateResponseAsync_WithUnsafeInput_ShouldThrowException()
    {
        // Arrange
        var context = CreateTestContext();
        var playerAction = "inappropriate content";

        _mockModerationService
            .Setup(m => m.ModerateInputAsync(playerAction, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ModerationResult.Unsafe("Blocked content"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.GenerateResponseAsync(context, playerAction));
    }

    [Fact]
    public async Task GenerateResponseAsync_WithUnsafeOutput_ShouldSanitize()
    {
        // Arrange
        var context = CreateTestContext();
        var playerAction = "I open the chest";
        var unsafeOutput = "You find explicit items";
        var sanitizedOutput = "You find [REDACTED] items";

        _mockModerationService
            .Setup(m => m.ModerateInputAsync(playerAction, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ModerationResult.Safe());

        _mockTemplateService
            .Setup(t => t.GetSystemPrompt(It.IsAny<SessionMode>()))
            .Returns("System prompt");

        _mockTemplateService
            .Setup(t => t.FormatContext(context))
            .Returns("Context");

        _mockProvider
            .Setup(p => p.CompleteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((unsafeOutput, 40));

        _mockModerationService
            .Setup(m => m.ModerateOutputAsync(unsafeOutput, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ModerationResult.Sanitized(sanitizedOutput, "explicit"));

        // Act
        var result = await _sut.GenerateResponseAsync(context, playerAction);

        // Assert
        result.Content.Should().Be(sanitizedOutput);
    }

    [Fact]
    public async Task GenerateResponseAsync_InCombat_ShouldUseCombatPrompt()
    {
        // Arrange
        var context = CreateTestContext(inCombat: true);
        var playerAction = "I attack";

        _mockModerationService
            .Setup(m => m.ModerateInputAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ModerationResult.Safe());

        _mockTemplateService
            .Setup(t => t.GetSystemPrompt(It.IsAny<SessionMode>()))
            .Returns("System prompt");

        _mockTemplateService
            .Setup(t => t.GetCombatPrompt(context))
            .Returns("Combat prompt");

        _mockTemplateService
            .Setup(t => t.FormatContext(context))
            .Returns("Context");

        _mockProvider
            .Setup(p => p.CompleteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(("You hit the enemy!", 30));

        _mockModerationService
            .Setup(m => m.ModerateOutputAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ModerationResult.Safe());

        // Act
        await _sut.GenerateResponseAsync(context, playerAction);

        // Assert
        _mockTemplateService.Verify(t => t.GetCombatPrompt(context), Times.Once);
    }

    [Fact]
    public async Task GenerateResponseAsync_NotInCombat_ShouldUseExplorationPrompt()
    {
        // Arrange
        var context = CreateTestContext(inCombat: false);
        var playerAction = "I look around";

        _mockModerationService
            .Setup(m => m.ModerateInputAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ModerationResult.Safe());

        _mockTemplateService
            .Setup(t => t.GetSystemPrompt(It.IsAny<SessionMode>()))
            .Returns("System prompt");

        _mockTemplateService
            .Setup(t => t.GetExplorationPrompt(context))
            .Returns("Exploration prompt");

        _mockTemplateService
            .Setup(t => t.FormatContext(context))
            .Returns("Context");

        _mockProvider
            .Setup(p => p.CompleteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(("You see a vast landscape.", 35));

        _mockModerationService
            .Setup(m => m.ModerateOutputAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ModerationResult.Safe());

        // Act
        await _sut.GenerateResponseAsync(context, playerAction);

        // Assert
        _mockTemplateService.Verify(t => t.GetExplorationPrompt(context), Times.Once);
    }

    [Fact]
    public async Task GenerateResponseAsync_WithSuggestedActions_ShouldExtractThem()
    {
        // Arrange
        var context = CreateTestContext();
        var playerAction = "What should I do?";
        var responseWithQuestions = "You could search the room, open the door, or talk to the guard. What will you do?";

        _mockModerationService
            .Setup(m => m.ModerateInputAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ModerationResult.Safe());

        _mockTemplateService
            .Setup(t => t.GetSystemPrompt(It.IsAny<SessionMode>()))
            .Returns("System prompt");

        _mockTemplateService
            .Setup(t => t.FormatContext(context))
            .Returns("Context");

        _mockProvider
            .Setup(p => p.CompleteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((responseWithQuestions, 60));

        _mockModerationService
            .Setup(m => m.ModerateOutputAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ModerationResult.Safe());

        // Act
        var result = await _sut.GenerateResponseAsync(context, playerAction);

        // Assert
        result.SuggestedActions.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GenerateNpcDialogueAsync_WithValidInput_ShouldReturnDialogue()
    {
        // Arrange
        var context = CreateTestContext();
        var npc = new NpcContext("Gandalf", "Wise wizard", "Wizard", "Serious", new Dictionary<string, object>());
        var playerMessage = "Can you help us?";

        _mockModerationService
            .Setup(m => m.ModerateInputAsync(playerMessage, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ModerationResult.Safe());

        _mockTemplateService
            .Setup(t => t.GetNpcPrompt(npc, playerMessage))
            .Returns("NPC prompt");

        _mockProvider
            .Setup(p => p.CompleteAsync(It.IsAny<string>(), "NPC prompt", It.IsAny<CancellationToken>()))
            .ReturnsAsync(("Of course, I shall aid you.", 25));

        _mockModerationService
            .Setup(m => m.ModerateOutputAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ModerationResult.Safe());

        // Act
        var result = await _sut.GenerateNpcDialogueAsync(context, npc, playerMessage);

        // Assert
        result.Should().NotBeNull();
        result.Content.Should().Be("Of course, I shall aid you.");
    }

    [Fact]
    public async Task DescribeSceneAsync_WithValidInput_ShouldReturnDescription()
    {
        // Arrange
        var context = CreateTestContext();
        var location = new LocationContext(
            "Rivendell",
            "Elven City",
            "Beautiful and ancient",
            new List<string> { "Waterfall", "Gardens" },
            new List<string> { "Elrond" },
            new Dictionary<string, object>());

        _mockTemplateService
            .Setup(t => t.GetScenePrompt(location))
            .Returns("Scene prompt");

        _mockProvider
            .Setup(p => p.CompleteAsync(It.IsAny<string>(), "Scene prompt", It.IsAny<CancellationToken>()))
            .ReturnsAsync(("The elven city gleams in the sunset.", 45));

        _mockModerationService
            .Setup(m => m.ModerateOutputAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ModerationResult.Safe());

        // Act
        var result = await _sut.DescribeSceneAsync(context, location);

        // Assert
        result.Should().NotBeNull();
        result.Content.Should().Be("The elven city gleams in the sunset.");
    }

    [Fact]
    public async Task StreamResponseAsync_ShouldYieldChunks()
    {
        // Arrange
        var context = CreateTestContext();
        var playerAction = "I explore";
        var chunks = new List<string> { "You ", "enter ", "the ", "room." };

        _mockModerationService
            .Setup(m => m.ModerateInputAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ModerationResult.Safe());

        _mockModerationService
            .Setup(m => m.ModerateOutputAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ModerationResult.Safe());

        _mockTemplateService
            .Setup(t => t.GetSystemPrompt(It.IsAny<SessionMode>()))
            .Returns("System prompt");

        _mockTemplateService
            .Setup(t => t.FormatContext(context))
            .Returns("Context");

        _mockTemplateService
            .Setup(t => t.GetExplorationPrompt(context))
            .Returns("Exploration prompt");

        _mockProvider
            .Setup(p => p.StreamCompleteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(ToAsyncEnumerable(chunks));

        // Act
        var result = new List<string>();
        await foreach (var chunk in _sut.StreamResponseAsync(context, playerAction))
        {
            result.Add(chunk);
        }

        // Assert
        result.Should().BeEquivalentTo(chunks);
    }

    [Fact]
    public async Task GenerateResponseAsync_WithCancellation_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var context = CreateTestContext();
        var playerAction = "I wait";
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _mockModerationService
            .Setup(m => m.ModerateInputAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _sut.GenerateResponseAsync(context, playerAction, cts.Token));
    }

    [Fact]
    public async Task GenerateResponseAsync_WithSoloMode_ShouldUseSoloSystemPrompt()
    {
        // Arrange
        var context = CreateTestContext(characterCount: 1);
        var playerAction = "I proceed";

        _mockModerationService
            .Setup(m => m.ModerateInputAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ModerationResult.Safe());

        _mockTemplateService
            .Setup(t => t.GetSystemPrompt(SessionMode.Solo))
            .Returns("Solo system prompt");

        _mockTemplateService
            .Setup(t => t.FormatContext(context))
            .Returns("Context");

        _mockProvider
            .Setup(p => p.CompleteAsync("Solo system prompt", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(("You continue alone.", 20));

        _mockModerationService
            .Setup(m => m.ModerateOutputAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ModerationResult.Safe());

        // Act
        await _sut.GenerateResponseAsync(context, playerAction);

        // Assert
        _mockTemplateService.Verify(t => t.GetSystemPrompt(SessionMode.Solo), Times.Once);
    }

    [Fact]
    public async Task GenerateResponseAsync_WithMultipleCharacters_ShouldUseMultiplayerPrompt()
    {
        // Arrange
        var context = CreateTestContext(characterCount: 3);
        var playerAction = "We advance";

        _mockModerationService
            .Setup(m => m.ModerateInputAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ModerationResult.Safe());

        _mockTemplateService
            .Setup(t => t.GetSystemPrompt(SessionMode.Multiplayer))
            .Returns("Multiplayer system prompt");

        _mockTemplateService
            .Setup(t => t.FormatContext(context))
            .Returns("Context");

        _mockProvider
            .Setup(p => p.CompleteAsync("Multiplayer system prompt", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(("The party moves forward together.", 25));

        _mockModerationService
            .Setup(m => m.ModerateOutputAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ModerationResult.Safe());

        // Act
        await _sut.GenerateResponseAsync(context, playerAction);

        // Assert
        _mockTemplateService.Verify(t => t.GetSystemPrompt(SessionMode.Multiplayer), Times.Once);
    }

    private static SessionContext CreateTestContext(bool inCombat = false, int characterCount = 1)
    {
        var characters = new List<Character>();
        for (int i = 0; i < characterCount; i++)
        {
            characters.Add(new Character
            {
                Id = i + 1,
                Name = $"Hero{i + 1}",
                Class = CharacterClass.Fighter,
                Level = 5,
                HitPoints = 50,
                MaxHitPoints = 50,
                ArmorClass = 16,
                AbilityScores = new AbilityScores(16, 14, 14, 10, 12, 10)
            });
        }

        return new SessionContext(
            1,
            new List<Message>(),
            characters,
            "Test Scene",
            new Dictionary<string, object> { ["InCombat"] = inCombat });
    }

    private static async IAsyncEnumerable<string> ToAsyncEnumerable(IEnumerable<string> items)
    {
        foreach (var item in items)
        {
            await Task.Yield();
            yield return item;
        }
    }
}
