using DNDGame.Application.Services;
using DNDGame.Core.Entities;
using DNDGame.Core.Enums;
using DNDGame.Core.Models;
using DNDGame.Core.ValueObjects;
using FluentAssertions;
using Xunit;

namespace DNDGame.UnitTests.Services;

public class PromptTemplateServiceTests
{
    private readonly PromptTemplateService _sut;

    public PromptTemplateServiceTests()
    {
        _sut = new PromptTemplateService();
    }

    [Fact]
    public void GetSystemPrompt_WithSoloMode_ShouldIncludeSoloGuidance()
    {
        // Arrange
        var mode = SessionMode.Solo;

        // Act
        var result = _sut.GetSystemPrompt(mode);

        // Assert
        result.Should().Contain("solo adventure");
        result.Should().Contain("vivid");
        result.Should().Contain("Dungeon Master");
    }

    [Fact]
    public void GetSystemPrompt_WithMultiplayerMode_ShouldIncludeMultiplayerGuidance()
    {
        // Arrange
        var mode = SessionMode.Multiplayer;

        // Act
        var result = _sut.GetSystemPrompt(mode);

        // Assert
        result.Should().Contain("multiplayer adventure");
        result.Should().Contain("party");
        result.Should().Contain("Dungeon Master");
    }

    [Fact]
    public void GetCombatPrompt_WithCharactersAndScene_ShouldIncludeAllDetails()
    {
        // Arrange
        var context = new SessionContext(
            1,
            new List<Message>(),
            new List<Character>
            {
                new Character { Id = 1, Name = "Gandalf", Class = CharacterClass.Wizard, Level = 10, HitPoints = 50, MaxHitPoints = 60, ArmorClass = 15, AbilityScores = new AbilityScores(10, 14, 12, 18, 16, 14) },
                new Character { Id = 2, Name = "Aragorn", Class = CharacterClass.Fighter, Level = 8, HitPoints = 80, MaxHitPoints = 100, ArmorClass = 18, AbilityScores = new AbilityScores(16, 14, 16, 10, 12, 14) }
            },
            "Dark Cave",
            new Dictionary<string, object>());

        // Act
        var result = _sut.GetCombatPrompt(context);

        // Assert
        result.Should().Contain("Gandalf");
        result.Should().Contain("Aragorn");
        result.Should().Contain("Wizard");
        result.Should().Contain("Fighter");
        result.Should().Contain("50/60");
        result.Should().Contain("80/100");
        result.Should().Contain("Dark Cave");
        result.Should().Contain("COMBAT");
    }

    [Fact]
    public void GetCombatPrompt_WithNoCharacters_ShouldReturnBasicPrompt()
    {
        // Arrange
        var context = new SessionContext(
            1,
            new List<Message>(),
            new List<Character>(),
            null,
            new Dictionary<string, object>());

        // Act
        var result = _sut.GetCombatPrompt(context);

        // Assert
        result.Should().Contain("COMBAT");
        result.Should().NotContain("Active Characters");
    }

    [Fact]
    public void GetExplorationPrompt_WithSceneDescription_ShouldIncludeContext()
    {
        // Arrange
        var context = new SessionContext(
            1,
            new List<Message>(),
            new List<Character>(),
            "Ancient forest with towering trees",
            new Dictionary<string, object>());

        // Act
        var result = _sut.GetExplorationPrompt(context);

        // Assert
        result.Should().Contain("Ancient forest with towering trees");
        result.Should().Contain("EXPLORATION");
    }

    [Fact]
    public void GetExplorationPrompt_WithNullScene_ShouldReturnBasicPrompt()
    {
        // Arrange
        var context = new SessionContext(
            1,
            new List<Message>(),
            new List<Character>(),
            null,
            new Dictionary<string, object>());

        // Act
        var result = _sut.GetExplorationPrompt(context);

        // Assert
        result.Should().Contain("EXPLORATION");
        result.Should().NotContain("Current Location:");
    }

