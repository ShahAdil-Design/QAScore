namespace AuditIQ.Application.Calibration.Dtos;

public sealed record CalibrationListSummaryDto(
    Guid Id,
    string Name,
    string VisibilityScope,
    string CreatedByName,
    DateTimeOffset CreatedAt,
    IReadOnlyList<string> GroupNames,
    IReadOnlyList<string> TeamNames,
    int ItemCount,
    int RatedItemCount);
