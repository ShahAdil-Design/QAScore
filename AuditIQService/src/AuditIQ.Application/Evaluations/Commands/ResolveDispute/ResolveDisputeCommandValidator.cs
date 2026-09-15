using FluentValidation;

namespace AuditIQ.Application.Evaluations.Commands.ResolveDispute;

public sealed class ResolveDisputeCommandValidator : AbstractValidator<ResolveDisputeCommand>
{
    public ResolveDisputeCommandValidator()
    {
        RuleFor(c => c.EvaluationId).NotEmpty();
        RuleFor(c => c.ResolutionNotes).NotEmpty().MaximumLength(2000);
    }
}
