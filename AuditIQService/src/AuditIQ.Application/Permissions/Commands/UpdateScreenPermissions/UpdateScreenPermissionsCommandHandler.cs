using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using AuditIQ.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Permissions.Commands.UpdateScreenPermissions;

public sealed class UpdateScreenPermissionsCommandHandler(IApplicationDbContext db) : ICommandHandler<UpdateScreenPermissionsCommand>
{
    public async Task<Result> Handle(UpdateScreenPermissionsCommand command, CancellationToken cancellationToken)
    {
        var existing = await db.ScreenPermissions.ToListAsync(cancellationToken);
        var existingByKey = existing.ToDictionary(p => (p.Role, p.ScreenKey));
        var incomingKeys = command.Permissions.Select(p => (p.Role, p.ScreenKey)).ToHashSet();

        foreach (var input in command.Permissions)
        {
            if (existingByKey.TryGetValue((input.Role, input.ScreenKey), out var row))
            {
                row.IsVisible = input.IsVisible;
            }
            else
            {
                db.ScreenPermissions.Add(new ScreenPermission
                {
                    Role = input.Role,
                    ScreenKey = input.ScreenKey,
                    IsVisible = input.IsVisible,
                });
            }
        }

        // Anything not present in this full-replacement submission is removed — keeps the table
        // from accumulating stale rows for a role/screen combo the admin panel stopped sending.
        foreach (var row in existing)
        {
            if (!incomingKeys.Contains((row.Role, row.ScreenKey)))
                db.ScreenPermissions.Remove(row);
        }

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
