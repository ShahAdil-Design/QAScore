using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Users.Commands.UpdateUser;

public sealed class UpdateUserCommandHandler(IApplicationDbContext db) : ICommandHandler<UpdateUserCommand>
{
    public async Task<Result> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        var user = await db.Users.FindAsync([command.UserId], cancellationToken);
        if (user is null)
            return Result.Failure(UserErrors.NotFound(command.UserId));

        var emailTaken = await db.Users.AnyAsync(u => u.Id != command.UserId && u.Email == command.Email, cancellationToken);
        if (emailTaken)
            return Result.Failure(UserErrors.DuplicateEmail(command.Email));

        user.DisplayName = command.DisplayName;
        user.Email = command.Email;
        user.Role = command.Role;
        user.EmploymentType = command.EmploymentType;
        user.Notes = command.Notes;

        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
