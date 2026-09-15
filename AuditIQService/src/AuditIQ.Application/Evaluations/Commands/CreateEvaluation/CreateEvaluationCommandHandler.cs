using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using AuditIQ.Domain.Entities;
using AuditIQ.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Evaluations.Commands.CreateEvaluation;

public sealed class CreateEvaluationCommandHandler(IApplicationDbContext db) : ICommandHandler<CreateEvaluationCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateEvaluationCommand command, CancellationToken cancellationToken)
    {
        var scorecard = await db.Scorecards.FindAsync([command.ScorecardId], cancellationToken);
        if (scorecard is null)
            return Result.Failure<Guid>(EvaluationErrors.ScorecardNotFound(command.ScorecardId));
        if (scorecard.IsArchived)
            return Result.Failure<Guid>(EvaluationErrors.ScorecardArchived(command.ScorecardId));

        if (!await db.Users.AnyAsync(u => u.Id == command.AgentId, cancellationToken))
            return Result.Failure<Guid>(EvaluationErrors.AgentNotFound(command.AgentId));

        if (!await db.Users.AnyAsync(u => u.Id == command.EvaluatorId, cancellationToken))
            return Result.Failure<Guid>(EvaluationErrors.EvaluatorNotFound(command.EvaluatorId));

        if (command.EventTypeId is { } eventTypeId)
        {
            if (!await db.EventTypes.AnyAsync(t => t.Id == eventTypeId && t.ScorecardId == command.ScorecardId, cancellationToken))
                return Result.Failure<Guid>(EvaluationErrors.EventTypeNotOnScorecard(eventTypeId));

            if (command.EventSubTypeId is { } eventSubTypeId
                && !await db.EventSubTypes.AnyAsync(s => s.Id == eventSubTypeId && s.EventTypeId == eventTypeId, cancellationToken))
                return Result.Failure<Guid>(EvaluationErrors.EventSubTypeNotOnEventType(eventSubTypeId));
        }

        var evaluation = new Evaluation
        {
            ScorecardId = command.ScorecardId,
            AgentId = command.AgentId,
            EvaluatorId = command.EvaluatorId,
            EventTypeId = command.EventTypeId,
            EventSubTypeId = command.EventSubTypeId,
            Reference = command.Reference,
            Status = EvaluationStatus.Draft,
            EventOccurredAt = command.EventOccurredAt,
            EventDurationSeconds = command.EventDurationSeconds,
        };

        db.Evaluations.Add(evaluation);
        await db.SaveChangesAsync(cancellationToken);

        return evaluation.Id;
    }
}
