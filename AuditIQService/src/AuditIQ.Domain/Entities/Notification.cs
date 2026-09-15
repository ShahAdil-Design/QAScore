using AuditIQ.Domain.Common;

namespace AuditIQ.Domain.Entities;

public enum NotificationType
{
    EvaluationSubmitted,
    DisputeResolved,
    CalibrationListAssigned,
}

/// <summary>One row per in-app alert delivered to a single recipient — created by
/// NotificationService alongside (never instead of) its existing log line. RelatedEvaluationId /
/// RelatedCalibrationListId let the frontend deep-link the notification to the thing it's about;
/// at most one of them is set, depending on Type.</summary>
public class Notification : Entity
{
    public required Guid RecipientUserId { get; init; }
    public required NotificationType Type { get; init; }
    public required string Message { get; init; }

    public Guid? RelatedEvaluationId { get; init; }
    public Guid? RelatedCalibrationListId { get; init; }

    public DateTimeOffset CreatedAtUtc { get; init; } = DateTimeOffset.UtcNow;
    public bool IsRead { get; set; }
}
