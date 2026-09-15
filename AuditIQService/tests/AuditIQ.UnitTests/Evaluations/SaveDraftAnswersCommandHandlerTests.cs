using AuditIQ.Application.Evaluations.Commands.SaveDraftAnswers;
using AuditIQ.Domain.Entities;
using AuditIQ.Domain.Enums;
using AuditIQ.UnitTests.TestUtilities;
using Xunit;

namespace AuditIQ.UnitTests.Evaluations;

public class SaveDraftAnswersCommandHandlerTests
{
    [Fact]
    public async Task GivenAnAnswerValueOutsideTheQuestionsOptionSet_WhenSavingDraftAnswers_ThenResultIsFailure()
    {
        await using var db = InMemoryDbContextFactory.Create();

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
        var question = new ScorecardQuestion { ScorecardId = scorecard.Id, SectionName = "Opening", Text = "Verified identity", Weight = 100 };
        question.AnswerOptions.Add(new QuestionAnswerOption { ScorecardQuestionId = question.Id, Label = "Pass", Value = 100 });
        question.AnswerOptions.Add(new QuestionAnswerOption { ScorecardQuestionId = question.Id, Label = "Procedural Fail", Value = 0 });

        var evaluation = new Evaluation
        {
            ScorecardId = scorecard.Id,
            AgentId = Guid.NewGuid(),
            EvaluatorId = Guid.NewGuid(),
            Status = EvaluationStatus.Draft,
        };

        db.ScorecardCategories.Add(category);
        db.Scorecards.Add(scorecard);
        db.ScorecardQuestions.Add(question);
        db.Evaluations.Add(evaluation);
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new SaveDraftAnswersCommandHandler(db);
        var command = new SaveDraftAnswersCommand(evaluation.Id, [new AnswerInput(question.Id, "Definitely Fail", null, null)]);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Evaluation.AnswerOptionInvalid", result.FirstError.Code);
    }

    [Fact]
    public async Task GivenAValidAnswerValue_WhenSavingDraftAnswers_ThenScoreIsDerivedFromTheChosenOptionsValue()
    {
        await using var db = InMemoryDbContextFactory.Create();

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
        var question = new ScorecardQuestion { ScorecardId = scorecard.Id, SectionName = "Opening", Text = "Verified identity", Weight = 100 };
        question.AnswerOptions.Add(new QuestionAnswerOption { ScorecardQuestionId = question.Id, Label = "Pass", Value = 100 });
        question.AnswerOptions.Add(new QuestionAnswerOption { ScorecardQuestionId = question.Id, Label = "Fail", Value = 0 });

        var evaluation = new Evaluation
        {
            ScorecardId = scorecard.Id,
            AgentId = Guid.NewGuid(),
            EvaluatorId = Guid.NewGuid(),
            Status = EvaluationStatus.Draft,
        };

        db.ScorecardCategories.Add(category);
        db.Scorecards.Add(scorecard);
        db.ScorecardQuestions.Add(question);
        db.Evaluations.Add(evaluation);
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new SaveDraftAnswersCommandHandler(db);
        var command = new SaveDraftAnswersCommand(evaluation.Id, [new AnswerInput(question.Id, "Pass", null, null)]);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var savedAnswer = db.EvaluationAnswers.Single(a => a.EvaluationId == evaluation.Id);
        Assert.Equal(100m, savedAnswer.Score);
    }
}
