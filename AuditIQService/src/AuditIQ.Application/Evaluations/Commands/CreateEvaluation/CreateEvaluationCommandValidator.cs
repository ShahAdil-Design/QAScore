using FluentValidation;

namespace AuditIQ.Application.Evaluations.Commands.CreateEvaluation;

public sealed class CreateEvaluationCommandValidator : AbstractValidator<CreateEvaluationCommand>
{
    public CreateEvaluationCommandValidator()
    {
        RuleFor(c => c.ScorecardId).NotEmpty();
        RuleFor(c => c.AgentId).NotEmpty();
        RuleFor(c => c.EvaluatorId).NotEmpty();
        RuleFor(c => c.EventDurationSeconds).GreaterThan(0).When(c => c.EventDurationSeconds.HasValue);
    }
}