    [Fact]
    public void GetNpcPrompt_WithFullNpcContext_ShouldIncludeAllDetails()
    {
        // Arrange
        var npc = new NpcContext(
            "Elrond",
            "Wise and ancient elf lord",
            "Lord of Rivendell",
            "Contemplative",
            new Dictionary<string, object> { ["knowledge"] = "Ancient lore" });
        var playerMessage = "Can you help us?";

        // Act
        var result = _sut.GetNpcPrompt(npc, playerMessage);

        // Assert
        result.Should().Contain("Elrond");
        result.Should().Contain("Wise and ancient elf lord");
        result.Should().Contain("Lord of Rivendell");
        result.Should().Contain("Contemplative");
        result.Should().Contain("Can you help us?");
        result.Should().Contain("in character");
    }

    [Fact]
    public void GetNpcPrompt_WithMinimalNpcContext_ShouldWorkCorrectly()
    {
        // Arrange
        var npc = new NpcContext(
            "Bob",
            "Grumpy blacksmith",
            null,
            null,
            new Dictionary<string, object>());
        var playerMessage = "Hello";

        // Act
        var result = _sut.GetNpcPrompt(npc, playerMessage);

        // Assert
        result.Should().Contain("Bob");
        result.Should().Contain("Grumpy blacksmith");
        result.Should().Contain("Hello");
        result.Should().NotContain("Occupation:");
        result.Should().NotContain("Mood:");
    }

    [Fact]
    public void GetScenePrompt_WithFullLocationContext_ShouldIncludeAllDetails()
    {
        // Arrange
        var location = new LocationContext(
            "The Prancing Pony",
            "Tavern",
            "A cozy inn with a roaring fireplace",
            new List<string> { "Bar", "Fireplace", "Upstairs rooms" },
            new List<string> { "Barliman Butterbur", "Mysterious hooded figure" },
            new Dictionary<string, object> { ["atmosphere"] = "Warm and welcoming" });

        // Act
        var result = _sut.GetScenePrompt(location);

        // Assert
        result.Should().Contain("The Prancing Pony");
        result.Should().Contain("Tavern");
        result.Should().Contain("cozy inn");
        result.Should().Contain("Bar");
        result.Should().Contain("Fireplace");
        result.Should().Contain("Barliman Butterbur");
        result.Should().Contain("Mysterious hooded figure");
        result.Should().Contain("vivid");
    }

    [Fact]
    public void FormatContext_WithMessagesAndCharacters_ShouldFormatCorrectly()
    {
        // Arrange
        var context = new SessionContext(
            1,
            new List<Message>
            {
                new Message { Content = "I attack the orc", Role = MessageRole.Player, Timestamp = DateTime.UtcNow.AddMinutes(-2), AuthorId = "1" },
                new Message { Content = "You strike the orc!", Role = MessageRole.DungeonMaster, Timestamp = DateTime.UtcNow.AddMinutes(-1), AuthorId = "0" }
            },
            new List<Character>
            {
                new Character { Name = "Hero", Class = CharacterClass.Fighter, Level = 5, HitPoints = 40, MaxHitPoints = 50, ArmorClass = 16, AbilityScores = new AbilityScores(16, 14, 14, 10, 12, 10) }
            },
            "Dungeon",
            new Dictionary<string, object> { ["InCombat"] = true });

        // Act
        var result = _sut.FormatContext(context);

        // Assert
        result.Should().Contain("Recent Events");
        result.Should().Contain("I attack the orc");
        result.Should().Contain("You strike the orc");
        result.Should().Contain("Hero");
        result.Should().Contain("Fighter");
        result.Should().Contain("40/50");
        result.Should().Contain("InCombat");
    }

    [Fact]
    public void FormatContext_WithEmptyContext_ShouldHandleGracefully()
    {
        // Arrange
        var context = new SessionContext(
            1,
            new List<Message>(),
            new List<Character>(),
            null,
            new Dictionary<string, object>());

        // Act
        var result = _sut.FormatContext(context);

        // Assert
        result.Should().Contain("GAME CONTEXT");
        result.Should().NotContain("Recent Events");
    }
}
