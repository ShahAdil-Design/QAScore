using FluentValidation;

namespace AuditIQ.Application.Scorecards.Commands.UpdateScorecardCategory;

public sealed class UpdateScorecardCategoryCommandValidator : AbstractValidator<UpdateScorecardCategoryCommand>
{
    public UpdateScorecardCategoryCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(256);
    }
}
