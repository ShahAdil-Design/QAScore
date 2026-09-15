using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using AuditIQ.Application.Users.Dtos;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Users.Queries.GetUsers;

public sealed class GetUsersQueryHandler(IApplicationDbContext db) : IQueryHandler<GetUsersQuery, IReadOnlyList<UserSummaryDto>>
{
    public async Task<Result<IReadOnlyList<UserSummaryDto>>> Handle(GetUsersQuery query, CancellationToken cancellationToken)
    {
        var users = db.Users.Where(u => !u.IsDeleted).AsQueryable();

        if (!query.IncludeInactive)
            users = users.Where(u => u.IsActive);

        if (query.Role is { } role)
            users = users.Where(u => u.Role == role);

        if (query.GroupId is { } groupId)
            users = users.Where(u => u.UserGroups.Any(ug => ug.GroupId == groupId));

        var results = await users
            .OrderBy(u => u.DisplayName)
            .Select(u => new UserSummaryDto(u.Id, u.DisplayName, u.Email, u.Role, u.IsActive))
            .ToListAsync(cancellationToken);

        return results;
    }
}
