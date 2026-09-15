using AuditIQ.Domain.Common;

namespace AuditIQ.Domain.Entities;

/// <summary>Section 5, table 4. Managed lookup list (e.g. "Phone Call") — not free text.</summary>
public class ScorecardCategory : Entity
{
    public required string Name { get; set; }
}
