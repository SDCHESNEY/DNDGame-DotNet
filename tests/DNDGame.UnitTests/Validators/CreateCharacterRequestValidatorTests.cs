using DNDGame.Application.DTOs;
using DNDGame.Application.Validators;
using DNDGame.Core.Enums;
using DNDGame.Core.ValueObjects;
using FluentAssertions;
using Xunit;

namespace DNDGame.UnitTests.Validators;

public class CreateCharacterRequestValidatorTests
{
    private readonly CreateCharacterRequestValidator _validator;

    public CreateCharacterRequestValidatorTests()
    {
        _validator = new CreateCharacterRequestValidator();
    }

    private static CreateCharacterRequest CreateValidRequest() => new(
        1, "Test", CharacterClass.Fighter, 1,
        new AbilityScores(10, 10, 10, 10, 10, 10), 10, 10);

    [Fact]
    public async Task Validate_WithValidRequest_PassesValidation()
    {
        // Arrange
        var request = new CreateCharacterRequest(
            PlayerId: 1,
            Name: "Gandalf",
            Class: CharacterClass.Wizard,
            Level: 5,
            AbilityScores: new AbilityScores(10, 14, 12, 18, 16, 12),
            MaxHitPoints: 30,
            ArmorClass: 12,
            Skills: null,
            Inventory: null,
            PersonalityTraits: null
        );

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithEmptyName_FailsValidation()
    {
        // Arrange
        var request = new CreateCharacterRequest(
            1, "", CharacterClass.Fighter, 1,
            new AbilityScores(10, 10, 10, 10, 10, 10), 10, 10);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Name");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(21)]
    public async Task Validate_WithInvalidLevel_FailsValidation(int level)
    {
        // Arrange
        var request = new CreateCharacterRequest(
            1, "Test", CharacterClass.Fighter, level,
            new AbilityScores(10, 10, 10, 10, 10, 10), 10, 10);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Level");
    }

    [Fact]
    public async Task Validate_WithNullAbilityScores_FailsValidation()
    {
        // Arrange
        var request = new CreateCharacterRequest(
            1, "Test", CharacterClass.Cleric, 1, null!, 10, 10);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "AbilityScores");
    }

    [Theory]
    [InlineData(0, "Strength")]
    [InlineData(31, "Strength")]
    public async Task Validate_WithInvalidAbilityScore_FailsValidation(int score, string abilityName)
    {
        // Arrange
        var request = new CreateCharacterRequest(
            1, "Test", CharacterClass.Paladin, 1,
            new AbilityScores(score, 10, 10, 10, 10, 10), 10, 10);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains(abilityName));
    }

    [Fact]
    public async Task Validate_WithZeroMaxHitPoints_FailsValidation()
    {
        // Arrange
        var request = new CreateCharacterRequest(
            1, "Test", CharacterClass.Rogue, 1,
            new AbilityScores(10, 10, 10, 10, 10, 10), 0, 10);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "MaxHitPoints");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(31)]
    public async Task Validate_WithInvalidArmorClass_FailsValidation(int ac)
    {
        // Arrange
        var request = new CreateCharacterRequest(
            1, "Test", CharacterClass.Monk, 1,
            new AbilityScores(10, 10, 10, 10, 10, 10), 10, ac);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ArmorClass");
    }
}
