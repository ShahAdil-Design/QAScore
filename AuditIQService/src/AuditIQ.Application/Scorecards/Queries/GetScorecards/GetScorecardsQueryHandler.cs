using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using AuditIQ.Application.Scorecards.Dtos;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Scorecards.Queries.GetScorecards;

public sealed class GetScorecardsQueryHandler(IApplicationDbContext db)
    : IQueryHandler<GetScorecardsQuery, IReadOnlyList<ScorecardSummaryDto>>
{
    public async Task<Result<IReadOnlyList<ScorecardSummaryDto>>> Handle(GetScorecardsQuery query, CancellationToken cancellationToken)
    {
        var scorecards = db.Scorecards
            .Include(s => s.Category)
            .Include(s => s.Questions)
            .Include(s => s.OrgGroups).ThenInclude(g => g.Group)
            .Where(s => s.IsCurrentVersion && !s.IsArchived)
            .AsQueryable();

        if (query.CategoryId is { } categoryId)
            scorecards = scorecards.Where(s => s.CategoryId == categoryId);

        var loaded = await scorecards.ToListAsync(cancellationToken);

        var results = loaded
            .Select(s => new ScorecardSummaryDto(
                s.Id, s.Name, s.Category!.Name, s.Location, s.TargetPercentage, s.MaxScore, s.Version, s.IsLocked, s.Questions.Count,
                s.OrgGroups.Select(g => g.Group!.Name).ToList()))
            .ToList();

        return results;
    }
}
