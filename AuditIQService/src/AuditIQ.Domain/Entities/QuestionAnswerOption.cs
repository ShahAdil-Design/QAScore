using AuditIQ.Domain.Common;

namespace AuditIQ.Domain.Entities;

/// <summary>
/// Section 5, table 6. E.g. Q1 = Pass/Procedural Fail/Breach, Q2 = Pass/Requires
/// Improvement/Fail — each question defines its own option list.
///
/// Value is the score AWARDED when this option is chosen — scoring is derived
/// from the selected option, never typed in freehand by the evaluator/calibrator.
/// IsFailSection/IsFailAll/IsNotApplicable are fail-logic cascades checked at
/// submit time (see SubmitEvaluationCommandHandler): choosing a Fail All option
/// zeroes the whole evaluation, Fail Section zeroes every question in the same
/// SectionName, and Not Applicable excludes the question from scoring entirely.
/// Convention (validated): Value must be 0 when any of the three flags is set.
/// </summary>
public class QuestionAnswerOption : Entity
{
    public required Guid ScorecardQuestionId { get; set; }
    public ScorecardQuestion? ScorecardQuestion { get; init; }

    public required string Label { get; set; }
    public int SortOrder { get; set; }

    public decimal Value { get; set; }
    public bool IsFailSection { get; set; }
    public bool IsFailAll { get; set; }
    public bool IsNotApplicable { get; set; }
}
