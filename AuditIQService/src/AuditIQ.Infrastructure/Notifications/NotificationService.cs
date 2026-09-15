using AuditIQ.Domain.Entities;
using AuditIQ.Domain.Enums;
using AuditIQ.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AuditIQ.Infrastructure.Notifications;

/// <summary>
/// Delivery channel is in-app only for now (Section 13 still hasn't settled real email) — each
/// method resolves its recipients and writes one Notification row per recipient, read by the
/// header bell. The log line stays alongside the write; it's cheap and useful for tracing what
/// a Hangfire job actually did.
/// </summary>
public class NotificationService(AuditIQDbContext db, ILogger<NotificationService> logger) : INotificationService
{
    public async Task NotifyAgentOfEvaluationSubmitted(Guid evaluationId)
    {
        var evaluation = await db.Evaluations
            .Include(e => e.Evaluator)
            .FirstOrDefaultAsync(e => e.Id == evaluationId);

        if (evaluation is null)
        {
            logger.LogWarning("NotifyAgentOfEvaluationSubmitted: evaluation {EvaluationId} not found", evaluationId);
            return;
        }

        // The evaluation is submitted by a supervisor/evaluator about this agent — it's the
        // agent who needs to see it next (Acknowledge/Dispute), not the evaluator who just wrote it.
        db.Notifications.Add(new Notification
        {
            RecipientUserId = evaluation.AgentId,
            Type = NotificationType.EvaluationSubmitted,
            Message = $"{evaluation.Evaluator!.DisplayName} submitted a new evaluation for you",
            RelatedEvaluationId = evaluationId,
        });
        await db.SaveChangesAsync();

        logger.LogInformation(
            "Evaluation {EvaluationId} submitted by evaluator {EvaluatorId} — notified agent {AgentId}",
            evaluationId, evaluation.EvaluatorId, evaluation.AgentId);
    }

    public async Task NotifyAgentOfDispute(Guid evaluationId)
    {
        var evaluation = await db.Evaluations.FirstOrDefaultAsync(e => e.Id == evaluationId);
        if (evaluation is null)
        {
            logger.LogWarning("NotifyAgentOfDispute: evaluation {EvaluationId} not found", evaluationId);
            return;
        }

        db.Notifications.Add(new Notification
        {
            RecipientUserId = evaluation.AgentId,
            Type = NotificationType.DisputeResolved,
            Message = "Your disputed evaluation has been resolved",
            RelatedEvaluationId = evaluationId,
        });
        await db.SaveChangesAsync();

        logger.LogInformation(
            "Dispute resolved for evaluation {EvaluationId} — notified agent {AgentId}",
            evaluationId, evaluation.AgentId);
    }

    public async Task NotifyCalibrationParticipants(Guid calibrationListId)
    {
        var list = await db.CalibrationLists.FirstOrDefaultAsync(l => l.Id == calibrationListId);
        if (list is null)
        {
            logger.LogWarning("NotifyCalibrationParticipants: calibration list {CalibrationListId} not found", calibrationListId);
            return;
        }

        // VisibilityScope is the free-text string set on the list (NewCalibrationListModal's
        // fixed options) — matched here, not parsed generically, since those are the only
        // values the frontend ever sends.
        var roles = list.VisibilityScope switch
        {
            "Supervisors" => new[] { UserRole.Supervisor },
            "Team Leads" => new[] { UserRole.TeamLead },
            "Evaluators" => new[] { UserRole.QaEvaluator },
            _ => new[] { UserRole.Admin, UserRole.Supervisor, UserRole.TeamLead, UserRole.QaEvaluator },
        };

        var recipientIds = await db.Users
            .Where(u => roles.Contains(u.Role))
            .Select(u => u.Id)
            .ToListAsync();

        var message = $"You've been assigned to calibrate \"{list.Name}\"";
        db.Notifications.AddRange(recipientIds.Select(recipientId => new Notification
        {
            RecipientUserId = recipientId,
            Type = NotificationType.CalibrationListAssigned,
            Message = message,
            RelatedCalibrationListId = calibrationListId,
        }));
        await db.SaveChangesAsync();

        logger.LogInformation(
            "Calibration list {CalibrationListId} created — notified [{RecipientIds}]",
            calibrationListId, string.Join(", ", recipientIds));
    }
}
