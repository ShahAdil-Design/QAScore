using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Pagination;
using AuditIQ.Application.Calibration.Dtos;

namespace AuditIQ.Application.Calibration.Queries.GetCalibrationListItems;

// EvaluatorId is optional and only used to populate MyScore per item (which calibrator
// is asking) — same pre-SSO caveat as elsewhere in this API (Section 13).
public sealed record GetCalibrationListItemsQuery(Guid CalibrationListId, Guid? EvaluatorId, int Page = 1, int PageSize = 50)
    : IQuery<PagedResult<CalibrationListItemSummaryDto>>;
