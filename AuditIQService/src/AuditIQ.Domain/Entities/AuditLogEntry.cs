using AuditIQ.Domain.Common;

namespace AuditIQ.Domain.Entities;

public enum AuditAction
{
    Created,
    Updated,
    Deleted,
}

/// <summary>One row per Add/Update/Delete of an IAuditableEntity, written automatically by
/// AuditLoggingInterceptor — never constructed directly by command handlers. Changes is a JSON
/// blob: {PropertyName: {Old, New}} for Updated, or a flat property snapshot for Created/Deleted.</summary>
public class AuditLogEntry : Entity
{
    public DateTimeOffset TimestampUtc { get; init; } = DateTimeOffset.UtcNow;

    // Null when the change wasn't attributable to a signed-in user (e.g. a background job,
    // or DevAuthHandler's placeholder token, which carries no auditiq:user_id claim).
    public Guid? UserId { get; init; }

    public required string EntityName { get; init; }
    public required string EntityId { get; init; }
    public required AuditAction Action { get; init; }
    public string? Changes { get; init; }
}
