using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Scorecards.Dtos;

namespace AuditIQ.Application.Scorecards.Queries.GetScorecardCategories;

public sealed record GetScorecardCategoriesQuery : IQuery<IReadOnlyList<ScorecardCategoryDto>>;
