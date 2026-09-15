using AuditIQ.Application.Abstractions.Messaging;

namespace AuditIQ.Application.Calibration.Commands.CreateCalibrationList;

public sealed record CreateCalibrationListCommand(string Name, string VisibilityScope, Guid CreatedByUserId) : ICommand<Guid>;
