using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Reviews.Dtos;

namespace AuditIQ.Application.Reviews.Queries.GetAgentPerformanceSummary;

/// <summary>Backs the agent self-service dashboard (Epic 4) — own results and trends.</summary>
public sealed record GetAgentPerformanceSummaryQuery(Guid AgentId) : IQuery<AgentPerformanceSummaryDto>;
