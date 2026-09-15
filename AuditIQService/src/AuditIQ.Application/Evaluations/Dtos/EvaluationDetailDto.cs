using AuditIQ.Domain.Enums;

namespace AuditIQ.Application.Evaluations.Dtos;

public sealed record EvaluationDetailDto(
    Guid Id,
    Guid ScorecardId,
    string ScorecardName,
    decimal? ScorecardTargetPercentage,
    Guid AgentId,
    string AgentName,
    Guid EvaluatorId,
    string EvaluatorName,
    string? Reference,
    EvaluationStatus Status,
    DateTimeOffset? EventOccurredAt,
    int? EventDurationSeconds,
    string? EvaluatorNotes,
    string? DisputeReason,
    string? ResolutionNotes,
    DateTimeOffset? SubmittedAt,
    decimal? TotalScore,
    IReadOnlyList<EvaluationAnswerDto> Answers);
