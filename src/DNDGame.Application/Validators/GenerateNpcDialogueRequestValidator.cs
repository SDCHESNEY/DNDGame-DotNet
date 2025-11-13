using FluentValidation;
using DNDGame.Application.DTOs;

namespace DNDGame.Application.Validators;

public class GenerateNpcDialogueRequestValidator : AbstractValidator<GenerateNpcDialogueRequest>
{
    public GenerateNpcDialogueRequestValidator()
    {
        RuleFor(x => x.SessionId)
            .GreaterThan(0)
            .WithMessage("Session ID must be greater than 0");

        RuleFor(x => x.NpcName)
            .NotEmpty()
            .WithMessage("NPC name is required")
            .MaximumLength(100)
            .WithMessage("NPC name must not exceed 100 characters");

        RuleFor(x => x.Personality)
            .NotEmpty()
            .WithMessage("NPC personality is required")
            .MaximumLength(500)
            .WithMessage("NPC personality must not exceed 500 characters");

        RuleFor(x => x.PlayerMessage)
            .NotEmpty()
            .WithMessage("Player message is required")
            .MaximumLength(2000)
            .WithMessage("Player message must not exceed 2000 characters");

        RuleFor(x => x.Occupation)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.Occupation))
            .WithMessage("Occupation must not exceed 100 characters");

        RuleFor(x => x.Mood)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.Mood))
            .WithMessage("Mood must not exceed 100 characters");
    }
}
