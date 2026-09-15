using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Dashboard.Dtos;

namespace AuditIQ.Application.Dashboard.Queries.GetAgentDashboard;

public sealed record GetAgentDashboardQuery(Guid AgentId, int Days = 30) : IQuery<AgentDashboardDto>;
