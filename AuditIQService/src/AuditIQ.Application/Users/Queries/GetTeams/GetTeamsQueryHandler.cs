using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using AuditIQ.Application.Users.Dtos;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Users.Queries.GetTeams;

public sealed class GetTeamsQueryHandler(IApplicationDbContext db) : IQueryHandler<GetTeamsQuery, IReadOnlyList<TeamDto>>
{
    public async Task<Result<IReadOnlyList<TeamDto>>> Handle(GetTeamsQuery query, CancellationToken cancellationToken)
    {
        var teams = db.Teams.Include(t => t.Group).AsQueryable();

        if (query.GroupId is { } groupId)
            teams = teams.Where(t => t.GroupId == groupId);

        var results = await teams
            .OrderBy(t => t.Name)
            .Select(t => new TeamDto(t.Id, t.Name, t.GroupId, t.Group!.Name))
            .ToListAsync(cancellationToken);

        return results;
    }
}
