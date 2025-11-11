using DNDGame.Core.Enums;
using DNDGame.Core.ValueObjects;
using FluentAssertions;
using Xunit;

namespace DNDGame.UnitTests.Domain;

public class AbilityScoresTests
{
    [Theory]
    [InlineData(10, 0)]
    [InlineData(8, -1)]
    [InlineData(12, 1)]
    [InlineData(20, 5)]
    [InlineData(1, -4)]
    [InlineData(30, 10)]
    public void GetModifier_CalculatesCorrectly(int abilityScore, int expectedModifier)
    {
        // Arrange
        var abilities = new AbilityScores(abilityScore, 10, 10, 10, 10, 10);
        
        // Act
        var modifier = abilities.GetModifier(abilityScore);

        // Assert
        modifier.Should().Be(expectedModifier);
    }

    [Fact]
    public void AbilityScores_CreatesWithAllValues()
    {
        // Arrange
        var abilities = new AbilityScores(
            Strength: 16,
            Dexterity: 14,
            Constitution: 15,
            Intelligence: 10,
            Wisdom: 12,
            Charisma: 8
        );

        // Act & Assert
        abilities.Strength.Should().Be(16);
        abilities.StrengthModifier.Should().Be(3);
        abilities.Dexterity.Should().Be(14);
        abilities.DexterityModifier.Should().Be(2);
        abilities.Constitution.Should().Be(15);
        abilities.ConstitutionModifier.Should().Be(2);
        abilities.Intelligence.Should().Be(10);
        abilities.IntelligenceModifier.Should().Be(0);
        abilities.Wisdom.Should().Be(12);
        abilities.WisdomModifier.Should().Be(1);
        abilities.Charisma.Should().Be(8);
        abilities.CharismaModifier.Should().Be(-1);
    }
}
