using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Dashboard.Dtos;

namespace AuditIQ.Application.Dashboard.Queries.GetAgentsOverview;

/// <summary>Backs the Supervisor Dashboard's "Agents" tab — one row per agent with their
/// aggregate performance, optionally scoped to one supervisor's direct reports (the "All
/// Supervisors" picker).</summary>
public sealed record GetAgentsOverviewQuery(Guid? SupervisorId = null) : IQuery<IReadOnlyList<AgentOverviewRowDto>>;
