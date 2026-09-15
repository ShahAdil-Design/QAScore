using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Pagination;
using AuditIQ.Application.Notifications.Dtos;

namespace AuditIQ.Application.Notifications.Queries.GetNotifications;

/// <summary>Always scoped to RecipientUserId — the controller sets this from the caller's own
/// identity claim, never a query-string value, so nobody can browse another user's feed.</summary>
public sealed record GetNotificationsQuery(
    Guid RecipientUserId,
    bool UnreadOnly,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<NotificationDto>>;
