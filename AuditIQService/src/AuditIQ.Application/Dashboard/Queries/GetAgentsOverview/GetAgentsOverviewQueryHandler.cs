using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using AuditIQ.Application.Dashboard.Dtos;
using AuditIQ.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Dashboard.Queries.GetAgentsOverview;

public sealed class GetAgentsOverviewQueryHandler(IApplicationDbContext db)
    : IQueryHandler<GetAgentsOverviewQuery, IReadOnlyList<AgentOverviewRowDto>>
{
    public async Task<Result<IReadOnlyList<AgentOverviewRowDto>>> Handle(GetAgentsOverviewQuery query, CancellationToken cancellationToken)
    {
        var agents = db.Users.Where(u => u.Role == UserRole.Agent && !u.IsDeleted);
        if (query.SupervisorId is { } supervisorId)
            agents = agents.Where(u => u.SupervisorId == supervisorId);

        var agentList = await agents
            .OrderBy(u => u.DisplayName)
            .Select(u => new { u.Id, u.DisplayName })
            .ToListAsync(cancellationToken);

        // Aggregated separately (rather than per-agent round trips) and joined in memory — one
        // query for however many agents are in scope, same pattern as GetDashboardSummary.
        var stats = await db.Evaluations
            .Where(e => e.Status != EvaluationStatus.Draft && (e.LegacyTotalScore ?? e.TotalScore) != null)
            .GroupBy(e => e.AgentId)
            .Select(g => new
            {
                AgentId = g.Key,
                Average = g.Average(e => e.LegacyTotalScore ?? e.TotalScore),
                Count = g.Count(),
                Disputed = g.Count(e => e.Status == EvaluationStatus.Disputed),
                Fails = g.Count(e => (e.LegacyTotalScore ?? e.TotalScore) == 0m),
            })
            .ToDictionaryAsync(s => s.AgentId, cancellationToken);

        var results = agentList
            .Select(a => stats.TryGetValue(a.Id, out var s)
                ? new AgentOverviewRowDto(a.Id, a.DisplayName, Math.Round(s.Average!.Value, 2), s.Count, s.Disputed, s.Fails)
                : new AgentOverviewRowDto(a.Id, a.DisplayName, null, 0, 0, 0))
            .ToList();

        return results;
    }
}
