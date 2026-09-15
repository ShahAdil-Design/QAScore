using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using AuditIQ.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Evaluations.Commands.DisputeEvaluation;

public sealed class DisputeEvaluationCommandHandler(IApplicationDbContext db) : ICommandHandler<DisputeEvaluationCommand>
{
    public async Task<Result> Handle(DisputeEvaluationCommand command, CancellationToken cancellationToken)
    {
        var evaluation = await db.Evaluations.FirstOrDefaultAsync(e => e.Id == command.EvaluationId, cancellationToken);

        if (evaluation is null)
            return Result.Failure(EvaluationErrors.NotFound(command.EvaluationId));
        if (evaluation.AgentId != command.RequestingAgentId)
            return Result.Failure(EvaluationErrors.AgentMismatch);
        if (evaluation.Status != EvaluationStatus.Submitted)
            return Result.Failure(EvaluationErrors.NotSubmitted);

        evaluation.Status = EvaluationStatus.Disputed;
        evaluation.DisputeReason = command.DisputeReason;
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
