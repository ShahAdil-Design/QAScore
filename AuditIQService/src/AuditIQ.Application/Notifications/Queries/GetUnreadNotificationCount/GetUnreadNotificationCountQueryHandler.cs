using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Notifications.Queries.GetUnreadNotificationCount;

public sealed class GetUnreadNotificationCountQueryHandler(IApplicationDbContext db)
    : IQueryHandler<GetUnreadNotificationCountQuery, int>
{
    public async Task<Result<int>> Handle(GetUnreadNotificationCountQuery query, CancellationToken cancellationToken)
    {
        var count = await db.Notifications
            .CountAsync(n => n.RecipientUserId == query.RecipientUserId && !n.IsRead, cancellationToken);

        return count;
    }
}
