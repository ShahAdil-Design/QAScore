using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Pagination;
using AuditIQ.Application.Calibration.Dtos;

namespace AuditIQ.Application.Calibration.Queries.GetCalibrationLists;

public sealed record GetCalibrationListsQuery(string? Search, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<CalibrationListSummaryDto>>;
