using AuditIQ.Application.Calibration.Commands.SaveCalibrationAnswers;
using AuditIQ.Domain.Entities;
using AuditIQ.Domain.Enums;
using AuditIQ.UnitTests.TestUtilities;
using Xunit;

namespace AuditIQ.UnitTests.Calibration;

public class SaveCalibrationAnswersCommandHandlerTests
{
    private static async Task<(AuditIQ.Infrastructure.Persistence.AuditIQDbContext Db, CalibrationListItem Item, Guid EvaluatorA, Guid QuestionOne, Guid QuestionTwo)> SeedItem()
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
        var questionOne = new ScorecardQuestion { ScorecardId = scorecard.Id, SectionName = "Opening", Text = "Verified identity", Weight = 60 };
        questionOne.AnswerOptions.Add(new QuestionAnswerOption { ScorecardQuestionId = questionOne.Id, Label = "Pass", Value = 100 });
        questionOne.AnswerOptions.Add(new QuestionAnswerOption { ScorecardQuestionId = questionOne.Id, Label = "Fail", Value = 0 });
        var questionTwo = new ScorecardQuestion { ScorecardId = scorecard.Id, SectionName = "Closing", Text = "Confirmed next steps", Weight = 40 };
        questionTwo.AnswerOptions.Add(new QuestionAnswerOption { ScorecardQuestionId = questionTwo.Id, Label = "Pass", Value = 100 });
        questionTwo.AnswerOptions.Add(new QuestionAnswerOption { ScorecardQuestionId = questionTwo.Id, Label = "Fail", Value = 50 });

        var agent = new User { SsoSubjectId = "a", DisplayName = "Agent", Email = "a@test.com", Role = UserRole.Agent };
        var evaluator = new User { SsoSubjectId = "b", DisplayName = "Evaluator", Email = "b@test.com", Role = UserRole.QaEvaluator };
        var creator = new User { SsoSubjectId = "c", DisplayName = "Creator", Email = "c@test.com", Role = UserRole.Supervisor };
        var evaluatorA = new User { SsoSubjectId = "d", DisplayName = "Calibrator", Email = "d@test.com", Role = UserRole.QaEvaluator };
        var evaluation = new Evaluation
        {
            ScorecardId = scorecard.Id,
            AgentId = agent.Id,
            EvaluatorId = evaluator.Id,
            Status = EvaluationStatus.Submitted,
        };

        var list = new CalibrationList { Name = "List 1", VisibilityScope = "All", CreatedByUserId = creator.Id, CreatedAt = DateTimeOffset.UtcNow };
        var item = new CalibrationListItem { CalibrationListId = list.Id, EvaluationId = evaluation.Id };

        db.ScorecardCategories.Add(category);
        db.Scorecards.Add(scorecard);
        db.ScorecardQuestions.AddRange(questionOne, questionTwo);
        db.Users.AddRange(agent, evaluator, creator, evaluatorA);
        db.Evaluations.Add(evaluation);
        db.CalibrationLists.Add(list);
        db.CalibrationListItems.Add(item);
        await db.SaveChangesAsync(CancellationToken.None);

        return (db, item, evaluatorA.Id, questionOne.Id, questionTwo.Id);
    }

    [Fact]
    public async Task GivenOnlySomeQuestionsAnswered_WhenSavingAnswers_ThenARatingIsCreatedButScoreStaysNull()
    {
        var (db, item, evaluatorA, questionOne, _) = await SeedItem();
        var handler = new SaveCalibrationAnswersCommandHandler(db);

        var result = await handler.Handle(
            new SaveCalibrationAnswersCommand(item.Id, evaluatorA, [new CalibrationAnswerInput(questionOne, "Pass", null, null)]),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var rating = db.CalibrationRatings.Single(r => r.EvaluatorId == evaluatorA);
        Assert.Null(rating.Score);
    }

    [Fact]
    public async Task GivenAllQuestionsAnswered_WhenSavingAnswers_ThenScoreIsDerivedAndComputedAsTheWeightedAverage()
    {
        var (db, item, evaluatorA, questionOne, questionTwo) = await SeedItem();
        var handler = new SaveCalibrationAnswersCommandHandler(db);

        var result = await handler.Handle(
            new SaveCalibrationAnswersCommand(item.Id, evaluatorA,
            [
                new CalibrationAnswerInput(questionOne, "Pass", null, null),
                new CalibrationAnswerInput(questionTwo, "Fail", "Missed step", "Needs coaching"),
            ]),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var rating = db.CalibrationRatings.Single(r => r.EvaluatorId == evaluatorA);
        // Pass=100 (weight 60), Fail=50 (weight 40) -> (100*60 + 50*40) / 100 = 80
        Assert.Equal(80m, rating.Score);
        var answers = db.CalibrationAnswers.Where(a => a.CalibrationRatingId == rating.Id).ToList();
        Assert.Contains(answers, a => a.ScorecardQuestionId == questionOne && a.Score == 100m);
        Assert.Contains(answers, a => a.ScorecardQuestionId == questionTwo && a.Score == 50m);
    }

    [Fact]
    public async Task GivenNoRatingYetForThisEvaluator_WhenSavingAnswers_ThenOneIsCreatedLazily()
    {
        var (db, item, evaluatorA, questionOne, _) = await SeedItem();
        var handler = new SaveCalibrationAnswersCommandHandler(db);

        Assert.Empty(db.CalibrationRatings);

        await handler.Handle(
            new SaveCalibrationAnswersCommand(item.Id, evaluatorA, [new CalibrationAnswerInput(questionOne, "Pass", null, null)]),
            CancellationToken.None);

        Assert.Single(db.CalibrationRatings, r => r.EvaluatorId == evaluatorA && r.CalibrationListItemId == item.Id);
    }

    [Fact]
    public async Task GivenAQuestionNotOnTheScorecard_WhenSavingAnswers_ThenResultIsFailure()
    {
        var (db, item, evaluatorA, _, _) = await SeedItem();
        var handler = new SaveCalibrationAnswersCommandHandler(db);

        var result = await handler.Handle(
            new SaveCalibrationAnswersCommand(item.Id, evaluatorA, [new CalibrationAnswerInput(Guid.NewGuid(), "Pass", null, null)]),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Calibration.QuestionNotOnScorecard", result.FirstError.Code);
    }

    [Fact]
    public async Task GivenAFailAllAnswerIsChosen_WhenAllQuestionsAreAnswered_ThenTheOverallScoreIsZero()
    {
        var (db, item, evaluatorA, questionOne, questionTwo) = await SeedItem();
        // Add a Fail All option to question one, alongside its existing Pass/Fail options.
        var breach = new QuestionAnswerOption { ScorecardQuestionId = questionOne, Label = "Breach", Value = 0, IsFailAll = true };
        db.QuestionAnswerOptions.Add(breach);
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new SaveCalibrationAnswersCommandHandler(db);
        var result = await handler.Handle(
            new SaveCalibrationAnswersCommand(item.Id, evaluatorA,
            [
                new CalibrationAnswerInput(questionOne, "Breach", null, null),
                new CalibrationAnswerInput(questionTwo, "Pass", null, null),
            ]),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var rating = db.CalibrationRatings.Single(r => r.EvaluatorId == evaluatorA);
        Assert.Equal(0m, rating.Score);
    }
}
