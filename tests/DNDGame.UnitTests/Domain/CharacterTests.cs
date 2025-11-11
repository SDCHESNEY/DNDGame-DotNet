using DNDGame.Core.Entities;
using DNDGame.Core.Enums;
using DNDGame.Core.ValueObjects;
using FluentAssertions;
using Xunit;

namespace DNDGame.UnitTests.Domain;

public class CharacterTests
{
    [Theory]
    [InlineData(1, 2)]
    [InlineData(4, 2)]
    [InlineData(5, 3)]
    [InlineData(8, 3)]
    [InlineData(9, 4)]
    [InlineData(20, 6)]
    public void ProficiencyBonus_CalculatesCorrectlyByLevel(int level, int expectedBonus)
    {
        // Arrange
        var character = CreateTestCharacter();
        character.Level = level;

        // Act & Assert
        character.ProficiencyBonus.Should().Be(expectedBonus);
    }

    [Fact]
    public void Character_InitializesWithDefaultValues()
    {
        // Arrange & Act
        var character = CreateTestCharacter();

        // Assert
        character.Level.Should().Be(1);
        character.Skills.Should().BeEmpty();
        character.Inventory.Should().BeEmpty();
        character.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Character_AcceptsAllRequiredProperties()
    {
        // Arrange
        var abilities = new AbilityScores(16, 14, 15, 10, 12, 8);
        
        // Act
        var character = new Character
        {
            Name = "Gandalf",
            AbilityScores = abilities,
            PlayerId = 1,
            Class = CharacterClass.Wizard,
            Level = 5,
            MaxHitPoints = 30,
            HitPoints = 25,
            ArmorClass = 12
        };

        // Assert
        character.Name.Should().Be("Gandalf");
        character.Class.Should().Be(CharacterClass.Wizard);
        character.Level.Should().Be(5);
        character.MaxHitPoints.Should().Be(30);
        character.HitPoints.Should().Be(25);
        character.ArmorClass.Should().Be(12);
        character.ProficiencyBonus.Should().Be(3);
    }

    private static Character CreateTestCharacter()
    {
        return new Character
        {
            Name = "Test Character",
            AbilityScores = new AbilityScores(10, 10, 10, 10, 10, 10),
            PlayerId = 1,
            MaxHitPoints = 10,
            HitPoints = 10,
            ArmorClass = 10
        };
    }
}
