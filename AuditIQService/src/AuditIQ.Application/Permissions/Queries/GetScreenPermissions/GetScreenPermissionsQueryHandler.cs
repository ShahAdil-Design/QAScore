using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using AuditIQ.Application.Permissions.Dtos;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Permissions.Queries.GetScreenPermissions;

public sealed class GetScreenPermissionsQueryHandler(IApplicationDbContext db)
    : IQueryHandler<GetScreenPermissionsQuery, IReadOnlyList<ScreenPermissionDto>>
{
    public async Task<Result<IReadOnlyList<ScreenPermissionDto>>> Handle(GetScreenPermissionsQuery query, CancellationToken cancellationToken)
    {
        var results = await db.ScreenPermissions
            .OrderBy(p => p.ScreenKey).ThenBy(p => p.Role)
            .Select(p => new ScreenPermissionDto(p.Role, p.ScreenKey, p.IsVisible))
            .ToListAsync(cancellationToken);

        return results;
    }
}
