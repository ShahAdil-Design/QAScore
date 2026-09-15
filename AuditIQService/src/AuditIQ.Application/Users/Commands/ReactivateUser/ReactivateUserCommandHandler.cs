using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;

namespace AuditIQ.Application.Users.Commands.ReactivateUser;

public sealed class ReactivateUserCommandHandler(IApplicationDbContext db) : ICommandHandler<ReactivateUserCommand>
{
    public async Task<Result> Handle(ReactivateUserCommand command, CancellationToken cancellationToken)
    {
        var user = await db.Users.FindAsync([command.UserId], cancellationToken);
        if (user is null)
            return Result.Failure(UserErrors.NotFound(command.UserId));

        if (user.IsActive)
            return Result.Failure(UserErrors.AlreadyActive);

        user.IsActive = true;
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
