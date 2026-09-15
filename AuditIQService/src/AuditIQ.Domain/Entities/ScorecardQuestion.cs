using AuditIQ.Domain.Common;

namespace AuditIQ.Domain.Entities;

/// <summary>Section 5, table 5. Questions per scorecard version, grouped by section.</summary>
public class ScorecardQuestion : Entity
{
    public required Guid ScorecardId { get; set; }
    public Scorecard? Scorecard { get; init; }

    public required string SectionName { get; set; }
    public required string Text { get; set; }
    public required int Weight { get; set; }
    public bool IsFailLogic { get; set; }
    public int SortOrder { get; set; }

    /// <summary>Per-question custom answer set — never a global Pass/Fail enum (Section 15).</summary>
    public ICollection<QuestionAnswerOption> AnswerOptions { get; init; } = [];
}
