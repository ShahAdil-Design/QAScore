using AuditIQ.Application.Abstractions.Messaging;

namespace AuditIQ.Application.Notifications.Commands.MarkAllNotificationsRead;

public sealed record MarkAllNotificationsReadCommand(Guid RecipientUserId) : ICommand;
