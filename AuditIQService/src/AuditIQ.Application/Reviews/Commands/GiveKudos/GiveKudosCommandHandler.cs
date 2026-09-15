using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using AuditIQ.Application.Abstractions.Time;
using AuditIQ.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Reviews.Commands.GiveKudos;

public sealed class GiveKudosCommandHandler(IApplicationDbContext db, IDateTimeProvider clock) : ICommandHandler<GiveKudosCommand, Guid>
{
    public async Task<Result<Guid>> Handle(GiveKudosCommand command, CancellationToken cancellationToken)
    {
        if (command.FromUserId == command.ToUserId)
            return Result.Failure<Guid>(ReviewErrors.CannotKudosSelf);

        if (!await db.Users.AnyAsync(u => u.Id == command.FromUserId, cancellationToken))
            return Result.Failure<Guid>(ReviewErrors.UserNotFound(command.FromUserId));

        if (!await db.Users.AnyAsync(u => u.Id == command.ToUserId, cancellationToken))
            return Result.Failure<Guid>(ReviewErrors.UserNotFound(command.ToUserId));

        var kudos = new Kudos
        {
            FromUserId = command.FromUserId,
            ToUserId = command.ToUserId,
            Message = command.Message,
            CreatedAt = clock.UtcNow,
        };

        db.Kudos.Add(kudos);
        await db.SaveChangesAsync(cancellationToken);

        return kudos.Id;
    }
}
