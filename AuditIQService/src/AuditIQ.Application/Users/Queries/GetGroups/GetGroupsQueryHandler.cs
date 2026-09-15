using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using AuditIQ.Application.Users.Dtos;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Users.Queries.GetGroups;

public sealed class GetGroupsQueryHandler(IApplicationDbContext db) : IQueryHandler<GetGroupsQuery, IReadOnlyList<GroupDto>>
{
    public async Task<Result<IReadOnlyList<GroupDto>>> Handle(GetGroupsQuery query, CancellationToken cancellationToken)
    {
        var groups = await db.Groups
            .OrderBy(g => g.Name)
            .Select(g => new GroupDto(g.Id, g.Name))
            .ToListAsync(cancellationToken);

        return groups;
    }
}
