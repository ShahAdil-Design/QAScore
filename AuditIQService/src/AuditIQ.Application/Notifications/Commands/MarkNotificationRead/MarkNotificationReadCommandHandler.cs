using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Notifications.Commands.MarkNotificationRead;

public sealed class MarkNotificationReadCommandHandler(IApplicationDbContext db) : ICommandHandler<MarkNotificationReadCommand>
{
    public async Task<Result> Handle(MarkNotificationReadCommand command, CancellationToken cancellationToken)
    {
        var notification = await db.Notifications.FirstOrDefaultAsync(n => n.Id == command.NotificationId, cancellationToken);

        if (notification is null)
            return Result.Failure(NotificationErrors.NotFound(command.NotificationId));
        if (notification.RecipientUserId != command.RecipientUserId)
            return Result.Failure(NotificationErrors.RecipientMismatch);

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            await db.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}
