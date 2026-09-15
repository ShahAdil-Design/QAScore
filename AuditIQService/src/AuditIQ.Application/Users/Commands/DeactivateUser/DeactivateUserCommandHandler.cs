using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;

namespace AuditIQ.Application.Users.Commands.DeactivateUser;

public sealed class DeactivateUserCommandHandler(IApplicationDbContext db) : ICommandHandler<DeactivateUserCommand>
{
    public async Task<Result> Handle(DeactivateUserCommand command, CancellationToken cancellationToken)
    {
        var user = await db.Users.FindAsync([command.UserId], cancellationToken);
        if (user is null)
            return Result.Failure(UserErrors.NotFound(command.UserId));

        if (!user.IsActive)
            return Result.Failure(UserErrors.AlreadyDeactivated);

        user.IsActive = false;
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
