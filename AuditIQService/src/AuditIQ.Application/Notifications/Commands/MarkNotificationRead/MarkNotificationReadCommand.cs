using AuditIQ.Application.Abstractions.Messaging;

namespace AuditIQ.Application.Notifications.Commands.MarkNotificationRead;

public sealed record MarkNotificationReadCommand(Guid NotificationId, Guid RecipientUserId) : ICommand;
