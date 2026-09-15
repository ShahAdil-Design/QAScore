using AuditIQ.Domain.Common;

namespace AuditIQ.Domain.Entities;

/// <summary>Section 5, table 11. E.g. "Manage Collections" -&gt; "Payment Plans".</summary>
public class EventSubType : Entity
{
    public required Guid EventTypeId { get; set; }
    public EventType? EventType { get; init; }

    public required string Name { get; set; }
}
