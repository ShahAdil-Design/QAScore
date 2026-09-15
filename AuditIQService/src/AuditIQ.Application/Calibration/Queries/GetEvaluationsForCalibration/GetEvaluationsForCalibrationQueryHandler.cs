using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Pagination;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using AuditIQ.Application.Calibration.Dtos;
using AuditIQ.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Calibration.Queries.GetEvaluationsForCalibration;

public sealed class GetEvaluationsForCalibrationQueryHandler(IApplicationDbContext db)
    : IQueryHandler<GetEvaluationsForCalibrationQuery, PagedResult<CalibrationCandidateEvaluationDto>>
{
    public async Task<Result<PagedResult<CalibrationCandidateEvaluationDto>>> Handle(
        GetEvaluationsForCalibrationQuery query, CancellationToken cancellationToken)
    {
        // Only scored evaluations can be calibrated — a Draft has no answers yet
        // for a calibrator to compare against.
        var evaluations = db.Evaluations.Where(e => e.Status != EvaluationStatus.Draft);

        if (query.DateFrom is { } from)
            evaluations = evaluations.Where(e => e.EventOccurredAt >= from);
        if (query.DateTo is { } to)
            evaluations = evaluations.Where(e => e.EventOccurredAt <= to);
        if (query.GroupId is { } groupId)
            evaluations = evaluations.Where(e => e.Agent!.UserTeams.Any(ut => ut.Team!.GroupId == groupId));
        if (query.TeamId is { } teamId)
            evaluations = evaluations.Where(e => e.Agent!.UserTeams.Any(ut => ut.TeamId == teamId));
        if (query.EventTypeId is { } eventTypeId)
            evaluations = evaluations.Where(e => e.EventTypeId == eventTypeId);
        if (query.EvaluatorId is { } evaluatorId)
            evaluations = evaluations.Where(e => e.EvaluatorId == evaluatorId);
        if (!string.IsNullOrWhiteSpace(query.Reference))
            evaluations = evaluations.Where(e => e.Reference != null && e.Reference.Contains(query.Reference));
        if (query.ScorecardId is { } scorecardId)
            evaluations = evaluations.Where(e => e.ScorecardId == scorecardId);
        if (query.CategoryId is { } categoryId)
            evaluations = evaluations.Where(e => e.Scorecard!.CategoryId == categoryId);
        if (query.ScoreMin is { } scoreMin)
            evaluations = evaluations.Where(e => (e.LegacyTotalScore ?? e.TotalScore) >= scoreMin);
        if (query.ScoreMax is { } scoreMax)
            evaluations = evaluations.Where(e => (e.LegacyTotalScore ?? e.TotalScore) <= scoreMax);

        var totalCount = await evaluations.CountAsync(cancellationToken);

        List<Guid> pageIds;
        int page;
        int pageSize;

        if (query.Random)
        {
            // ORDER BY NEWID() — a random sample of SampleSize matches, no further paging.
            page = 1;
            pageSize = Math.Clamp(query.SampleSize, 1, 500);
            pageIds = await evaluations
                .OrderBy(_ => Guid.NewGuid())
                .Take(pageSize)
                .Select(e => e.Id)
                .ToListAsync(cancellationToken);
        }
        else
        {
            page = Math.Max(query.Page, 1);
            pageSize = Math.Clamp(query.PageSize, 1, 200);
            pageIds = await evaluations
                .OrderByDescending(e => e.SubmittedAt ?? e.EventOccurredAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => e.Id)
                .ToListAsync(cancellationToken);
        }

        // Re-query by id to project the full shape (keeps the filter/order query above
        // a simple Guid projection, which SQL Server optimizes far better for ORDER BY NEWID()).
        var rows = await db.Evaluations
            .Where(e => pageIds.Contains(e.Id))
            .Select(e => new
            {
                e.Id,
                e.Reference,
                e.AgentId,
                AgentName = e.Agent!.DisplayName,
                e.EvaluatorId,
                EvaluatorName = e.Evaluator!.DisplayName,
                e.EventOccurredAt,
                e.SubmittedAt,
                TeamNames = e.Agent.UserTeams.Select(ut => ut.Team!.Name).Distinct().ToList(),
                GroupNames = e.Agent.UserTeams.Select(ut => ut.Team!.Group!.Name).Distinct().ToList(),
                ScorecardName = e.Scorecard!.Name,
                CategoryName = e.Scorecard.Category!.Name,
                EventTypeName = e.EventType != null ? e.EventType.Name : null,
                EventSubTypeName = e.EventSubType != null ? e.EventSubType.Name : null,
                TotalScore = e.LegacyTotalScore ?? e.TotalScore,
            })
            .ToListAsync(cancellationToken);

        var byId = rows.ToDictionary(r => r.Id);
        var items = pageIds
            .Where(byId.ContainsKey)
            .Select(id =>
            {
                var r = byId[id];
                return new CalibrationCandidateEvaluationDto(
                    r.Id, r.Reference, r.AgentId, r.AgentName, r.EvaluatorId, r.EvaluatorName,
                    r.EventOccurredAt, r.SubmittedAt, r.TeamNames, r.GroupNames,
                    r.ScorecardName, r.CategoryName, r.EventTypeName, r.EventSubTypeName, r.TotalScore);
            })
            .ToList();

        return new PagedResult<CalibrationCandidateEvaluationDto>(items, page, pageSize, query.Random ? items.Count : totalCount);
    }
}
