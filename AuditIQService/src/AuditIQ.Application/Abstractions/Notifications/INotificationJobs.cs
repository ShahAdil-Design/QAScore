namespace AuditIQ.Application.Abstractions.Notifications;

/// <summary>
/// Enqueues the notification jobs from master doc Section 6. Implemented in
/// Infrastructure via Hangfire — Application stays free of a Hangfire reference.
/// Delivery channel (email/in-app) is still TBD (Section 13); handlers only need
/// to know a job gets enqueued after their write commits (ADR-04's no-dual-writes
/// rule), not how it's eventually delivered.
/// </summary>
public interface INotificationJobs
{
    void EnqueueEvaluationSubmittedNotification(Guid evaluationId);
    void EnqueueDisputeResolutionNotification(Guid evaluationId);
    void EnqueueCalibrationParticipantsNotification(Guid calibrationSessionId);
}
