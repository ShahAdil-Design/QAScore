namespace AuditIQ.Domain.Common;

/// <summary>Marker for entities whose Add/Update/Delete should be recorded by
/// AuditLoggingInterceptor as an AuditLogEntry. Add this to any entity worth tracking
/// "who did this" for — start narrow (User, Scorecard) rather than every entity, since
/// every property change on a marked entity is serialized into the log.</summary>
public interface IAuditableEntity
{
}
