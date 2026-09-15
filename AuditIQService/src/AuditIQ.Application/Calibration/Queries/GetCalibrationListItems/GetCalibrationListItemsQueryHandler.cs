using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Pagination;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using AuditIQ.Application.Calibration.Dtos;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Calibration.Queries.GetCalibrationListItems;

public sealed class GetCalibrationListItemsQueryHandler(IApplicationDbContext db)
    : IQueryHandler<GetCalibrationListItemsQuery, PagedResult<CalibrationListItemSummaryDto>>
{
    public async Task<Result<PagedResult<CalibrationListItemSummaryDto>>> Handle(
        GetCalibrationListItemsQuery query, CancellationToken cancellationToken)
    {
        var listExists = await db.CalibrationLists.AnyAsync(l => l.Id == query.CalibrationListId, cancellationToken);
        if (!listExists)
            return Result.Failure<PagedResult<CalibrationListItemSummaryDto>>(CalibrationErrors.ListNotFound(query.CalibrationListId));

        var itemsQuery = db.CalibrationListItems.Where(i => i.CalibrationListId == query.CalibrationListId);

        var totalCount = await itemsQuery.CountAsync(cancellationToken);

        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 500);

        var evaluatorId = query.EvaluatorId;
        var rows = await itemsQuery
            .OrderByDescending(i => i.Evaluation!.SubmittedAt ?? i.Evaluation!.EventOccurredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new CalibrationListItemSummaryDto(
                i.Id,
                i.EvaluationId,
                i.Evaluation!.AgentId,
                i.Evaluation.Agent!.DisplayName,
                i.Evaluation.Evaluator!.DisplayName,
                i.Evaluation.EventOccurredAt,
                i.Evaluation.Agent.UserTeams.Select(ut => ut.Team!.Name).Distinct().ToList(),
                i.Evaluation.Scorecard!.Name,
                i.Evaluation.Reference,
                i.Evaluation.LegacyTotalScore ?? i.Evaluation.TotalScore,
                evaluatorId == null ? null : i.Ratings.Where(r => r.EvaluatorId == evaluatorId).Select(r => r.Score).FirstOrDefault(),
                i.Ratings.Count(r => r.Score != null)))
            .ToListAsync(cancellationToken);

        return new PagedResult<CalibrationListItemSummaryDto>(rows, page, pageSize, totalCount);
    }
}
