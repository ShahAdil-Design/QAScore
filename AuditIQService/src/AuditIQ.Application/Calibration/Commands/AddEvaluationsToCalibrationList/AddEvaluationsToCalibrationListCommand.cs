using AuditIQ.Application.Abstractions.Messaging;

namespace AuditIQ.Application.Calibration.Commands.AddEvaluationsToCalibrationList;

public sealed record AddEvaluationsToCalibrationListCommand(Guid CalibrationListId, IReadOnlyList<Guid> EvaluationIds) : ICommand;
