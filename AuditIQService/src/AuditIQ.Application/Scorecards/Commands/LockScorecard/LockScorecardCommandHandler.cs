using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Scorecards.Commands.LockScorecard;

public sealed class LockScorecardCommandHandler(IApplicationDbContext db) : ICommandHandler<LockScorecardCommand>
{
    public async Task<Result> Handle(LockScorecardCommand command, CancellationToken cancellationToken)
    {
        var scorecard = await db.Scorecards
            .FirstOrDefaultAsync(s => s.Id == command.ScorecardId && s.IsCurrentVersion, cancellationToken);

        if (scorecard is null)
            return Result.Failure(ScorecardErrors.NotFound(command.ScorecardId));

        if (scorecard.IsArchived)
            return Result.Failure(ScorecardErrors.Archived(scorecard.Id));

        if (scorecard.IsLocked)
            return Result.Failure(ScorecardErrors.AlreadyLocked);

        scorecard.IsLocked = true;
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
