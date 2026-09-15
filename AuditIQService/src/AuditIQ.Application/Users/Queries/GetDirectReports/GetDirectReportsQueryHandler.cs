using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using AuditIQ.Application.Users.Dtos;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Users.Queries.GetDirectReports;

public sealed class GetDirectReportsQueryHandler(IApplicationDbContext db) : IQueryHandler<GetDirectReportsQuery, IReadOnlyList<UserSummaryDto>>
{
    public async Task<Result<IReadOnlyList<UserSummaryDto>>> Handle(GetDirectReportsQuery query, CancellationToken cancellationToken)
    {
        var results = await db.Users
            .Where(u => u.SupervisorId == query.SupervisorId && !u.IsDeleted)
            .OrderBy(u => u.DisplayName)
            .Select(u => new UserSummaryDto(u.Id, u.DisplayName, u.Email, u.Role, u.IsActive))
            .ToListAsync(cancellationToken);

        return results;
    }
}
