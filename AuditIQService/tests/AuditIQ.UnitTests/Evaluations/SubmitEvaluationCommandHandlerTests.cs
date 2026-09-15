using AuditIQ.Application.Evaluations.Commands.SubmitEvaluation;
using AuditIQ.Domain.Entities;
using AuditIQ.Domain.Enums;
using AuditIQ.UnitTests.TestUtilities;
using Xunit;

namespace AuditIQ.UnitTests.Evaluations;

public class SubmitEvaluationCommandHandlerTests
{
    private static async Task<(AuditIQ.Infrastructure.Persistence.AuditIQDbContext Db, Evaluation Evaluation, ScorecardQuestion Q1, ScorecardQuestion Q2)> SeedDraftEvaluation(bool answerAllQuestions)
    {
        var db = InMemoryDbContextFactory.Create();

        var category = new ScorecardCategory { Name = "Phone Call" };
        var scorecard = new Scorecard
        {
            ScorecardGroupId = Guid.NewGuid(),
            Version = 1,
            IsCurrentVersion = true,
            Name = "Core QA",
            ScorecardType = "Standard",
            CategoryId = category.Id,
        };
        var q1 = new ScorecardQuestion { ScorecardId = scorecard.Id, SectionName = "Opening", Text = "Verified identity", Weight = 60 };
        q1.AnswerOptions.Add(new QuestionAnswerOption { ScorecardQuestionId = q1.Id, Label = "Pass", Value = 100 });
        q1.AnswerOptions.Add(new QuestionAnswerOption { ScorecardQuestionId = q1.Id, Label = "Fail", Value = 0 });
        var q2 = new ScorecardQuestion { ScorecardId = scorecard.Id, SectionName = "Opening", Text = "Closed with next steps", Weight = 40 };
        q2.AnswerOptions.Add(new QuestionAnswerOption { ScorecardQuestionId = q2.Id, Label = "Pass", Value = 100 });
        q2.AnswerOptions.Add(new QuestionAnswerOption { ScorecardQuestionId = q2.Id, Label = "Fail", Value = 50 });

        var agent = new User { SsoSubjectId = "agent-1", DisplayName = "Agent", Email = "agent@test.com", Role = UserRole.Agent };
        var evaluator = new User { SsoSubjectId = "eval-1", DisplayName = "Evaluator", Email = "eval@test.com", Role = UserRole.QaEvaluator };

        var evaluation = new Evaluation
        {
            ScorecardId = scorecard.Id,
            AgentId = agent.Id,
            EvaluatorId = evaluator.Id,
            Status = EvaluationStatus.Draft,
        };

        db.ScorecardCategories.Add(category);
        db.Scorecards.Add(scorecard);
        db.ScorecardQuestions.AddRange(q1, q2);
        db.Users.AddRange(agent, evaluator);
        db.Evaluations.Add(evaluation);

        db.EvaluationAnswers.Add(new EvaluationAnswer { EvaluationId = evaluation.Id, ScorecardQuestionId = q1.Id, AnswerValue = "Pass", Score = 100 });
        if (answerAllQuestions)
            db.EvaluationAnswers.Add(new EvaluationAnswer { EvaluationId = evaluation.Id, ScorecardQuestionId = q2.Id, AnswerValue = "Fail", Score = 50 });

        await db.SaveChangesAsync(CancellationToken.None);
        return (db, evaluation, q1, q2);
    }

