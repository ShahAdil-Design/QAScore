namespace AuditIQ.Application.Reviews.Dtos;

public sealed record ScorePointDto(DateTimeOffset Date, decimal Score);

public sealed record AgentPerformanceSummaryDto(
    Guid AgentId,
    string AgentName,
    decimal? AverageScore,
    int EvaluationCount,
    int DisputedCount,
    IReadOnlyList<ScorePointDto> Trend);
