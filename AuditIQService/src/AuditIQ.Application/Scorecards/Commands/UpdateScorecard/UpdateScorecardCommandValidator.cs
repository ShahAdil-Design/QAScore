using FluentValidation;

namespace AuditIQ.Application.Scorecards.Commands.UpdateScorecard;

public sealed class UpdateScorecardCommandValidator : AbstractValidator<UpdateScorecardCommand>
{
    public UpdateScorecardCommandValidator()
    {
        RuleFor(c => c.ScorecardId).NotEmpty();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(256);
        RuleFor(c => c.Description).MaximumLength(2000);
        RuleFor(c => c.ScorecardType).NotEmpty().MaximumLength(64);
        RuleFor(c => c.CategoryId).NotEmpty();
        RuleFor(c => c.Location).MaximumLength(256);
        RuleFor(c => c.TargetPercentage).InclusiveBetween(0, 100).When(c => c.TargetPercentage is not null);
        RuleFor(c => c.MaxScore).GreaterThan(0);

        // Empty is allowed — you can save a scorecard with no questions yet and add them later.
        RuleForEach(c => c.Questions).ChildRules(question =>
        {
            question.RuleFor(q => q.SectionName).NotEmpty().MaximumLength(256);
            question.RuleFor(q => q.Text).NotEmpty().MaximumLength(1000);
            // 0 is legitimate — real migrated Scorebuddy scorecards use 0-weight questions for
            // pure diagnostic/tagging purposes (e.g. "root cause raised") that must stay on the
            // scorecard but never contribute to scoring; ScoreCalculator already treats a 0
            // weight as inert (zero in both numerator and denominator), so only reject negative.
            question.RuleFor(q => q.Weight).GreaterThanOrEqualTo(0);
            question.RuleFor(q => q.AnswerOptions).NotEmpty().WithMessage("Each question must have at least one answer option.");

            question.RuleForEach(q => q.AnswerOptions).ChildRules(answer =>
            {
                answer.RuleFor(a => a.Label).NotEmpty().MaximumLength(256);
                answer.RuleFor(a => a.Value)
                    .Equal(0)
                    .When(a => a.IsFailSection || a.IsFailAll || a.IsNotApplicable)
                    .WithMessage("A Fail Section, Fail All, or N/A answer must have a value of zero.");
            });
        });
    }
}
