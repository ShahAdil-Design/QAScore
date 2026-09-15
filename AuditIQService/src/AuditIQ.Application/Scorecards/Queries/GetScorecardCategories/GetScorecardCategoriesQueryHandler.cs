using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using AuditIQ.Application.Scorecards.Dtos;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Scorecards.Queries.GetScorecardCategories;

public sealed class GetScorecardCategoriesQueryHandler(IApplicationDbContext db)
    : IQueryHandler<GetScorecardCategoriesQuery, IReadOnlyList<ScorecardCategoryDto>>
{
    public async Task<Result<IReadOnlyList<ScorecardCategoryDto>>> Handle(GetScorecardCategoriesQuery query, CancellationToken cancellationToken)
    {
        var categories = await db.ScorecardCategories
            .OrderBy(c => c.Name)
            .Select(c => new ScorecardCategoryDto(c.Id, c.Name))
            .ToListAsync(cancellationToken);

        return categories;
    }
}
