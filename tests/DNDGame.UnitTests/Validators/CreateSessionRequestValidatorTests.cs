using DNDGame.Application.DTOs;
using DNDGame.Application.Validators;
using DNDGame.Core.Enums;
using FluentAssertions;
using Xunit;

namespace DNDGame.UnitTests.Validators;

public class CreateSessionRequestValidatorTests
{
    private readonly CreateSessionRequestValidator _validator;

    public CreateSessionRequestValidatorTests()
    {
        _validator = new CreateSessionRequestValidator();
    }

    [Fact]
    public async Task Validate_WithValidRequest_PassesValidation()
    {
        // Arrange
        var request = new CreateSessionRequest(
            Title: "Epic Adventure",
            Mode: SessionMode.Multiplayer
        );

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_WithEmptyOrNullTitle_FailsValidation(string? title)
    {
        // Arrange
        var request = new CreateSessionRequest(
            Title: title!,
            Mode: SessionMode.Solo
        );

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Title");
    }

    [Fact]
    public async Task Validate_WithTooLongTitle_FailsValidation()
    {
        // Arrange
        var longTitle = new string('A', 201);
        var request = new CreateSessionRequest(
            Title: longTitle,
            Mode: SessionMode.Multiplayer
        );

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title" && e.ErrorMessage.Contains("200"));
    }

    [Theory]
    [InlineData(SessionMode.Solo)]
    [InlineData(SessionMode.Multiplayer)]
    public async Task Validate_WithValidMode_PassesValidation(SessionMode mode)
    {
        // Arrange
        var request = new CreateSessionRequest(
            Title: "Test Session",
            Mode: mode
        );

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
