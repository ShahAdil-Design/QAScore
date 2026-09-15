using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using AuditIQ.Application.Reviews.Dtos;
using AuditIQ.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Reviews.Queries.GetAgentPerformanceSummary;

public sealed class GetAgentPerformanceSummaryQueryHandler(IApplicationDbContext db)
    : IQueryHandler<GetAgentPerformanceSummaryQuery, AgentPerformanceSummaryDto>
{
    public async Task<Result<AgentPerformanceSummaryDto>> Handle(GetAgentPerformanceSummaryQuery query, CancellationToken cancellationToken)
    {
        var agent = await db.Users.FirstOrDefaultAsync(u => u.Id == query.AgentId, cancellationToken);
        if (agent is null)
            return Result.Failure<AgentPerformanceSummaryDto>(ReviewErrors.UserNotFound(query.AgentId));

        var evaluations = await db.Evaluations
            .Where(e => e.AgentId == query.AgentId && (e.LegacyTotalScore ?? e.TotalScore) != null && e.SubmittedAt != null)
            .OrderBy(e => e.SubmittedAt)
            .Select(e => new { e.SubmittedAt, Score = e.LegacyTotalScore ?? e.TotalScore, e.Status })
            .ToListAsync(cancellationToken);

        var trend = evaluations
            .Select(e => new ScorePointDto(e.SubmittedAt!.Value, e.Score!.Value))
            .ToList();

        var averageScore = evaluations.Count > 0 ? Math.Round(evaluations.Average(e => e.Score!.Value), 2) : (decimal?)null;
        var disputedCount = evaluations.Count(e => e.Status is EvaluationStatus.Disputed);

        return new AgentPerformanceSummaryDto(agent.Id, agent.DisplayName, averageScore, evaluations.Count, disputedCount, trend);
    }
}
