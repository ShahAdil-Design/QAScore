using AuditIQ.Domain.Enums;

namespace AuditIQ.Application.Evaluations.Dtos;

public sealed record EvaluationSummaryDto(
    Guid Id,
    string ScorecardName,
    Guid AgentId,
    string AgentName,
    Guid EvaluatorId,
    string EvaluatorName,
    string? Reference,
    EvaluationStatus Status,
    decimal? TotalScore,
    DateTimeOffset? SubmittedAt);
