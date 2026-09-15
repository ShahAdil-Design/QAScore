namespace AuditIQ.Infrastructure.Notifications;

/// <summary>
/// The three notification jobs from master doc Section 6. Hangfire re-resolves
/// this from DI when the background worker picks up the job — the method body
/// is what actually runs, not the enqueue call site.
/// </summary>
public interface INotificationService
{
    Task NotifyAgentOfEvaluationSubmitted(Guid evaluationId);
    Task NotifyAgentOfDispute(Guid evaluationId);
    Task NotifyCalibrationParticipants(Guid calibrationListId);
}
