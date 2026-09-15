using System.Text.Json;
using AuditIQ.Application.Abstractions.Auth;
using AuditIQ.Domain.Common;
using AuditIQ.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AuditIQ.Infrastructure.Persistence;

/// <summary>
/// Writes an AuditLogEntry for every Add/Update/Delete of an IAuditableEntity, as part of the
/// same SaveChanges call that made the change — this is the only place "who did this" is
/// captured, so command handlers never need to remember to log anything themselves.
/// </summary>
public sealed class AuditLoggingInterceptor(ICurrentUserService currentUser) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
            AddAuditEntries(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
            AddAuditEntries(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void AddAuditEntries(DbContext context)
    {
        var entries = new List<AuditLogEntry>();

        foreach (var entry in context.ChangeTracker.Entries<IAuditableEntity>())
        {
            var action = entry.State switch
            {
                EntityState.Added => AuditAction.Created,
                EntityState.Deleted => AuditAction.Deleted,
                EntityState.Modified => AuditAction.Updated,
                _ => (AuditAction?)null,
            };

            if (action is null)
                continue;

            var changes = action == AuditAction.Updated
                ? SerializeModifiedProperties(entry)
                : SerializeAllProperties(entry, useOriginalValues: action == AuditAction.Deleted);

            // A Modified entry with no actual scalar changes (e.g. only a navigation fixup) has
            // nothing worth recording.
            if (changes is null)
                continue;

            entries.Add(new AuditLogEntry
            {
                UserId = currentUser.UserId,
                EntityName = entry.Entity.GetType().Name,
                EntityId = entry.Property(nameof(Entity.Id)).CurrentValue?.ToString() ?? "",
                Action = action.Value,
                Changes = changes,
            });
        }

        if (entries.Count > 0)
            context.Set<AuditLogEntry>().AddRange(entries);
    }

    private static string? SerializeModifiedProperties(EntityEntry entry)
    {
        var changed = entry.Properties
            .Where(p => p.IsModified && !Equals(p.OriginalValue, p.CurrentValue))
            .ToDictionary(p => p.Metadata.Name, p => new { Old = p.OriginalValue, New = p.CurrentValue });

        return changed.Count == 0 ? null : JsonSerializer.Serialize(changed);
    }

    private static string SerializeAllProperties(EntityEntry entry, bool useOriginalValues)
    {
        var values = entry.Properties.ToDictionary(
            p => p.Metadata.Name,
            p => useOriginalValues ? p.OriginalValue : p.CurrentValue);

        return JsonSerializer.Serialize(values);
    }
}
