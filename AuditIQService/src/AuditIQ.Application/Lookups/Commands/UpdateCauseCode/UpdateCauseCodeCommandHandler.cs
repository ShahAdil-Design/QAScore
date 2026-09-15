using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Lookups.Commands.UpdateCauseCode;

public sealed class UpdateCauseCodeCommandHandler(IApplicationDbContext db) : ICommandHandler<UpdateCauseCodeCommand>
{
    public async Task<Result> Handle(UpdateCauseCodeCommand command, CancellationToken cancellationToken)
    {
        var causeCode = await db.CauseCodes.FindAsync([command.Id], cancellationToken);
        if (causeCode is null)
            return Result.Failure(LookupErrors.CauseCodeNotFound(command.Id));

        var textTaken = await db.CauseCodes.AnyAsync(c => c.Id != command.Id && c.Text == command.Text, cancellationToken);
        if (textTaken)
            return Result.Failure(LookupErrors.DuplicateCauseCodeText(command.Text));

        causeCode.Text = command.Text;
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
