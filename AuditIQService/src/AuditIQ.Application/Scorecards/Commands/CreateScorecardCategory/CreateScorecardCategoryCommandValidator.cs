using FluentValidation;

namespace AuditIQ.Application.Scorecards.Commands.CreateScorecardCategory;

public sealed class CreateScorecardCategoryCommandValidator : AbstractValidator<CreateScorecardCategoryCommand>
{
    public CreateScorecardCategoryCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(256);
    }
}
