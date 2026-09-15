using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using AuditIQ.Application.Scorecards.Dtos;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Scorecards.Queries.GetScorecardById;

public sealed class GetScorecardByIdQueryHandler(IApplicationDbContext db) : IQueryHandler<GetScorecardByIdQuery, ScorecardDetailDto>
{
    public async Task<Result<ScorecardDetailDto>> Handle(GetScorecardByIdQuery query, CancellationToken cancellationToken)
    {
        var scorecard = await db.Scorecards
            .Include(s => s.Category)
            .Include(s => s.TipSheets)
            .Include(s => s.Questions).ThenInclude(q => q.AnswerOptions)
            .Include(s => s.OrgGroups).ThenInclude(g => g.Group)
            .FirstOrDefaultAsync(s => s.Id == query.ScorecardId, cancellationToken);

        if (scorecard is null)
            return Result.Failure<ScorecardDetailDto>(ScorecardErrors.NotFound(query.ScorecardId));

        var questions = scorecard.Questions
            .OrderBy(q => q.SortOrder)
            .Select(q =>
            {
                var options = q.AnswerOptions.OrderBy(o => o.SortOrder)
                    .Select(o => new AnswerOptionDto(o.Label, o.Value, o.IsFailSection, o.IsFailAll, o.IsNotApplicable))
                    .ToList();
                var maxScore = options.Count > 0 ? options.Max(o => o.Value) : 0;
                return new ScorecardQuestionDto(q.Id, q.SectionName, q.Text, q.Weight, q.IsFailLogic, q.SortOrder, options, maxScore);
            })
            .ToList();

        return new ScorecardDetailDto(
            scorecard.Id,
            scorecard.Name,
            scorecard.Description,
            scorecard.ScorecardType,
            scorecard.Category!.Name,
            scorecard.Location,
            scorecard.TargetPercentage,
            scorecard.MaxScore,
            scorecard.Version,
            scorecard.IsLocked,
            scorecard.IsArchived,
            scorecard.OrgGroups.Select(g => g.GroupId).ToList(),
            scorecard.OrgGroups.Select(g => g.Group!.Name).ToList(),
            questions,
            scorecard.TipSheets.Select(t => t.Title).ToList());
    }
}
