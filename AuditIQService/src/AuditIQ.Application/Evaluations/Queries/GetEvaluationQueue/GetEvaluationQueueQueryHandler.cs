using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Pagination;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using AuditIQ.Application.Evaluations.Dtos;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Evaluations.Queries.GetEvaluationQueue;

public sealed class GetEvaluationQueueQueryHandler(IApplicationDbContext db)
    : IQueryHandler<GetEvaluationQueueQuery, PagedResult<EvaluationSummaryDto>>
{
    public async Task<Result<PagedResult<EvaluationSummaryDto>>> Handle(GetEvaluationQueueQuery query, CancellationToken cancellationToken)
    {
        var evaluations = db.Evaluations
            .Include(e => e.Scorecard)
            .Include(e => e.Agent)
            .Include(e => e.Evaluator)
            .AsQueryable();

        if (query.RestrictToAgentIds is { } restrictToAgentIds)
            evaluations = evaluations.Where(e => restrictToAgentIds.Contains(e.AgentId));
        if (query.AgentId is { } agentId)
            evaluations = evaluations.Where(e => e.AgentId == agentId);
        if (query.EvaluatorId is { } evaluatorId)
            evaluations = evaluations.Where(e => e.EvaluatorId == evaluatorId);
        if (query.TeamId is { } teamId)
            evaluations = evaluations.Where(e => e.Agent!.UserTeams.Any(ut => ut.TeamId == teamId));
        if (query.Status is { } status)
            evaluations = evaluations.Where(e => e.Status == status);
        if (query.ScorecardId is { } scorecardId)
            evaluations = evaluations.Where(e => e.ScorecardId == scorecardId);
        if (query.DateFrom is { } dateFrom)
            evaluations = evaluations.Where(e => e.EventOccurredAt >= dateFrom);
        if (query.DateTo is { } dateTo)
            evaluations = evaluations.Where(e => e.EventOccurredAt <= dateTo);

        var totalCount = await evaluations.CountAsync(cancellationToken);

        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 200);

        var items = await evaluations
            .OrderByDescending(e => e.SubmittedAt ?? e.EventOccurredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new EvaluationSummaryDto(
                e.Id,
                e.Scorecard!.Name,
                e.AgentId,
                e.Agent!.DisplayName,
                e.EvaluatorId,
                e.Evaluator!.DisplayName,
                e.Reference,
                e.Status,
                e.LegacyTotalScore ?? e.TotalScore,
                e.SubmittedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<EvaluationSummaryDto>(items, page, pageSize, totalCount);
    }
}
