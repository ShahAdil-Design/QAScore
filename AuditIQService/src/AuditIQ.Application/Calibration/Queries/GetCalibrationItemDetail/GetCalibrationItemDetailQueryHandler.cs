using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using AuditIQ.Application.Calibration.Dtos;
using AuditIQ.Application.Scorecards.Dtos;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Calibration.Queries.GetCalibrationItemDetail;

public sealed class GetCalibrationItemDetailQueryHandler(IApplicationDbContext db)
    : IQueryHandler<GetCalibrationItemDetailQuery, CalibrationItemDetailDto>
{
    public async Task<Result<CalibrationItemDetailDto>> Handle(GetCalibrationItemDetailQuery query, CancellationToken cancellationToken)
    {
        var item = await db.CalibrationListItems
            .Include(i => i.Evaluation).ThenInclude(e => e!.Scorecard)
            .Include(i => i.Evaluation).ThenInclude(e => e!.Agent).ThenInclude(a => a!.UserTeams).ThenInclude(ut => ut.Team)
            .Include(i => i.Evaluation).ThenInclude(e => e!.Evaluator)
            .Include(i => i.Evaluation).ThenInclude(e => e!.EventType)
            .Include(i => i.Evaluation).ThenInclude(e => e!.EventSubType)
            .Include(i => i.Evaluation).ThenInclude(e => e!.Answers)
            .Include(i => i.Ratings).ThenInclude(r => r.Evaluator)
            .Include(i => i.Ratings).ThenInclude(r => r.Answers)
            .FirstOrDefaultAsync(i => i.Id == query.CalibrationListItemId, cancellationToken);

        if (item is null)
            return Result.Failure<CalibrationItemDetailDto>(CalibrationErrors.ItemNotFound(query.CalibrationListItemId));

        var evaluation = item.Evaluation!;

        var scorecardQuestions = await db.ScorecardQuestions
            .Include(q => q.AnswerOptions)
            .Where(q => q.ScorecardId == evaluation.ScorecardId)
            .OrderBy(q => q.SortOrder)
            .ToListAsync(cancellationToken);

        var questions = scorecardQuestions.Select(q =>
        {
            var originalAnswer = evaluation.Answers.FirstOrDefault(a => a.ScorecardQuestionId == q.Id);
            return new CalibrationQuestionDto(
                q.Id,
                q.Text,
                q.SectionName,
                q.Weight,
                q.AnswerOptions.OrderBy(o => o.SortOrder)
                    .Select(o => new AnswerOptionDto(o.Label, o.Value, o.IsFailSection, o.IsFailAll, o.IsNotApplicable))
                    .ToList(),
                originalAnswer?.AnswerValue,
                originalAnswer?.CauseCode,
                originalAnswer?.Comment,
                originalAnswer?.Score);
        }).ToList();

        var ratings = item.Ratings
            .Select(r => new CalibrationRatingDto(
                r.EvaluatorId,
                r.Evaluator!.DisplayName,
                r.Score,
                r.Answers.Select(a => new CalibrationAnswerDto(a.ScorecardQuestionId, a.AnswerValue, a.CauseCode, a.Comment, a.Score)).ToList()))
            .ToList();

        // Variance across original + every calibrator's score — flags disagreement,
        // same idea as the old single-evaluation model but now including the original
        // evaluator's score as one of the compared values, not just the calibrators.
        var scores = ratings.Where(r => r.Score.HasValue).Select(r => r.Score!.Value).ToList();
        if (evaluation.DisplayScore.HasValue)
            scores.Add(evaluation.DisplayScore.Value);
        var variance = scores.Count >= 2 ? scores.Max() - scores.Min() : (decimal?)null;

        return new CalibrationItemDetailDto(
            item.Id,
            item.CalibrationListId,
            item.EvaluationId,
            evaluation.Scorecard!.Name,
            evaluation.AgentId,
            evaluation.Agent!.DisplayName,
            evaluation.EvaluatorId,
            evaluation.Evaluator!.DisplayName,
            evaluation.Agent.UserTeams.Select(ut => ut.Team!.Name).Distinct().ToList(),
            evaluation.EventType?.Name,
            evaluation.EventSubType?.Name,
            evaluation.EventOccurredAt,
            evaluation.EventDurationSeconds,
            evaluation.Reference,
            evaluation.DisplayScore,
            questions,
            ratings,
            variance);
    }
}
