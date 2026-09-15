using FluentValidation;

namespace AuditIQ.Application.Evaluations.Commands.DisputeEvaluation;

public sealed class DisputeEvaluationCommandValidator : AbstractValidator<DisputeEvaluationCommand>
{
    public DisputeEvaluationCommandValidator()
    {
        RuleFor(c => c.EvaluationId).NotEmpty();
        RuleFor(c => c.RequestingAgentId).NotEmpty();
        RuleFor(c => c.DisputeReason).NotEmpty().MaximumLength(2000);
    }
}
