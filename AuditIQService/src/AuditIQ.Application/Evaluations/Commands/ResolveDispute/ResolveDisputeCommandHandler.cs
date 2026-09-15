using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Notifications;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using AuditIQ.Application.Scorecards;
using AuditIQ.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Evaluations.Commands.ResolveDispute;

public sealed class ResolveDisputeCommandHandler(IApplicationDbContext db, INotificationJobs notificationJobs)
    : ICommandHandler<ResolveDisputeCommand>
{
    public async Task<Result> Handle(ResolveDisputeCommand command, CancellationToken cancellationToken)
    {
        var evaluation = await db.Evaluations
            .Include(e => e.Answers)
            .FirstOrDefaultAsync(e => e.Id == command.EvaluationId, cancellationToken);

        if (evaluation is null)
            return Result.Failure(EvaluationErrors.NotFound(command.EvaluationId));
        if (evaluation.Status != EvaluationStatus.Disputed)
            return Result.Failure(EvaluationErrors.NotDisputed);

        // The evaluator may have corrected answers while the evaluation sat Disputed (via
        // SaveDraftAnswers, which now also accepts this status) — recompute the score from
        // whatever is currently saved rather than trusting the stale value from the original
        // Submit, exactly like SubmitEvaluationCommandHandler does.
        var questions = await db.ScorecardQuestions
            .Include(q => q.AnswerOptions)
            .Where(q => q.ScorecardId == evaluation.ScorecardId)
            .ToListAsync(cancellationToken);

        var maxScore = await db.Scorecards
            .Where(s => s.Id == evaluation.ScorecardId)
            .Select(s => s.MaxScore)
            .FirstOrDefaultAsync(cancellationToken);

        var allAnswered = questions.All(q =>
            evaluation.Answers.Any(a => a.ScorecardQuestionId == q.Id && a.AnswerValue is not null));
        if (!allAnswered)
            return Result.Failure(EvaluationErrors.IncompleteAnswers);

        var answerValues = evaluation.Answers.ToDictionary(a => a.ScorecardQuestionId, a => a.AnswerValue);
        evaluation.TotalScore = ScoreCalculator.ComputeTotalScore(questions, answerValues, maxScore);
        evaluation.Status = EvaluationStatus.Resolved;
        evaluation.ResolutionNotes = command.ResolutionNotes;
        await db.SaveChangesAsync(cancellationToken);

        notificationJobs.EnqueueDisputeResolutionNotification(evaluation.Id);

        return Result.Success();
    }
}
