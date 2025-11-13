using FluentValidation;
using DNDGame.Application.DTOs;

namespace DNDGame.Application.Validators;

public class GenerateDmResponseRequestValidator : AbstractValidator<GenerateDmResponseRequest>
{
    public GenerateDmResponseRequestValidator()
    {
        RuleFor(x => x.SessionId)
            .GreaterThan(0)
            .WithMessage("Session ID must be greater than 0");

        RuleFor(x => x.PlayerMessage)
            .NotEmpty()
            .WithMessage("Player message is required")
            .MaximumLength(5000)
            .WithMessage("Player message must not exceed 5000 characters");

        RuleFor(x => x.CharacterId)
            .GreaterThan(0)
            .When(x => x.CharacterId.HasValue)
            .WithMessage("Character ID must be greater than 0");
    }
}
