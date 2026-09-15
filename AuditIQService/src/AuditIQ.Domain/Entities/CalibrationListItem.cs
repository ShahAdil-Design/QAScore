using AuditIQ.Domain.Common;

namespace AuditIQ.Domain.Entities;

/// <summary>One evaluation added to a CalibrationList — Team/Group/scorecard/category
/// context for display is all derived from the linked Evaluation, never duplicated here.</summary>
public class CalibrationListItem : Entity
{
    public required Guid CalibrationListId { get; set; }
    public CalibrationList? CalibrationList { get; init; }

    public required Guid EvaluationId { get; set; }
    public Evaluation? Evaluation { get; init; }

    public ICollection<CalibrationRating> Ratings { get; init; } = [];
}
