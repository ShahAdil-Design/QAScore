using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;

namespace AuditIQ.Application.Lookups.Commands.DeleteCauseCode;

public sealed class DeleteCauseCodeCommandHandler(IApplicationDbContext db) : ICommandHandler<DeleteCauseCodeCommand>
{
    public async Task<Result> Handle(DeleteCauseCodeCommand command, CancellationToken cancellationToken)
    {
        var causeCode = await db.CauseCodes.FindAsync([command.Id], cancellationToken);
        if (causeCode is null)
            return Result.Failure(LookupErrors.CauseCodeNotFound(command.Id));

        db.CauseCodes.Remove(causeCode);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
