using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Pagination;
using AuditIQ.Application.Calibration.Dtos;

namespace AuditIQ.Application.Calibration.Queries.GetEvaluationsForCalibration;

/// <summary>
/// Backs the calibration list builder's Filter Results panel + candidate table.
/// When Random is true, SampleSize evaluations matching the filters are returned in
/// random order (no further paging) — the "Random" sampling mode; otherwise it's a
/// normal filtered, paged browse (the "Manual" mode's underlying table).
/// </summary>
public sealed record GetEvaluationsForCalibrationQuery(
    DateTimeOffset? DateFrom,
    DateTimeOffset? DateTo,
    Guid? GroupId,
    Guid? TeamId,
    Guid? EventTypeId,
    Guid? EvaluatorId,
    string? Reference,
    Guid? ScorecardId,
    Guid? CategoryId,
    decimal? ScoreMin,
    decimal? ScoreMax,
    bool Random = false,
    int SampleSize = 50,
    int Page = 1,
    int PageSize = 25) : IQuery<PagedResult<CalibrationCandidateEvaluationDto>>;
