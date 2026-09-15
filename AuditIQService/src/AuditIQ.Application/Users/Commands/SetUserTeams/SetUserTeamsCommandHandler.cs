using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using AuditIQ.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Users.Commands.SetUserTeams;

public sealed class SetUserTeamsCommandHandler(IApplicationDbContext db) : ICommandHandler<SetUserTeamsCommand>
{
    public async Task<Result> Handle(SetUserTeamsCommand command, CancellationToken cancellationToken)
    {
        var user = await db.Users.Include(u => u.UserTeams).FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);
        if (user is null)
            return Result.Failure(UserErrors.NotFound(command.UserId));

        var teamIds = command.TeamIds.Distinct().ToList();
        var teamCount = await db.Teams.CountAsync(t => teamIds.Contains(t.Id), cancellationToken);
        if (teamCount != teamIds.Count)
            return Result.Failure(UserErrors.TeamNotFound(teamIds[0]));

        user.UserTeams.Clear();
        foreach (var teamId in teamIds)
            user.UserTeams.Add(new UserTeam { UserId = user.Id, TeamId = teamId });

        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
