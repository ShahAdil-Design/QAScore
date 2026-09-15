using AuditIQ.Application.Abstractions.Messaging;

namespace AuditIQ.Application.Notifications.Queries.GetUnreadNotificationCount;

public sealed record GetUnreadNotificationCountQuery(Guid RecipientUserId) : IQuery<int>;
