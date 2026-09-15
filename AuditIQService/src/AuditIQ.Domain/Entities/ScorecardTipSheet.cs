using AuditIQ.Domain.Common;

namespace AuditIQ.Domain.Entities;

/// <summary>Section 5, table 7. Per-scorecard evaluator guidance doc.</summary>
public class ScorecardTipSheet : Entity
{
    public required Guid ScorecardId { get; set; }
    public Scorecard? Scorecard { get; init; }

    public required string Title { get; set; }
    public required string Content { get; set; }
}
