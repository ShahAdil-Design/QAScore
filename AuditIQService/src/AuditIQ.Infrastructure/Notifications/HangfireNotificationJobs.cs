using AuditIQ.Application.Abstractions.Notifications;
using Hangfire;

namespace AuditIQ.Infrastructure.Notifications;

public class HangfireNotificationJobs(IBackgroundJobClient backgroundJobs) : INotificationJobs
{
    public void EnqueueEvaluationSubmittedNotification(Guid evaluationId) =>
        backgroundJobs.Enqueue<INotificationService>(x => x.NotifyAgentOfEvaluationSubmitted(evaluationId));

    public void EnqueueDisputeResolutionNotification(Guid evaluationId) =>
        backgroundJobs.Enqueue<INotificationService>(x => x.NotifyAgentOfDispute(evaluationId));

    public void EnqueueCalibrationParticipantsNotification(Guid calibrationSessionId) =>
        backgroundJobs.Enqueue<INotificationService>(x => x.NotifyCalibrationParticipants(calibrationSessionId));
}
