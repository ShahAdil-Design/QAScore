using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Notifications;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using AuditIQ.Application.Abstractions.Time;
using AuditIQ.Application.Scorecards;
using AuditIQ.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Evaluations.Commands.SubmitEvaluation;

public sealed class SubmitEvaluationCommandHandler(
    IApplicationDbContext db,
    INotificationJobs notificationJobs,
    IDateTimeProvider clock) : ICommandHandler<SubmitEvaluationCommand>
{
    public async Task<Result> Handle(SubmitEvaluationCommand command, CancellationToken cancellationToken)
    {
        var evaluation = await db.Evaluations
            .Include(e => e.Answers)
            .FirstOrDefaultAsync(e => e.Id == command.EvaluationId, cancellationToken);

        if (evaluation is null)
            return Result.Failure(EvaluationErrors.NotFound(command.EvaluationId));
        if (evaluation.Status != EvaluationStatus.Draft)
            return Result.Failure(EvaluationErrors.NotDraft);

        var questions = await db.ScorecardQuestions
            .Include(q => q.AnswerOptions)
            .Where(q => q.ScorecardId == evaluation.ScorecardId)
            .ToListAsync(cancellationToken);

        var maxScore = await db.Scorecards
            .Where(s => s.Id == evaluation.ScorecardId)
            .Select(s => s.MaxScore)
            .FirstOrDefaultAsync(cancellationToken);

        // Score is fully derived from the chosen option now, so "answered" only needs a
        // non-null AnswerValue — every valid option (including a zero-value Fail/N/A one)
        // resolves to a determinate score.
        var allAnswered = questions.All(q =>
            evaluation.Answers.Any(a => a.ScorecardQuestionId == q.Id && a.AnswerValue is not null));
        if (!allAnswered)
            return Result.Failure(EvaluationErrors.IncompleteAnswers);

        var answerValues = evaluation.Answers.ToDictionary(a => a.ScorecardQuestionId, a => a.AnswerValue);
        evaluation.TotalScore = ScoreCalculator.ComputeTotalScore(questions, answerValues, maxScore);
        evaluation.Status = EvaluationStatus.Submitted;
        evaluation.EvaluatorNotes = command.EvaluatorNotes;
        evaluation.SubmittedAt = clock.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        // Enqueued only after the write commits — no dual writes (ADR-04).
        notificationJobs.EnqueueEvaluationSubmittedNotification(evaluation.Id);

        return Result.Success();
    }
}
