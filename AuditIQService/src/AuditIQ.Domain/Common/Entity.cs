namespace AuditIQ.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Traceability back to the source Scorebuddy record (Epic 2 migration) — e.g. "staff:1234"
    /// or "scorecard:55:version:3:question:12". Null for rows created natively in AuditIQ.
    /// </summary>
    public string? LegacySourceId { get; set; }
}
