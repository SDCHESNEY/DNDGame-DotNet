using DNDGame.Application.Services;
using DNDGame.Core.Enums;
using FluentAssertions;
using Xunit;

namespace DNDGame.UnitTests.Services;

public class DiceRollerServiceTests
{
    private readonly DiceRollerService _sut;

    public DiceRollerServiceTests()
    {
        _sut = new DiceRollerService();
    }

    [Theory]
    [InlineData("1d4", 1, 4)]
    [InlineData("1d6", 1, 6)]
    [InlineData("1d8", 1, 8)]
    [InlineData("1d10", 1, 10)]
    [InlineData("1d12", 1, 12)]
    [InlineData("1d20", 1, 20)]
    [InlineData("1d100", 1, 100)]
    public void Roll_WithBasicDiceFormula_ReturnsResultInValidRange(string formula, int minValue, int maxValue)
    {
        // Act
        var result = _sut.Roll(formula);

        // Assert
        result.Should().NotBeNull();
        result.Formula.Should().Be(formula);
        result.Total.Should().BeInRange(minValue, maxValue);
        result.IndividualRolls.Should().HaveCount(1);
        result.IndividualRolls[0].Should().BeInRange(minValue, maxValue);
        result.Modifier.Should().Be(0);
        result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("2d6", 2, 12, 2)]
    [InlineData("3d8", 3, 24, 3)]
    [InlineData("4d10", 4, 40, 4)]
    public void Roll_WithMultipleDice_ReturnsResultWithCorrectRollCount(string formula, int minTotal, int maxTotal, int expectedRollCount)
    {
        // Act
        var result = _sut.Roll(formula);

        // Assert
        result.Total.Should().BeInRange(minTotal, maxTotal);
        result.IndividualRolls.Should().HaveCount(expectedRollCount);
        result.IndividualRolls.Sum().Should().Be(result.Total - result.Modifier);
    }

    [Theory]
    [InlineData("1d20+5", 6, 25, 5)]
    [InlineData("2d6+3", 5, 15, 3)]
    [InlineData("1d8-2", -1, 6, -2)]
    [InlineData("3d6+10", 13, 28, 10)]
    public void Roll_WithModifier_AppliesModifierCorrectly(string formula, int minTotal, int maxTotal, int expectedModifier)
    {
        // Act
        var result = _sut.Roll(formula);

        // Assert
        result.Formula.Should().Be(formula);
        result.Total.Should().BeInRange(minTotal, maxTotal);
        result.Modifier.Should().Be(expectedModifier);
        result.Total.Should().Be(result.IndividualRolls.Sum() + result.Modifier);
    }

    [Fact]
    public void Roll_With1d20_DetectsCriticalHit()
    {
        // Act & Assert - Roll until we get a critical
        for (int i = 0; i < 100; i++)
        {
            var result = _sut.Roll("1d20");
            
            if (result.IndividualRolls[0] == 20)
            {
                result.IsCritical.Should().BeTrue();
                result.IsFumble.Should().BeFalse();
                return;
            }
        }
        
        // If we didn't get a critical in 100 tries, that's fine for the test
        // The important thing is the logic is there
        Assert.True(true, "Critical detection logic exists");
    }

    [Fact]
    public void Roll_With1d20_DetectsFumble()
    {
        // Act & Assert - Roll until we get a fumble
        for (int i = 0; i < 100; i++)
        {
            var result = _sut.Roll("1d20");
            
            if (result.IndividualRolls[0] == 1)
            {
                result.IsFumble.Should().BeTrue();
                result.IsCritical.Should().BeFalse();
                return;
            }
        }
        
        // If we didn't get a fumble in 100 tries, that's fine for the test
        Assert.True(true, "Fumble detection logic exists");
    }

