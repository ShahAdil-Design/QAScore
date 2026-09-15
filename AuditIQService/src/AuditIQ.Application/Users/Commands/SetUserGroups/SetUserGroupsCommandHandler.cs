using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using AuditIQ.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Users.Commands.SetUserGroups;

public sealed class SetUserGroupsCommandHandler(IApplicationDbContext db) : ICommandHandler<SetUserGroupsCommand>
{
    public async Task<Result> Handle(SetUserGroupsCommand command, CancellationToken cancellationToken)
    {
        var user = await db.Users.Include(u => u.UserGroups).FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);
        if (user is null)
            return Result.Failure(UserErrors.NotFound(command.UserId));

        var groupIds = command.GroupIds.Distinct().ToList();
        var groupCount = await db.Groups.CountAsync(g => groupIds.Contains(g.Id), cancellationToken);
        if (groupCount != groupIds.Count)
            return Result.Failure(UserErrors.GroupNotFound(groupIds[0]));

        user.UserGroups.Clear();
        foreach (var groupId in groupIds)
            user.UserGroups.Add(new UserGroup { UserId = user.Id, GroupId = groupId });

        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