    [Fact]
    public async Task GivenAllQuestionsAnswered_WhenSubmitting_ThenEvaluationIsSubmittedWithWeightedScoreAndNotificationEnqueued()
    {
        var (db, evaluation, _, _) = await SeedDraftEvaluation(answerAllQuestions: true);
        var notificationJobs = new RecordingNotificationJobs();
        var clock = new FakeDateTimeProvider(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
        var handler = new SubmitEvaluationCommandHandler(db, notificationJobs, clock);

        var result = await handler.Handle(new SubmitEvaluationCommand(evaluation.Id, "Looks good"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var updated = await db.Evaluations.FindAsync([evaluation.Id], CancellationToken.None);
        Assert.Equal(EvaluationStatus.Submitted, updated!.Status);
        // Pass=100 (weight 60), Fail=50 (weight 40) -> (100*60 + 50*40) / 100 = 80
        Assert.Equal(80m, updated.TotalScore);
        Assert.Equal(clock.UtcNow, updated.SubmittedAt);
        Assert.Single(notificationJobs.EvaluationSubmittedNotifications, evaluation.Id);
    }

    [Fact]
    public async Task GivenAnUnansweredQuestion_WhenSubmitting_ThenResultIsFailureAndNoNotificationIsEnqueued()
    {
        var (db, evaluation, _, _) = await SeedDraftEvaluation(answerAllQuestions: false);
        var notificationJobs = new RecordingNotificationJobs();
        var handler = new SubmitEvaluationCommandHandler(db, notificationJobs, new FakeDateTimeProvider(DateTimeOffset.UtcNow));

        var result = await handler.Handle(new SubmitEvaluationCommand(evaluation.Id, null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Evaluation.IncompleteAnswers", result.FirstError.Code);
        Assert.Empty(notificationJobs.EvaluationSubmittedNotifications);
    }

    [Fact]
    public async Task GivenAFailAllAnswerIsChosen_WhenSubmitting_ThenTotalScoreIsZeroRegardlessOfOtherAnswers()
    {
        var (db, evaluation, q1, q2) = await SeedDraftEvaluation(answerAllQuestions: true);
        var breach = new QuestionAnswerOption { ScorecardQuestionId = q1.Id, Label = "Breach", Value = 0, IsFailAll = true };
        db.QuestionAnswerOptions.Add(breach);
        var q1Answer = db.EvaluationAnswers.Single(a => a.ScorecardQuestionId == q1.Id);
        q1Answer.AnswerValue = "Breach";
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new SubmitEvaluationCommandHandler(db, new RecordingNotificationJobs(), new FakeDateTimeProvider(DateTimeOffset.UtcNow));
        var result = await handler.Handle(new SubmitEvaluationCommand(evaluation.Id, null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var updated = await db.Evaluations.FindAsync([evaluation.Id], CancellationToken.None);
        Assert.Equal(0m, updated!.TotalScore);
    }

    [Fact]
    public async Task GivenAFailSectionAnswerIsChosen_WhenSubmitting_ThenEveryQuestionInThatSectionScoresZero()
    {
        var (db, evaluation, q1, q2) = await SeedDraftEvaluation(answerAllQuestions: true);
        // Both questions share the "Opening" section — failing q1's section must zero q2 too,
        // even though q2's own answer ("Fail", value 50) is untouched.
        var proceduralFail = new QuestionAnswerOption { ScorecardQuestionId = q1.Id, Label = "Procedural Fail", Value = 0, IsFailSection = true };
        db.QuestionAnswerOptions.Add(proceduralFail);
        var q1Answer = db.EvaluationAnswers.Single(a => a.ScorecardQuestionId == q1.Id);
        q1Answer.AnswerValue = "Procedural Fail";
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new SubmitEvaluationCommandHandler(db, new RecordingNotificationJobs(), new FakeDateTimeProvider(DateTimeOffset.UtcNow));
        var result = await handler.Handle(new SubmitEvaluationCommand(evaluation.Id, null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var updated = await db.Evaluations.FindAsync([evaluation.Id], CancellationToken.None);
        // Both questions in "Opening" contribute 0 -> (0*60 + 0*40) / 100 = 0
        Assert.Equal(0m, updated!.TotalScore);
    }

    [Fact]
    public async Task GivenANotApplicableAnswerIsChosen_WhenSubmitting_ThenItContributesItsOwnValueLikeAnyOtherAnswer()
    {
        // Scorebuddy's own scoring never excludes N/A from the denominator — it scores the N/A
        // option like any other answer, using whatever Value it was configured with (verified
        // against real migrated evaluation data). ScoreCalculator matches that: N/A is not a
        // special case, it just contributes its own Value and weight.
        var (db, evaluation, q1, q2) = await SeedDraftEvaluation(answerAllQuestions: true);
        var notApplicable = new QuestionAnswerOption { ScorecardQuestionId = q1.Id, Label = "N/A", Value = 0, IsNotApplicable = true };
        db.QuestionAnswerOptions.Add(notApplicable);
        var q1Answer = db.EvaluationAnswers.Single(a => a.ScorecardQuestionId == q1.Id);
        q1Answer.AnswerValue = "N/A";
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new SubmitEvaluationCommandHandler(db, new RecordingNotificationJobs(), new FakeDateTimeProvider(DateTimeOffset.UtcNow));
        var result = await handler.Handle(new SubmitEvaluationCommand(evaluation.Id, null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var updated = await db.Evaluations.FindAsync([evaluation.Id], CancellationToken.None);
        // q1 N/A contributes Value=0 at weight 60, q2 Fail contributes 50 at weight 40:
        // (0*60 + 50*40) / 100 = 20
        Assert.Equal(20m, updated!.TotalScore);
    }
}
