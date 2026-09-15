using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Notifications.Commands.MarkAllNotificationsRead;

public sealed class MarkAllNotificationsReadCommandHandler(IApplicationDbContext db) : ICommandHandler<MarkAllNotificationsReadCommand>
{
    public async Task<Result> Handle(MarkAllNotificationsReadCommand command, CancellationToken cancellationToken)
    {
        await db.Notifications
            .Where(n => n.RecipientUserId == command.RecipientUserId && !n.IsRead)
            .ExecuteUpdateAsync(setters => setters.SetProperty(n => n.IsRead, true), cancellationToken);

        return Result.Success();
    }
}
