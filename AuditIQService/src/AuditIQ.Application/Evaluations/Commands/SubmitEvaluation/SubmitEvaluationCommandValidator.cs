using FluentValidation;

namespace AuditIQ.Application.Evaluations.Commands.SubmitEvaluation;

public sealed class SubmitEvaluationCommandValidator : AbstractValidator<SubmitEvaluationCommand>
{
    public SubmitEvaluationCommandValidator()
    {
        RuleFor(c => c.EvaluationId).NotEmpty();
        RuleFor(c => c.EvaluatorNotes).MaximumLength(4000);
    }
}
