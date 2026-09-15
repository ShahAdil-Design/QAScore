using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Calibration.Dtos;

namespace AuditIQ.Application.Calibration.Queries.GetCalibrationItemDetail;

public sealed record GetCalibrationItemDetailQuery(Guid CalibrationListItemId) : IQuery<CalibrationItemDetailDto>;
