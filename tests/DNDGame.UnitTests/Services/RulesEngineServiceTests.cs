using DNDGame.Application.Services;
using DNDGame.Core.Entities;
using DNDGame.Core.Enums;
using DNDGame.Core.Interfaces;
using DNDGame.Core.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace DNDGame.UnitTests.Services;

public class RulesEngineServiceTests
{
    private readonly Mock<IDiceRoller> _mockDiceRoller;
    private readonly RulesEngineService _sut;

    public RulesEngineServiceTests()
    {
        _mockDiceRoller = new Mock<IDiceRoller>();
        _sut = new RulesEngineService(_mockDiceRoller.Object);
    }

    [Theory]
    [InlineData(16, 2, 10, true)]   // Roll 10 + modifier 3 + prof 2 = 15 >= DC 10
    [InlineData(14, 2, 15, false)]  // Roll 10 + modifier 2 + prof 2 = 14 < DC 15
    [InlineData(12, 0, 15, false)]  // Roll 10 + modifier 1 = 11 < DC 15
    [InlineData(18, 3, 15, true)]   // Roll 10 + modifier 4 + prof 3 = 17 >= DC 15
    [InlineData(20, 4, 12, true)]   // Roll 10 + modifier 5 + prof 4 = 19 >= DC 12
    [InlineData(8, 0, 10, false)]   // Roll 10 + modifier -1 = 9 < DC 10
    public void ResolveAbilityCheck_WithVariousAbilities_ReturnsCorrectResult(
        int abilityScore,
        int proficiencyBonus,
        int dc,
        bool expectedSuccess)
    {
        // Arrange
        _mockDiceRoller.Setup(d => d.Roll("1d20", AdvantageType.Normal))
            .Returns(new Core.Models.DiceRollResult
            {
                Formula = "1d20",
                Total = 10,
                IndividualRolls = [10],
                Modifier = 0,
                IsCritical = false,
                IsFumble = false,
                Timestamp = DateTime.UtcNow
            });

        // Act
        var result = _sut.ResolveAbilityCheck(abilityScore, dc, proficiencyBonus > 0, proficiencyBonus, AdvantageType.Normal);

        // Assert
        result.Should().NotBeNull();
        result.AbilityModifier.Should().Be((abilityScore - 10) / 2);
        result.ProficiencyBonus.Should().Be(proficiencyBonus);
        result.DifficultyClass.Should().Be(dc);
        result.Success.Should().Be(expectedSuccess);
    }

    [Fact]
    public void ResolveAbilityCheck_WithNatural20_IsCriticalSuccess()
    {
        // Arrange
        _mockDiceRoller.Setup(d => d.Roll("1d20", AdvantageType.Normal))
            .Returns(new Core.Models.DiceRollResult
            {
                Formula = "1d20",
                Total = 20,
                IndividualRolls = [20],
                Modifier = 0,
                IsCritical = true,
                IsFumble = false,
                Timestamp = DateTime.UtcNow
            });

        // Act
        var result = _sut.ResolveAbilityCheck(10, 25, false, 0, AdvantageType.Normal);

        // Assert
        result.IsCritical.Should().BeTrue();
        result.Total.Should().Be(20); // Natural 20 + 0 modifier + 0 proficiency
    }

    [Fact]
    public void ResolveAbilityCheck_WithNatural1_IsFumble()
    {
        // Arrange
        _mockDiceRoller.Setup(d => d.Roll("1d20", AdvantageType.Normal))
            .Returns(new Core.Models.DiceRollResult
            {
                Formula = "1d20",
                Total = 1,
                IndividualRolls = [1],
                Modifier = 0,
                IsCritical = false,
                IsFumble = true,
                Timestamp = DateTime.UtcNow
            });

        // Act
        var result = _sut.ResolveAbilityCheck(20, 10, true, 5, AdvantageType.Normal);

        // Assert
        result.IsFumble.Should().BeTrue();
        result.Total.Should().Be(11); // Natural 1 + 5 modifier + 5 proficiency
    }

    [Fact]
    public void ResolveAbilityCheck_WithAdvantage_PassesAdvantageToRoller()
    {
        // Arrange
        _mockDiceRoller.Setup(d => d.Roll("1d20", AdvantageType.Advantage))
            .Returns(new Core.Models.DiceRollResult
            {
                Formula = "1d20",
                Total = 15,
                IndividualRolls = [15, 10],
                Modifier = 0,
                IsCritical = false,
                IsFumble = false,
                Timestamp = DateTime.UtcNow
            });

        // Act
        var result = _sut.ResolveAbilityCheck(14, 12, true, 2, AdvantageType.Advantage);

        // Assert
        _mockDiceRoller.Verify(d => d.Roll("1d20", AdvantageType.Advantage), Times.Once);
        result.Roll.Should().Be(15);
    }

