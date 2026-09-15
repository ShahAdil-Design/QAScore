using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Pagination;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using AuditIQ.Application.Calibration.Dtos;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Calibration.Queries.GetCalibrationLists;

public sealed class GetCalibrationListsQueryHandler(IApplicationDbContext db)
    : IQueryHandler<GetCalibrationListsQuery, PagedResult<CalibrationListSummaryDto>>
{
    public async Task<Result<PagedResult<CalibrationListSummaryDto>>> Handle(GetCalibrationListsQuery query, CancellationToken cancellationToken)
    {
        var lists = db.CalibrationLists.Include(l => l.CreatedBy).AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
            lists = lists.Where(l => l.Name.Contains(query.Search));

        var totalCount = await lists.CountAsync(cancellationToken);

        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 200);

        var pageOfLists = await lists
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new
            {
                l.Id,
                l.Name,
                l.VisibilityScope,
                CreatedByName = l.CreatedBy!.DisplayName,
                l.CreatedAt,
                ItemCount = l.Items.Count,
                RatedItemCount = l.Items.Count(i => i.Ratings.Any(r => r.Score != null)),
            })
            .ToListAsync(cancellationToken);

        var listIds = pageOfLists.Select(l => l.Id).ToList();

        // Group/Team are derived from the agents behind each list's items (an
        // evaluation carries no direct Group/Team field — see Evaluation.cs) rather
        // than stored on the list itself, so they can't drift from reality.
        var teamsByList = await db.CalibrationListItems
            .Where(i => listIds.Contains(i.CalibrationListId))
            .SelectMany(i => i.Evaluation!.Agent!.UserTeams.Select(ut => new { i.CalibrationListId, ut.Team!.Name, GroupName = ut.Team.Group!.Name }))
            .Distinct()
            .ToListAsync(cancellationToken);

        var items = pageOfLists.Select(l => new CalibrationListSummaryDto(
            l.Id,
            l.Name,
            l.VisibilityScope,
            l.CreatedByName,
            l.CreatedAt,
            teamsByList.Where(t => t.CalibrationListId == l.Id).Select(t => t.GroupName).Distinct().ToList(),
            teamsByList.Where(t => t.CalibrationListId == l.Id).Select(t => t.Name).Distinct().ToList(),
            l.ItemCount,
            l.RatedItemCount)).ToList();

        return new PagedResult<CalibrationListSummaryDto>(items, page, pageSize, totalCount);
    }
}
