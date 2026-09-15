using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Pagination;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using AuditIQ.Application.Notifications.Dtos;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Notifications.Queries.GetNotifications;

public sealed class GetNotificationsQueryHandler(IApplicationDbContext db)
    : IQueryHandler<GetNotificationsQuery, PagedResult<NotificationDto>>
{
    public async Task<Result<PagedResult<NotificationDto>>> Handle(GetNotificationsQuery query, CancellationToken cancellationToken)
    {
        var notifications = db.Notifications.Where(n => n.RecipientUserId == query.RecipientUserId);

        if (query.UnreadOnly)
            notifications = notifications.Where(n => !n.IsRead);

        var totalCount = await notifications.CountAsync(cancellationToken);

        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var items = await notifications
            .OrderByDescending(n => n.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new NotificationDto(
                n.Id,
                n.Type,
                n.Message,
                n.RelatedEvaluationId,
                n.RelatedCalibrationListId,
                n.CreatedAtUtc,
                n.IsRead))
            .ToListAsync(cancellationToken);

        return new PagedResult<NotificationDto>(items, page, pageSize, totalCount);
    }
}
