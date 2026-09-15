namespace AuditIQ.Application.Reviews.Dtos;

public sealed record AgentResultDto(Guid AgentId, string AgentName, decimal? AverageScore, int EvaluationCount);

public sealed record TeamResultsDto(Guid TeamId, string TeamName, IReadOnlyList<AgentResultDto> Agents);
