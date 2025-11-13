using FluentValidation;
using DNDGame.Application.DTOs;

namespace DNDGame.Application.Validators;

public class GenerateSceneDescriptionRequestValidator : AbstractValidator<GenerateSceneDescriptionRequest>
{
    public GenerateSceneDescriptionRequestValidator()
    {
        RuleFor(x => x.SessionId)
            .GreaterThan(0)
            .WithMessage("Session ID must be greater than 0");

        RuleFor(x => x.LocationName)
            .NotEmpty()
            .WithMessage("Location name is required")
            .MaximumLength(200)
            .WithMessage("Location name must not exceed 200 characters");

        RuleFor(x => x.LocationType)
            .NotEmpty()
            .WithMessage("Location type is required")
            .MaximumLength(100)
            .WithMessage("Location type must not exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Description must not exceed 1000 characters");

        RuleFor(x => x.Features)
            .Must(features => features == null || features.Count <= 20)
            .WithMessage("Features list must not exceed 20 items");

        RuleFor(x => x.NpcsPresent)
            .Must(npcs => npcs == null || npcs.Count <= 20)
            .WithMessage("NPCs present list must not exceed 20 items");
    }
}
