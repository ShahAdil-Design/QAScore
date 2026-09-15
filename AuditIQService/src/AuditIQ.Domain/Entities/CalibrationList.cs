using AuditIQ.Domain.Common;

namespace AuditIQ.Domain.Entities;

/// <summary>
/// A named, reusable batch of evaluations pulled together for calibration — built by
/// filtering/sampling (or manually picking) evaluations into it, then anyone matching
/// VisibilityScope can open the list and calibrate any item in it. There is no
/// pre-invited participant list (unlike the old single-evaluation CalibrationSession
/// model): a CalibrationRating is created lazily the first time someone rates an item.
/// </summary>
public class CalibrationList : Entity
{
    public required string Name { get; set; }
    public required string VisibilityScope { get; set; }

    public required Guid CreatedByUserId { get; set; }
    public User? CreatedBy { get; init; }
    public required DateTimeOffset CreatedAt { get; set; }

    public ICollection<CalibrationListItem> Items { get; init; } = [];
}
