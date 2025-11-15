using Bunit;
using DNDGame.Core.Interfaces;
using DNDGame.Core.Models;
using DNDGame.Core.ValueObjects;
using DNDGame.Web.Components.Pages;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using FluentAssertions;

namespace DNDGame.ComponentTests.Pages;

public class DiceTests : TestContext
{
    private readonly Mock<IDiceRoller> _mockDiceRoller;

    public DiceTests()
    {
        _mockDiceRoller = new Mock<IDiceRoller>();
    }

    [Fact]
    public void Dice_Renders_Successfully()
    {
        // Arrange
        Services.Add(new ServiceDescriptor(typeof(IDiceRoller), _mockDiceRoller.Object));
        
        // Act
        var cut = RenderComponent<Dice>();

        // Assert
        cut.Should().NotBeNull();
    }

    [Fact]
    public void Dice_HasFormulaInput()
    {
        // Arrange
        Services.Add(new ServiceDescriptor(typeof(IDiceRoller), _mockDiceRoller.Object));
        
        // Act
        var cut = RenderComponent<Dice>();

        // Assert
        var input = cut.Find("input[type='text']");
        input.Should().NotBeNull();
        input.GetAttribute("value").Should().Be("1d20");
    }

    [Fact]
    public void Dice_HasRollButton()
    {
        // Arrange
        Services.Add(new ServiceDescriptor(typeof(IDiceRoller), _mockDiceRoller.Object));
        
        // Act
        var cut = RenderComponent<Dice>();

        // Assert
        var button = cut.Find("button:contains('Roll')");
        button.Should().NotBeNull();
    }

    [Fact]
    public void Dice_HasQuickDiceButtons()
    {
        // Arrange
        Services.Add(new ServiceDescriptor(typeof(IDiceRoller), _mockDiceRoller.Object));
        
        // Act
        var cut = RenderComponent<Dice>();

        // Assert
        cut.Markup.Should().Contain("d20");
        cut.Markup.Should().Contain("d12");
        cut.Markup.Should().Contain("d10");
        cut.Markup.Should().Contain("d8");
        cut.Markup.Should().Contain("d6");
        cut.Markup.Should().Contain("d4");
    }

    [Fact]
    public void Dice_DisplaysRollResult_AfterRolling()
    {
        // Arrange
        Services.Add(new ServiceDescriptor(typeof(IDiceRoller), _mockDiceRoller.Object));
        
        var result = new DiceRollResult
        {
            Formula = "1d20",
            Total = 15,
            IndividualRolls = new[] { 15 },
            Modifier = 0,
            Timestamp = DateTime.UtcNow
        };

        _mockDiceRoller
            .Setup(d => d.Roll(It.IsAny<string>()))
            .Returns(result);

        var cut = RenderComponent<Dice>();

        // Act
        var rollButton = cut.Find("button:contains('Roll')");
        rollButton.Click();

        // Assert
        cut.Markup.Should().Contain("15");
        cut.Markup.Should().Contain("1d20");
    }

    [Fact]
    public void Dice_HighlightsCriticalHit_On20()
    {
        // Arrange
        Services.Add(new ServiceDescriptor(typeof(IDiceRoller), _mockDiceRoller.Object));
        
        var result = new DiceRollResult
        {
            Formula = "1d20",
            Total = 20,
            IndividualRolls = new[] { 20 },
            Modifier = 0,
            Timestamp = DateTime.UtcNow
        };

        _mockDiceRoller
            .Setup(d => d.Roll(It.IsAny<string>()))
            .Returns(result);

        var cut = RenderComponent<Dice>();

        // Act
        var rollButton = cut.Find("button:contains('Roll')");
        rollButton.Click();

        // Assert
        cut.Markup.Should().Contain("🎉");
        cut.Markup.Should().Contain("Critical Hit");
    }

    [Fact]
    public void Dice_HighlightsCriticalFumble_On1()
    {
        // Arrange
        Services.Add(new ServiceDescriptor(typeof(IDiceRoller), _mockDiceRoller.Object));
        
        var result = new DiceRollResult
        {
            Formula = "1d20",
            Total = 1,
            IndividualRolls = new[] { 1 },
            Modifier = 0,
            Timestamp = DateTime.UtcNow
        };

        _mockDiceRoller
            .Setup(d => d.Roll(It.IsAny<string>()))
            .Returns(result);

        var cut = RenderComponent<Dice>();

        // Act
        var rollButton = cut.Find("button:contains('Roll')");
        rollButton.Click();

        // Assert
        cut.Markup.Should().Contain("💀");
        cut.Markup.Should().Contain("Critical Fumble");
    }

    [Fact]
    public void Dice_TracksRollHistory()
    {
        // Arrange
        Services.Add(new ServiceDescriptor(typeof(IDiceRoller), _mockDiceRoller.Object));
        
        var result1 = new DiceRollResult { Formula = "1d20", Total = 15, IndividualRolls = new[] { 15 }, Modifier = 0, Timestamp = DateTime.UtcNow };
        var result2 = new DiceRollResult { Formula = "1d20", Total = 18, IndividualRolls = new[] { 18 }, Modifier = 0, Timestamp = DateTime.UtcNow };

        _mockDiceRoller.SetupSequence(d => d.Roll(It.IsAny<string>()))
            .Returns(result1)
            .Returns(result2);

        var cut = RenderComponent<Dice>();

        // Act
        var rollButton = cut.Find("button:contains('Roll')");
        rollButton.Click();
        rollButton.Click();

        // Assert
        cut.Markup.Should().Contain("Roll History");
    }

    [Fact]
    public void Dice_RollsWithAdvantage()
    {
        // Arrange
        Services.Add(new ServiceDescriptor(typeof(IDiceRoller), _mockDiceRoller.Object));
        
        var result = new DiceRollResult { Formula = "1d20", Total = 18, IndividualRolls = new[] { 18, 12 }, Modifier = 0, Timestamp = DateTime.UtcNow };

        _mockDiceRoller
            .Setup(d => d.RollWithAdvantage(It.IsAny<string>()))
            .Returns(result);

        var cut = RenderComponent<Dice>();

        // Act
        var advButton = cut.Find("button:contains('Advantage')");
        advButton.Click();

        // Assert
        _mockDiceRoller.Verify(d => d.RollWithAdvantage(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void Dice_RollsWithDisadvantage()
    {
        // Arrange
        Services.Add(new ServiceDescriptor(typeof(IDiceRoller), _mockDiceRoller.Object));
        
        var result = new DiceRollResult { Formula = "1d20", Total = 8, IndividualRolls = new[] { 8, 15 }, Modifier = 0, Timestamp = DateTime.UtcNow };

        _mockDiceRoller
            .Setup(d => d.RollWithDisadvantage(It.IsAny<string>()))
            .Returns(result);

        var cut = RenderComponent<Dice>();

        // Act
        var disadvButton = cut.Find("button:contains('Disadvantage')");
        disadvButton.Click();

        // Assert
        _mockDiceRoller.Verify(d => d.RollWithDisadvantage(It.IsAny<string>()), Times.Once);
    }
}
