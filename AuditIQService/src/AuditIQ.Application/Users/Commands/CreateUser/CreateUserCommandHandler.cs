using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using AuditIQ.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Users.Commands.CreateUser;

public sealed class CreateUserCommandHandler(IApplicationDbContext db) : ICommandHandler<CreateUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var emailTaken = await db.Users.AnyAsync(u => u.Email == command.Email, cancellationToken);
        if (emailTaken)
            return Result.Failure<Guid>(UserErrors.DuplicateEmail(command.Email));

        var teamIds = command.TeamIds.Distinct().ToList();
        var teamCount = await db.Teams.CountAsync(t => teamIds.Contains(t.Id), cancellationToken);
        if (teamCount != teamIds.Count)
            return Result.Failure<Guid>(UserErrors.TeamNotFound(teamIds[0]));

        var groupIds = command.GroupIds.Distinct().ToList();
        var groupCount = await db.Groups.CountAsync(g => groupIds.Contains(g.Id), cancellationToken);
        if (groupCount != groupIds.Count)
            return Result.Failure<Guid>(UserErrors.GroupNotFound(groupIds[0]));

        var user = new User
        {
            // No real SSO integration yet (Section 13) — admin-created users get a placeholder
            // subject id, same convention as the Scorebuddy migration's StaffLoader.
            SsoSubjectId = $"pending-sso:{Guid.NewGuid()}",
            DisplayName = command.DisplayName,
            Email = command.Email,
            Role = command.Role,
            EmploymentType = command.EmploymentType,
            IsActive = true,
        };

        foreach (var teamId in teamIds)
            user.UserTeams.Add(new UserTeam { UserId = user.Id, TeamId = teamId });

        foreach (var groupId in groupIds)
            user.UserGroups.Add(new UserGroup { UserId = user.Id, GroupId = groupId });

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
