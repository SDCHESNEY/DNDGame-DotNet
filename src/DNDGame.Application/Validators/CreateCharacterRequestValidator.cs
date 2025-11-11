using FluentValidation;
using DNDGame.Application.DTOs;

namespace DNDGame.Application.Validators;

public class CreateCharacterRequestValidator : AbstractValidator<CreateCharacterRequest>
{
    public CreateCharacterRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Character name is required")
            .MaximumLength(200).WithMessage("Character name must not exceed 200 characters");

        RuleFor(x => x.Class)
            .IsInEnum().WithMessage("Invalid character class");

        RuleFor(x => x.Level)
            .GreaterThanOrEqualTo(1).WithMessage("Level must be at least 1")
            .LessThanOrEqualTo(20).WithMessage("Level must not exceed 20");

        RuleFor(x => x.MaxHitPoints)
            .GreaterThan(0).WithMessage("Max hit points must be greater than 0");

        RuleFor(x => x.ArmorClass)
            .GreaterThanOrEqualTo(1).WithMessage("Armor class must be at least 1")
            .LessThanOrEqualTo(30).WithMessage("Armor class must not exceed 30");

        RuleFor(x => x.AbilityScores)
            .NotNull().WithMessage("Ability scores are required");

        When(x => x.AbilityScores != null, () =>
        {
            RuleFor(x => x.AbilityScores.Strength)
                .InclusiveBetween(1, 30).WithMessage("Strength must be between 1 and 30");
            
            RuleFor(x => x.AbilityScores.Dexterity)
                .InclusiveBetween(1, 30).WithMessage("Dexterity must be between 1 and 30");
            
            RuleFor(x => x.AbilityScores.Constitution)
                .InclusiveBetween(1, 30).WithMessage("Constitution must be between 1 and 30");
            
            RuleFor(x => x.AbilityScores.Intelligence)
                .InclusiveBetween(1, 30).WithMessage("Intelligence must be between 1 and 30");
            
            RuleFor(x => x.AbilityScores.Wisdom)
                .InclusiveBetween(1, 30).WithMessage("Wisdom must be between 1 and 30");
            
            RuleFor(x => x.AbilityScores.Charisma)
                .InclusiveBetween(1, 30).WithMessage("Charisma must be between 1 and 30");
        });

        RuleFor(x => x.PersonalityTraits)
            .MaximumLength(500).WithMessage("Personality traits must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.PersonalityTraits));
    }
}