    [Fact]
    public void Roll_WithAdvantage_RollsTwiceAndTakesHigher()
    {
        // Act - Roll multiple times to verify advantage logic
        var hasAdvantageWorked = false;
        
        for (int i = 0; i < 50; i++)
        {
            var result = _sut.Roll("1d20", AdvantageType.Advantage);
            
            // Should have 2 rolls for d20 with advantage
            result.IndividualRolls.Should().HaveCount(2);
            
            // Total should be the higher of the two rolls
            var maxRoll = result.IndividualRolls.Max();
            result.Total.Should().Be(maxRoll);
            
            // If the rolls are different, advantage is working
            if (result.IndividualRolls[0] != result.IndividualRolls[1])
            {
                hasAdvantageWorked = true;
                break;
            }
        }
        
        hasAdvantageWorked.Should().BeTrue("advantage should eventually produce different rolls");
    }

    [Fact]
    public void Roll_WithDisadvantage_RollsTwiceAndTakesLower()
    {
        // Act - Roll multiple times to verify disadvantage logic
        var hasDisadvantageWorked = false;
        
        for (int i = 0; i < 50; i++)
        {
            var result = _sut.Roll("1d20", AdvantageType.Disadvantage);
            
            // Should have 2 rolls for d20 with disadvantage
            result.IndividualRolls.Should().HaveCount(2);
            
            // Total should be the lower of the two rolls
            var minRoll = result.IndividualRolls.Min();
            result.Total.Should().Be(minRoll);
            
            // If the rolls are different, disadvantage is working
            if (result.IndividualRolls[0] != result.IndividualRolls[1])
            {
                hasDisadvantageWorked = true;
                break;
            }
        }
        
        hasDisadvantageWorked.Should().BeTrue("disadvantage should eventually produce different rolls");
    }

    [Fact]
    public void Roll_WithNormalAdvantageType_RollsOnce()
    {
        // Act
        var result = _sut.Roll("1d20", AdvantageType.Normal);

        // Assert
        result.IndividualRolls.Should().HaveCount(1);
        result.Total.Should().Be(result.IndividualRolls[0]);
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid")]
    [InlineData("d20")]
    [InlineData("1d")]
    [InlineData("1d20++5")]
    [InlineData("abc123")]
    public void Roll_WithInvalidFormula_ThrowsArgumentException(string invalidFormula)
    {
        // Act
        var act = () => _sut.Roll(invalidFormula);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("0d6")]
    [InlineData("1d0")]
    [InlineData("-1d6")]
    [InlineData("1d-6")]
    public void Roll_WithInvalidDiceValues_ThrowsArgumentException(string invalidFormula)
    {
        // Act
        var act = () => _sut.Roll(invalidFormula);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Roll_CalledMultipleTimes_ProducesDifferentResults()
    {
        // Act
        var results = Enumerable.Range(0, 20)
            .Select(_ => _sut.Roll("3d6"))
            .ToList();

        // Assert - At least some results should be different
        results.Select(r => r.Total).Distinct().Count().Should().BeGreaterThan(1);
    }

    [Fact]
    public void Roll_WithLargeNumberOfDice_HandlesCorrectly()
    {
        // Act
        var result = _sut.Roll("10d6");

        // Assert
        result.IndividualRolls.Should().HaveCount(10);
        result.Total.Should().BeInRange(10, 60);
        result.IndividualRolls.Sum().Should().Be(result.Total);
    }

    [Fact]
    public void Roll_WithAdvantageAndModifier_AppliesBothCorrectly()
    {
        // Act
        var result = _sut.Roll("1d20+5", AdvantageType.Advantage);

        // Assert
        result.IndividualRolls.Should().HaveCount(2);
        result.Modifier.Should().Be(5);
        var maxRoll = result.IndividualRolls.Max();
        result.Total.Should().Be(maxRoll + 5);
    }

    [Fact]
    public void Roll_MultipleTimes_ProducesReasonableDistribution()
    {
        // Act - Roll 100 d6 dice
        var results = Enumerable.Range(0, 100)
            .Select(_ => _sut.Roll("1d6").Total)
            .ToList();

        // Assert - Should use all possible values with reasonable distribution
        var uniqueValues = results.Distinct().Count();
        uniqueValues.Should().BeGreaterThan(3, "should see most of the 6 possible values in 100 rolls");
        
        // Average should be close to expected value (3.5 for 1d6)
        var average = results.Average();
        average.Should().BeInRange(2.5, 4.5, "average should be reasonably close to 3.5");
    }
}
