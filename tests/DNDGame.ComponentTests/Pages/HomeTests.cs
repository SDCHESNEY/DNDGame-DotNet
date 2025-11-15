using Bunit;
using DNDGame.Web.Components.Pages;
using Xunit;
using FluentAssertions;

namespace DNDGame.ComponentTests.Pages;

public class HomeTests : TestContext
{
    [Fact]
    public void Home_Renders_Successfully()
    {
        // Act
        var cut = RenderComponent<Home>();

        // Assert
        cut.Should().NotBeNull();
    }

    [Fact]
    public void Home_DisplaysHeroSection()
    {
        // Act
        var cut = RenderComponent<Home>();

        // Assert
        cut.Find("h1").TextContent.Should().Contain("🎲 DND Game");
    }

    [Fact]
    public void Home_DisplaysFeatureCards()
    {
        // Act
        var cut = RenderComponent<Home>();

        // Assert
        var cards = cut.FindAll(".card");
        cards.Count.Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public void Home_HasCharactersLink()
    {
        // Act
        var cut = RenderComponent<Home>();

        // Assert
        var link = cut.Find("a[href='/characters']");
        link.Should().NotBeNull();
        link.TextContent.Should().Contain("View Characters");
    }

    [Fact]
    public void Home_HasSessionsLink()
    {
        // Act
        var cut = RenderComponent<Home>();

        // Assert
        var link = cut.Find("a[href='/sessions']");
        link.Should().NotBeNull();
        link.TextContent.Should().Contain("Join Sessions");
    }

    [Fact]
    public void Home_HasDiceRollerLink()
    {
        // Act
        var cut = RenderComponent<Home>();

        // Assert
        var link = cut.Find("a[href='/dice']");
        link.Should().NotBeNull();
        link.TextContent.Should().Contain("Roll Dice");
    }

    [Fact]
    public void Home_DisplaysFeaturesList()
    {
        // Act
        var cut = RenderComponent<Home>();

        // Assert
        var features = cut.FindAll(".feature-list li");
        features.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Home_HasWelcomeMessage()
    {
        // Act
        var cut = RenderComponent<Home>();

        // Assert
        cut.Markup.Should().Contain("Welcome");
    }
}
