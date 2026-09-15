using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using AuditIQ.Application.Abstractions.Time;
using AuditIQ.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Calibration.Commands.CreateCalibrationList;

public sealed class CreateCalibrationListCommandHandler(IApplicationDbContext db, IDateTimeProvider clock)
    : ICommandHandler<CreateCalibrationListCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateCalibrationListCommand command, CancellationToken cancellationToken)
    {
        var creatorExists = await db.Users.AnyAsync(u => u.Id == command.CreatedByUserId, cancellationToken);
        if (!creatorExists)
            return Result.Failure<Guid>(CalibrationErrors.EvaluatorNotFound(command.CreatedByUserId));

        var list = new CalibrationList
        {
            Name = command.Name,
            VisibilityScope = command.VisibilityScope,
            CreatedByUserId = command.CreatedByUserId,
            CreatedAt = clock.UtcNow,
        };

        db.CalibrationLists.Add(list);
        await db.SaveChangesAsync(cancellationToken);

        return list.Id;
    }
}
