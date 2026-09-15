using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Scorecards.Commands.ArchiveScorecard;

public sealed class ArchiveScorecardCommandHandler(IApplicationDbContext db) : ICommandHandler<ArchiveScorecardCommand>
{
    public async Task<Result> Handle(ArchiveScorecardCommand command, CancellationToken cancellationToken)
    {
        var scorecard = await db.Scorecards
            .FirstOrDefaultAsync(s => s.Id == command.ScorecardId && s.IsCurrentVersion, cancellationToken);

        if (scorecard is null)
            return Result.Failure(ScorecardErrors.NotFound(command.ScorecardId));

        if (scorecard.IsArchived)
            return Result.Failure(ScorecardErrors.AlreadyArchived);

        scorecard.IsArchived = true;
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
