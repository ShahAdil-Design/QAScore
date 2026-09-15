using AuditIQ.Application.Abstractions.Notifications;

namespace AuditIQ.UnitTests.TestUtilities;

public class RecordingNotificationJobs : INotificationJobs
{
    public List<Guid> EvaluationSubmittedNotifications { get; } = [];
    public List<Guid> DisputeResolutionNotifications { get; } = [];
    public List<Guid> CalibrationParticipantsNotifications { get; } = [];

    public void EnqueueEvaluationSubmittedNotification(Guid evaluationId) => EvaluationSubmittedNotifications.Add(evaluationId);
    public void EnqueueDisputeResolutionNotification(Guid evaluationId) => DisputeResolutionNotifications.Add(evaluationId);
    public void EnqueueCalibrationParticipantsNotification(Guid calibrationSessionId) => CalibrationParticipantsNotifications.Add(calibrationSessionId);
}
