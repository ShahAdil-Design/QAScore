using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Scorecards.Commands.UnlockScorecard;

public sealed class UnlockScorecardCommandHandler(IApplicationDbContext db) : ICommandHandler<UnlockScorecardCommand>
{
    public async Task<Result> Handle(UnlockScorecardCommand command, CancellationToken cancellationToken)
    {
        var scorecard = await db.Scorecards
            .FirstOrDefaultAsync(s => s.Id == command.ScorecardId && s.IsCurrentVersion, cancellationToken);

        if (scorecard is null)
            return Result.Failure(ScorecardErrors.NotFound(command.ScorecardId));

        if (!scorecard.IsLocked)
            return Result.Failure(ScorecardErrors.NotLocked);

        scorecard.IsLocked = false;
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
