using AuditIQ.Application.Abstractions.Results;

namespace AuditIQ.Application.Notifications;

public static class NotificationErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("Notification.NotFound", $"Notification '{id}' was not found.");

    public static readonly Error RecipientMismatch =
        Error.Forbidden("Notification.RecipientMismatch", "You can only mark your own notifications as read.");
}