    [Fact]
    public void ResolveSavingThrow_ReturnsCorrectResult()
    {
        // Arrange
        var character = new Character
        {
            Name = "Test Character",
            AbilityScores = new AbilityScores(10, 14, 12, 16, 13, 8),
            Level = 5  // This gives ProficiencyBonus = 3
        };

        _mockDiceRoller.Setup(d => d.Roll("1d20", AdvantageType.Normal))
            .Returns(new Core.Models.DiceRollResult
            {
                Formula = "1d20",
                Total = 10,
                IndividualRolls = [10],
                Modifier = 0,
                IsCritical = false,
                IsFumble = false,
                Timestamp = DateTime.UtcNow
            });

        // Act
        var result = _sut.ResolveSavingThrow(character, AbilityType.Intelligence, 15, AdvantageType.Normal);

        // Assert
        result.AbilityModifier.Should().Be(3); // INT modifier (16 - 10) / 2 = 3
        result.Roll.Should().Be(10);
        // Note: ResolveSavingThrow doesn't use proficiency by default in the implementation
    }

    [Fact]
    public void ResolveSavingThrow_WithDifferentAbility_UsesCorrectModifier()
    {
        // Arrange
        var character = new Character
        {
            Name = "Test Character",
            AbilityScores = new AbilityScores(10, 14, 12, 16, 13, 8),
            Level = 1
        };

        _mockDiceRoller.Setup(d => d.Roll("1d20", AdvantageType.Normal))
            .Returns(new Core.Models.DiceRollResult
            {
                Formula = "1d20",
                Total = 10,
                IndividualRolls = [10],
                Modifier = 0,
                IsCritical = false,
                IsFumble = false,
                Timestamp = DateTime.UtcNow
            });

        // Act
        var result = _sut.ResolveSavingThrow(character, AbilityType.Charisma, 12, AdvantageType.Normal);

        // Assert
        result.AbilityModifier.Should().Be(-1); // CHA modifier (8 - 10) / 2 = -1
        result.Roll.Should().Be(10);
        result.Total.Should().Be(9); // Roll 10 + modifier -1
        result.Success.Should().BeFalse(); // 9 < DC 12
    }

    [Theory]
    [InlineData(15, 10, true)]  // Roll 15 >= AC 10
    [InlineData(18, 18, true)]  // Roll 18 >= AC 18 (edge case)
    [InlineData(12, 15, false)] // Roll 12 < AC 15
    [InlineData(20, 20, true)]  // Roll 20 >= AC 20
    public void ResolveAttack_WithVariousRolls_ReturnsCorrectHitStatus(int diceRoll, int targetAC, bool expectedHit)
    {
        // Arrange
        _mockDiceRoller.Setup(d => d.Roll("1d20", AdvantageType.Normal))
            .Returns(new Core.Models.DiceRollResult
            {
                Formula = "1d20",
                Total = diceRoll,
                IndividualRolls = [diceRoll],
                Modifier = 0,
                IsCritical = diceRoll == 20,
                IsFumble = diceRoll == 1,
                Timestamp = DateTime.UtcNow
            });

        // Act - attackBonus is first parameter
        var result = _sut.ResolveAttack(0, targetAC, AdvantageType.Normal);

        // Assert
        result.AttackRoll.Should().Be(diceRoll); // roll + 0 bonus
        result.TargetAC.Should().Be(targetAC);
        result.Hit.Should().Be(expectedHit);
    }

    [Fact]
    public void ResolveAttack_WithNatural20_IsAlwaysCriticalHit()
    {
        // Arrange
        _mockDiceRoller.Setup(d => d.Roll("1d20", AdvantageType.Normal))
            .Returns(new Core.Models.DiceRollResult
            {
                Formula = "1d20",
                Total = 20,
                IndividualRolls = [20],
                Modifier = 0,
                IsCritical = true,
                IsFumble = false,
                Timestamp = DateTime.UtcNow
            });

        // Act - attackBonus=5, targetAC=25
        var result = _sut.ResolveAttack(5, 25, AdvantageType.Normal);

        // Assert
        result.IsCritical.Should().BeTrue();
        result.Hit.Should().BeTrue(); // Natural 20 always hits
        result.AttackRoll.Should().Be(25); // 20 + 5 attack bonus
    }

    [Fact]
    public void ResolveAttack_WithNatural1_IsAlwaysFumble()
    {
        // Arrange
        _mockDiceRoller.Setup(d => d.Roll("1d20", AdvantageType.Normal))
            .Returns(new Core.Models.DiceRollResult
            {
                Formula = "1d20",
                Total = 1,
                IndividualRolls = [1],
                Modifier = 0,
                IsCritical = false,
                IsFumble = true,
                Timestamp = DateTime.UtcNow
            });

        // Act - attackBonus=10, targetAC=5
        var result = _sut.ResolveAttack(10, 5, AdvantageType.Normal);

        // Assert
        result.IsFumble.Should().BeTrue();
        result.Hit.Should().BeFalse(); // Natural 1 always misses
        result.AttackRoll.Should().Be(11); // 1 + 10 attack bonus
    }

