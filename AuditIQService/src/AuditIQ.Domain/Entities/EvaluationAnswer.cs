using AuditIQ.Domain.Common;

namespace AuditIQ.Domain.Entities;

/// <summary>
/// Section 5, table 9. Four independent fields per answered question — never
/// collapse this to just "answer + comment" (Section 15).
/// </summary>
public class EvaluationAnswer : Entity
{
    public required Guid EvaluationId { get; set; }
    public Evaluation? Evaluation { get; init; }

    public required Guid ScorecardQuestionId { get; set; }
    public ScorecardQuestion? ScorecardQuestion { get; init; }

    public string? AnswerValue { get; set; }
    public string? CauseCode { get; set; }
    public string? Comment { get; set; }
    public decimal? Score { get; set; }
}
