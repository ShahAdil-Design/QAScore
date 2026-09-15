using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Notifications;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using AuditIQ.Domain.Entities;
using AuditIQ.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Calibration.Commands.AddEvaluationsToCalibrationList;

public sealed class AddEvaluationsToCalibrationListCommandHandler(IApplicationDbContext db, INotificationJobs notificationJobs)
    : ICommandHandler<AddEvaluationsToCalibrationListCommand>
{
    public async Task<Result> Handle(AddEvaluationsToCalibrationListCommand command, CancellationToken cancellationToken)
    {
        var list = await db.CalibrationLists.FirstOrDefaultAsync(l => l.Id == command.CalibrationListId, cancellationToken);
        if (list is null)
            return Result.Failure(CalibrationErrors.ListNotFound(command.CalibrationListId));

        var requestedIds = command.EvaluationIds.Distinct().ToList();

        // Only evaluations that have actually been scored can be calibrated — a Draft
        // has no answers yet for a calibrator to compare against (same rule the old
        // single-evaluation model enforced).
        var validEvaluationIds = await db.Evaluations
            .Where(e => requestedIds.Contains(e.Id) && e.Status != EvaluationStatus.Draft)
            .Select(e => e.Id)
            .ToListAsync(cancellationToken);

        var missing = requestedIds.Except(validEvaluationIds).ToList();
        if (missing.Count > 0)
            return Result.Failure(CalibrationErrors.EvaluationNotFound(missing[0]));

        var alreadyInList = await db.CalibrationListItems
            .Where(i => i.CalibrationListId == list.Id && validEvaluationIds.Contains(i.EvaluationId))
            .Select(i => i.EvaluationId)
            .ToListAsync(cancellationToken);

        var toAdd = validEvaluationIds.Except(alreadyInList).ToList();
        if (toAdd.Count == 0)
            return Result.Success();

        foreach (var evaluationId in toAdd)
        {
            db.CalibrationListItems.Add(new CalibrationListItem
            {
                CalibrationListId = list.Id,
                EvaluationId = evaluationId,
            });
        }

        await db.SaveChangesAsync(cancellationToken);

        notificationJobs.EnqueueCalibrationParticipantsNotification(list.Id);

        return Result.Success();
    }
}
