using AuditIQ.Domain.Common;

namespace AuditIQ.Domain.Entities;

/// <summary>
/// One calibrator's re-score of one CalibrationListItem. Created lazily on first save
/// (there's no pre-invited-participant step — anyone matching the list's
/// VisibilityScope can calibrate an item), one row per (item, evaluator) pair.
/// </summary>
public class CalibrationRating : Entity
{
    public required Guid CalibrationListItemId { get; set; }
    public CalibrationListItem? CalibrationListItem { get; init; }

    public required Guid EvaluatorId { get; set; }
    public User? Evaluator { get; init; }

    public decimal? Score { get; set; }

    public ICollection<CalibrationAnswer> Answers { get; init; } = [];
}
