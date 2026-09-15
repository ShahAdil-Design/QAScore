namespace AuditIQ.Application.Calibration.Dtos;

/// <summary>One row in a calibration list's item queue — the "Calibrate" screen you land
/// on after opening a list. MyScore is null until the requesting evaluatorId has rated it.</summary>
public sealed record CalibrationListItemSummaryDto(
    Guid Id,
    Guid EvaluationId,
    Guid AgentId,
    string AgentName,
    string EvaluatorName,
    DateTimeOffset? EventOccurredAt,
    IReadOnlyList<string> TeamNames,
    string ScorecardName,
    string? Reference,
    decimal? OriginalScore,
    decimal? MyScore,
    int RatingCount);
