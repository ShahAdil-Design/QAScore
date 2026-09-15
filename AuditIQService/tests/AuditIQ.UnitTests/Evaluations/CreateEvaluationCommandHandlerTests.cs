using AuditIQ.Application.Evaluations.Commands.CreateEvaluation;
using AuditIQ.Domain.Entities;
using AuditIQ.Domain.Enums;
using AuditIQ.UnitTests.TestUtilities;
using Xunit;

namespace AuditIQ.UnitTests.Evaluations;

public class CreateEvaluationCommandHandlerTests
{
    [Fact]
    public async Task GivenValidScorecardAndUsers_WhenHandlingCreateEvaluation_ThenEvaluationIsCreatedAsDraft()
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
        var agent = new User { SsoSubjectId = "agent-1", DisplayName = "Agent", Email = "agent@test.com", Role = UserRole.Agent };
        var evaluator = new User { SsoSubjectId = "eval-1", DisplayName = "Evaluator", Email = "eval@test.com", Role = UserRole.QaEvaluator };

        db.ScorecardCategories.Add(category);
        db.Scorecards.Add(scorecard);
        db.Users.AddRange(agent, evaluator);
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new CreateEvaluationCommandHandler(db);
        var command = new CreateEvaluationCommand(scorecard.Id, agent.Id, evaluator.Id, null, null, "AQ-1", null, null);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var evaluation = await db.Evaluations.FindAsync([result.Value], CancellationToken.None);
        Assert.NotNull(evaluation);
        Assert.Equal(EvaluationStatus.Draft, evaluation.Status);
        Assert.Equal(agent.Id, evaluation.AgentId);
    }

    [Fact]
    public async Task GivenAnArchivedScorecard_WhenHandlingCreateEvaluation_ThenResultIsFailure()
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
            IsArchived = true,
        };
        db.ScorecardCategories.Add(category);
        db.Scorecards.Add(scorecard);
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new CreateEvaluationCommandHandler(db);
        var command = new CreateEvaluationCommand(scorecard.Id, Guid.NewGuid(), Guid.NewGuid(), null, null, null, null, null);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Evaluation.ScorecardArchived", result.FirstError.Code);
    }

    [Fact]
    public async Task GivenAnUnknownScorecard_WhenHandlingCreateEvaluation_ThenResultIsNotFoundFailure()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var handler = new CreateEvaluationCommandHandler(db);
        var command = new CreateEvaluationCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, null, null, null, null);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Evaluation.ScorecardNotFound", result.FirstError.Code);
    }
}
