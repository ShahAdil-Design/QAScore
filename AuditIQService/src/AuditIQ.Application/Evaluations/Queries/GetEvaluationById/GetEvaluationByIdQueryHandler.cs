using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using AuditIQ.Application.Evaluations.Dtos;
using AuditIQ.Application.Scorecards.Dtos;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Evaluations.Queries.GetEvaluationById;

public sealed class GetEvaluationByIdQueryHandler(IApplicationDbContext db) : IQueryHandler<GetEvaluationByIdQuery, EvaluationDetailDto>
{
    public async Task<Result<EvaluationDetailDto>> Handle(GetEvaluationByIdQuery query, CancellationToken cancellationToken)
    {
        var evaluation = await db.Evaluations
            .Include(e => e.Scorecard)
            .Include(e => e.Agent)
            .Include(e => e.Evaluator)
            .Include(e => e.Answers)
            .FirstOrDefaultAsync(e => e.Id == query.EvaluationId, cancellationToken);

        if (evaluation is null)
            return Result.Failure<EvaluationDetailDto>(EvaluationErrors.NotFound(query.EvaluationId));

        var questions = await db.ScorecardQuestions
            .Include(q => q.AnswerOptions)
            .Where(q => q.ScorecardId == evaluation.ScorecardId)
            .OrderBy(q => q.SortOrder)
            .ToListAsync(cancellationToken);

        var answers = questions.Select(q =>
        {
            var answer = evaluation.Answers.FirstOrDefault(a => a.ScorecardQuestionId == q.Id);
            return new EvaluationAnswerDto(
                q.Id,
                q.Text,
                q.SectionName,
                q.Weight,
                q.AnswerOptions.OrderBy(o => o.SortOrder)
                    .Select(o => new AnswerOptionDto(o.Label, o.Value, o.IsFailSection, o.IsFailAll, o.IsNotApplicable))
                    .ToList(),
                answer?.AnswerValue,
                answer?.CauseCode,
                answer?.Comment,
                answer?.Score);
        }).ToList();

        return new EvaluationDetailDto(
            evaluation.Id,
            evaluation.ScorecardId,
            evaluation.Scorecard!.Name,
            evaluation.Scorecard!.TargetPercentage,
            evaluation.AgentId,
            evaluation.Agent!.DisplayName,
            evaluation.EvaluatorId,
            evaluation.Evaluator!.DisplayName,
            evaluation.Reference,
            evaluation.Status,
            evaluation.EventOccurredAt,
            evaluation.EventDurationSeconds,
            evaluation.EvaluatorNotes,
            evaluation.DisputeReason,
            evaluation.ResolutionNotes,
            evaluation.SubmittedAt,
            evaluation.DisplayScore,
            answers);
    }
}
