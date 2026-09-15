using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using AuditIQ.Application.Scorecards;
using AuditIQ.Domain.Entities;
using AuditIQ.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Evaluations.Commands.SaveDraftAnswers;

public sealed class SaveDraftAnswersCommandHandler(IApplicationDbContext db) : ICommandHandler<SaveDraftAnswersCommand>
{
    public async Task<Result> Handle(SaveDraftAnswersCommand command, CancellationToken cancellationToken)
    {
        var evaluation = await db.Evaluations
            .Include(e => e.Answers)
            .FirstOrDefaultAsync(e => e.Id == command.EvaluationId, cancellationToken);

        if (evaluation is null)
            return Result.Failure(EvaluationErrors.NotFound(command.EvaluationId));
        // Editable while Draft (normal scoring) or Disputed (evaluator correcting a disputed
        // score) — every other status (Submitted, Acknowledged, Resolved) is locked.
        if (evaluation.Status != EvaluationStatus.Draft && evaluation.Status != EvaluationStatus.Disputed)
            return Result.Failure(EvaluationErrors.NotEditable);

        var questions = await db.ScorecardQuestions
            .Include(q => q.AnswerOptions)
            .Where(q => q.ScorecardId == evaluation.ScorecardId)
            .ToDictionaryAsync(q => q.Id, cancellationToken);

        foreach (var input in command.Answers)
        {
            if (!questions.TryGetValue(input.QuestionId, out var question))
                return Result.Failure(EvaluationErrors.QuestionNotOnScorecard(input.QuestionId));

            if (input.AnswerValue is not null && question.AnswerOptions.All(o => o.Label != input.AnswerValue))
                return Result.Failure(EvaluationErrors.AnswerOptionInvalid(input.QuestionId, input.AnswerValue));
        }

        foreach (var input in command.Answers)
        {
            var question = questions[input.QuestionId];
            var score = ScoreCalculator.ResolveAnswerScore(question, input.AnswerValue);

            var existing = evaluation.Answers.FirstOrDefault(a => a.ScorecardQuestionId == input.QuestionId);
            if (existing is null)
            {
                db.EvaluationAnswers.Add(new EvaluationAnswer
                {
                    EvaluationId = evaluation.Id,
                    ScorecardQuestionId = input.QuestionId,
                    AnswerValue = input.AnswerValue,
                    CauseCode = input.CauseCode,
                    Comment = input.Comment,
                    Score = score,
                });
            }
            else
            {
                existing.AnswerValue = input.AnswerValue;
                existing.CauseCode = input.CauseCode;
                existing.Comment = input.Comment;
                existing.Score = score;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
