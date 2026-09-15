using AuditIQ.Domain.Common;

namespace AuditIQ.Domain.Entities;

/// <summary>
/// A calibrator's own answer to one scorecard question, scoped to a single
/// CalibrationRating — mirrors EvaluationAnswer's four independent fields so a
/// calibrator re-scores the same questions the original evaluator did, rather than
/// entering one opaque overall number.
/// </summary>
public class CalibrationAnswer : Entity
{
    public required Guid CalibrationRatingId { get; set; }
    public CalibrationRating? CalibrationRating { get; init; }

    public required Guid ScorecardQuestionId { get; set; }
    public ScorecardQuestion? ScorecardQuestion { get; init; }

    public string? AnswerValue { get; set; }
    public string? CauseCode { get; set; }
    public string? Comment { get; set; }
    public decimal? Score { get; set; }
}
