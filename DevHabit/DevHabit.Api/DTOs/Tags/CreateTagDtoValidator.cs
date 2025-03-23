using FluentValidation;

namespace DevHabit.Api.DTOs.Tags;

public sealed class CreateTagDtoValidator : AbstractValidator<CreateTagDto>
{
    public CreateTagDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(3);

        RuleFor(x => x.Description)
            .MaximumLength(50)
            .When(x => x.Description is not null)
            .WithMessage("Description must be less than 50 characters.");
    }
}
