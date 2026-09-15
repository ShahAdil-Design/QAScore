namespace AuditIQ.Application.Calibration.Dtos;

/// <summary>One row in the calibration list builder's browse/filter table — an
/// evaluation available to add to a list (already scored, i.e. not Draft).</summary>
public sealed record CalibrationCandidateEvaluationDto(
    Guid Id,
    string? Reference,
    Guid AgentId,
    string AgentName,
    Guid EvaluatorId,
    string EvaluatorName,
    DateTimeOffset? EventOccurredAt,
    DateTimeOffset? SubmittedAt,
    IReadOnlyList<string> TeamNames,
    IReadOnlyList<string> GroupNames,
    string ScorecardName,
    string CategoryName,
    string? EventTypeName,
    string? EventSubTypeName,
    decimal? TotalScore);
