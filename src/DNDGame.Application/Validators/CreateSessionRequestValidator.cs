using FluentValidation;
using DNDGame.Application.DTOs;

namespace DNDGame.Application.Validators;

public class CreateSessionRequestValidator : AbstractValidator<CreateSessionRequest>
{
    public CreateSessionRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Session title is required")
            .MaximumLength(200).WithMessage("Session title must not exceed 200 characters");

        RuleFor(x => x.Mode)
            .IsInEnum().WithMessage("Invalid session mode");
    }
}