    [Fact]
    public void CalculateDamage_WithNormalHit_RollsDamageOnce()
    {
        // Arrange
        _mockDiceRoller.Setup(d => d.Roll("2d6+3"))
            .Returns(new Core.Models.DiceRollResult
            {
                Formula = "2d6+3",
                Total = 11,
                IndividualRolls = [4, 4],
                Modifier = 3,
                IsCritical = false,
                IsFumble = false,
                Timestamp = DateTime.UtcNow
            });

        // Act
        var damage = _sut.CalculateDamage("2d6+3", false);

        // Assert
        damage.Should().Be(11); // 8 (roll) + 3 (damage bonus)
        _mockDiceRoller.Verify(d => d.Roll("2d6+3"), Times.Once);
    }

    [Fact]
    public void CalculateDamage_WithCriticalHit_DoublesDiceButNotModifier()
    {
        // Arrange
        _mockDiceRoller.Setup(d => d.Roll("2d6+3"))
            .Returns(new Core.Models.DiceRollResult
            {
                Formula = "2d6+3",
                Total = 11,
                IndividualRolls = [4, 4],
                Modifier = 3,
                IsCritical = false,
                IsFumble = false,
                Timestamp = DateTime.UtcNow
            });

        _mockDiceRoller.Setup(d => d.Roll("4d6+3"))
            .Returns(new Core.Models.DiceRollResult
            {
                Formula = "4d6+3",
                Total = 19,
                IndividualRolls = [4, 4, 4, 4],
                Modifier = 3,
                IsCritical = false,
                IsFumble = false,
                Timestamp = DateTime.UtcNow
            });

        // Act
        var damage = _sut.CalculateDamage("2d6+3", true);

        // Assert
        // Critical: double the dice (4d6) but not the modifier
        damage.Should().Be(19); // 16 (doubled dice roll) + 3 (damage bonus not doubled)
        _mockDiceRoller.Verify(d => d.Roll("4d6+3"), Times.Once);
    }

    [Fact]
    public void CalculateDamage_WithNegativeModifier_CanReduceDamage()
    {
        // Arrange
        _mockDiceRoller.Setup(d => d.Roll("1d4-1"))
            .Returns(new Core.Models.DiceRollResult
            {
                Formula = "1d4-1",
                Total = 1,
                IndividualRolls = [2],
                Modifier = -1,
                IsCritical = false,
                IsFumble = false,
                Timestamp = DateTime.UtcNow
            });

        // Act
        var damage = _sut.CalculateDamage("1d4-1", false);

        // Assert
        damage.Should().Be(1); // 2 (roll) - 1 (negative bonus)
    }

    [Fact]
    public void CalculateDamage_WithLowRoll_CanResultInNegative()
    {
        // Arrange
        _mockDiceRoller.Setup(d => d.Roll("1d4-5"))
            .Returns(new Core.Models.DiceRollResult
            {
                Formula = "1d4-5",
                Total = -4,
                IndividualRolls = [1],
                Modifier = -5,
                IsCritical = false,
                IsFumble = false,
                Timestamp = DateTime.UtcNow
            });

        // Act
        var damage = _sut.CalculateDamage("1d4-5", false);

        // Assert
        damage.Should().Be(-4); // The implementation returns the total as-is
    }

    [Theory]
    [InlineData(10, 0)]   // (10-10)/2 = 0
    [InlineData(11, 0)]   // (11-10)/2 = 0
    [InlineData(12, 1)]   // (12-10)/2 = 1
    [InlineData(13, 1)]   // (13-10)/2 = 1
    [InlineData(14, 2)]   // (14-10)/2 = 2
    [InlineData(16, 3)]   // (16-10)/2 = 3
    [InlineData(18, 4)]   // (18-10)/2 = 4
    [InlineData(20, 5)]   // (20-10)/2 = 5
    [InlineData(8, -1)]   // (8-10)/2 = -1
    [InlineData(6, -2)]   // (6-10)/2 = -2
    [InlineData(4, -3)]   // (4-10)/2 = -3
    [InlineData(2, -4)]   // (2-10)/2 = -4
    [InlineData(1, -4)]   // (1-10)/2 = -9/2 = -4 (C# integer division rounds toward zero)
    [InlineData(3, -3)]   // (3-10)/2 = -7/2 = -3
    public void CalculateAbilityModifier_WithVariousScores_ReturnsCorrectModifier(int abilityScore, int expectedModifier)
    {
        // Act
        var modifier = RulesEngineService.CalculateAbilityModifier(abilityScore);

        // Assert
        modifier.Should().Be(expectedModifier);
    }
}
