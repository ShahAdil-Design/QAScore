namespace AuditIQ.Application.Dashboard.Dtos;

public sealed record AgentOverviewRowDto(
    Guid AgentId,
    string DisplayName,
    decimal? AverageScore,
    int EvaluationCount,
    int DisputedCount,
    int FailsCount);
