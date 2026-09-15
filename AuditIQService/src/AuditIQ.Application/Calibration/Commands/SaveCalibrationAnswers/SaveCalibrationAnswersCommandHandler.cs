using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using AuditIQ.Application.Scorecards;
using AuditIQ.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Calibration.Commands.SaveCalibrationAnswers;

public sealed class SaveCalibrationAnswersCommandHandler(IApplicationDbContext db) : ICommandHandler<SaveCalibrationAnswersCommand>
{
    public async Task<Result> Handle(SaveCalibrationAnswersCommand command, CancellationToken cancellationToken)
    {
        var item = await db.CalibrationListItems
            .Include(i => i.Evaluation)
            .Include(i => i.Ratings).ThenInclude(r => r.Answers)
            .FirstOrDefaultAsync(i => i.Id == command.CalibrationListItemId, cancellationToken);

        if (item is null)
            return Result.Failure(CalibrationErrors.ItemNotFound(command.CalibrationListItemId));

        var evaluatorExists = await db.Users.AnyAsync(u => u.Id == command.EvaluatorId, cancellationToken);
        if (!evaluatorExists)
            return Result.Failure(CalibrationErrors.EvaluatorNotFound(command.EvaluatorId));

        // No pre-invited participant list in this model — the first save for a given
        // (item, evaluator) pair creates the rating row lazily.
        var rating = item.Ratings.FirstOrDefault(r => r.EvaluatorId == command.EvaluatorId);
        if (rating is null)
        {
            rating = new CalibrationRating { CalibrationListItemId = item.Id, EvaluatorId = command.EvaluatorId };
            db.CalibrationRatings.Add(rating);
        }

        var questions = await db.ScorecardQuestions
            .Include(q => q.AnswerOptions)
            .Where(q => q.ScorecardId == item.Evaluation!.ScorecardId)
            .ToDictionaryAsync(q => q.Id, cancellationToken);

        var maxScore = await db.Scorecards
            .Where(s => s.Id == item.Evaluation!.ScorecardId)
            .Select(s => s.MaxScore)
            .FirstOrDefaultAsync(cancellationToken);

        foreach (var input in command.Answers)
        {
            if (!questions.TryGetValue(input.QuestionId, out var question))
                return Result.Failure(CalibrationErrors.QuestionNotOnScorecard(input.QuestionId));

            if (input.AnswerValue is not null && question.AnswerOptions.All(o => o.Label != input.AnswerValue))
                return Result.Failure(CalibrationErrors.AnswerOptionInvalid(input.QuestionId, input.AnswerValue));
        }

        // Tracks the final AnswerValue per question after this save, combining whatever was
        // already stored with what this call is upserting — built explicitly rather than
        // re-reading rating.Answers after the Add() calls below, since that collection's
        // fixup timing for newly-added entities isn't something to depend on.
        var finalAnswerValues = rating.Answers.ToDictionary(a => a.ScorecardQuestionId, a => a.AnswerValue);

        foreach (var input in command.Answers)
        {
            var question = questions[input.QuestionId];
            var score = ScoreCalculator.ResolveAnswerScore(question, input.AnswerValue);

            var existing = rating.Answers.FirstOrDefault(a => a.ScorecardQuestionId == input.QuestionId);
            if (existing is null)
            {
                db.CalibrationAnswers.Add(new CalibrationAnswer
                {
                    CalibrationRatingId = rating.Id,
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

            finalAnswerValues[input.QuestionId] = input.AnswerValue;
        }

        // Auto-finalize this rating's overall score (including Fail All/Fail Section/N/A
        // cascades) once every scorecard question has been answered — the same rule
        // SubmitEvaluationCommandHandler uses for real evaluations, via the same
        // ScoreCalculator, so a calibrator's total is computed identically to the original
        // evaluator's rather than a separately-maintained formula.
        var allQuestions = questions.Values.ToList();
        var allAnswered = allQuestions.Count > 0 && allQuestions.All(q =>
            finalAnswerValues.TryGetValue(q.Id, out var v) && v is not null);

        if (allAnswered)
            rating.Score = ScoreCalculator.ComputeTotalScore(allQuestions, finalAnswerValues, maxScore);

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
