using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using AuditIQ.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Lookups.Commands.CreateCauseCode;

public sealed class CreateCauseCodeCommandHandler(IApplicationDbContext db) : ICommandHandler<CreateCauseCodeCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateCauseCodeCommand command, CancellationToken cancellationToken)
    {
        var exists = await db.CauseCodes.AnyAsync(c => c.Text == command.Text, cancellationToken);
        if (exists)
            return Result.Failure<Guid>(LookupErrors.DuplicateCauseCodeText(command.Text));

        var causeCode = new CauseCode { Text = command.Text };

        db.CauseCodes.Add(causeCode);
        await db.SaveChangesAsync(cancellationToken);

        return causeCode.Id;
    }
}
