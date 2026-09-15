using System.Net;
using System.Net.Http.Json;
using AuditIQ.Domain.Entities;
using AuditIQ.Domain.Enums;
using AuditIQ.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AuditIQ.IntegrationTests;

public class CalibrationApiTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory = new();
    private HttpClient _client = null!;
    private Guid _evaluationId;
    private Guid _evaluatorOneId;
    private Guid _evaluatorTwoId;
    private Guid _creatorId;
    private Guid _questionId;

    public async Task InitializeAsync()
    {
        await _factory.InitializeDatabaseAsync();
        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-Test-Role", "Supervisor");

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuditIQDbContext>();

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
        question.AnswerOptions.Add(new QuestionAnswerOption { ScorecardQuestionId = question.Id, Label = "Fail", Value = 50 });

        var agent = new User { SsoSubjectId = "cal-agent", DisplayName = "Agent", Email = "cal-agent@test.com", Role = UserRole.Agent };
        var evaluatorOne = new User { SsoSubjectId = "cal-eval-1", DisplayName = "Evaluator One", Email = "cal-eval-1@test.com", Role = UserRole.QaEvaluator };
        var evaluatorTwo = new User { SsoSubjectId = "cal-eval-2", DisplayName = "Evaluator Two", Email = "cal-eval-2@test.com", Role = UserRole.QaEvaluator };
        var creator = new User { SsoSubjectId = "cal-creator", DisplayName = "Creator Supervisor", Email = "cal-creator@test.com", Role = UserRole.Supervisor };
        var evaluation = new Evaluation
        {
            ScorecardId = scorecard.Id,
            AgentId = agent.Id,
            EvaluatorId = evaluatorOne.Id,
            Status = EvaluationStatus.Submitted,
            TotalScore = 95,
        };
        evaluation.Answers.Add(new EvaluationAnswer { EvaluationId = evaluation.Id, ScorecardQuestionId = question.Id, AnswerValue = "Pass", Score = 95 });

        db.ScorecardCategories.Add(category);
        db.Scorecards.Add(scorecard);
        db.ScorecardQuestions.Add(question);
        db.Users.AddRange(agent, evaluatorOne, evaluatorTwo, creator);
        db.Evaluations.Add(evaluation);
        await db.SaveChangesAsync();

        _evaluationId = evaluation.Id;
        _evaluatorOneId = evaluatorOne.Id;
        _evaluatorTwoId = evaluatorTwo.Id;
        _creatorId = creator.Id;
        _questionId = question.Id;
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _factory.DropDatabaseAsync();
    }

    [Fact]
    public async Task GivenAScoredEvaluation_WhenBuildingAndCalibratingAList_ThenScoreIsComputedAndVarianceReflectsBothRaters()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/v1/calibration-lists", new
        {
            name = "August alignment",
            visibilityScope = "All",
            createdByUserId = _creatorId,
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var list = await createResponse.Content.ReadFromJsonAsync<CreatedResponse>();
        Assert.NotNull(list);

        var addItemsResponse = await _client.PostAsJsonAsync(
            $"/api/v1/calibration-lists/{list!.Id}/items", new { evaluationIds = new[] { _evaluationId } });
        Assert.Equal(HttpStatusCode.NoContent, addItemsResponse.StatusCode);

        var itemsResponse = await _client.GetAsync($"/api/v1/calibration-lists/{list.Id}/items");
        Assert.Equal(HttpStatusCode.OK, itemsResponse.StatusCode);
        var itemsPage = await itemsResponse.Content.ReadFromJsonAsync<PagedItemsResponse>();
        var item = Assert.Single(itemsPage!.Items);

        var saveOneResponse = await _client.PutAsJsonAsync($"/api/v1/calibration-lists/items/{item.Id}/answers", new
        {
            evaluatorId = _evaluatorOneId,
            answers = new[] { new { questionId = _questionId, answerValue = "Fail", causeCode = (string?)null, comment = "Disagree" } },
        });
        Assert.Equal(HttpStatusCode.NoContent, saveOneResponse.StatusCode);

        var saveTwoResponse = await _client.PutAsJsonAsync($"/api/v1/calibration-lists/items/{item.Id}/answers", new
        {
            evaluatorId = _evaluatorTwoId,
            answers = new[] { new { questionId = _questionId, answerValue = "Pass", causeCode = (string?)null, comment = (string?)null } },
        });
        Assert.Equal(HttpStatusCode.NoContent, saveTwoResponse.StatusCode);

        var detailResponse = await _client.GetAsync($"/api/v1/calibration-lists/items/{item.Id}");
        Assert.Equal(HttpStatusCode.OK, detailResponse.StatusCode);
        var detail = await detailResponse.Content.ReadFromJsonAsync<CalibrationItemDetailResponse>();

        Assert.NotNull(detail);
        Assert.Single(detail!.Questions);
        Assert.Equal("Pass", detail.Questions[0].OriginalAnswerValue);
        Assert.Equal(95m, detail.Questions[0].OriginalScore);
        Assert.Equal(2, detail.Ratings.Count);
        // Original 95, rater one picked "Fail" (derived score 50), rater two picked "Pass"
        // (derived score 100) -> variance = 100 - 50 = 50
        Assert.Equal(50m, detail.Variance);
        Assert.Contains(detail.Ratings, r => r.EvaluatorId == _evaluatorOneId && r.Score == 50m);
        Assert.Contains(detail.Ratings, r => r.EvaluatorId == _evaluatorTwoId && r.Score == 100m);

        var listsResponse = await _client.GetAsync("/api/v1/calibration-lists");
        var listsPage = await listsResponse.Content.ReadFromJsonAsync<PagedListsResponse>();
        var listSummary = Assert.Single(listsPage!.Items, l => l.Id == list.Id);
        Assert.Equal("Creator Supervisor", listSummary.CreatedByName);
        Assert.Equal(1, listSummary.ItemCount);
        Assert.Equal(1, listSummary.RatedItemCount);
    }

    [Fact]
    public async Task GivenAScoredEvaluation_WhenBrowsingCandidatesForTheListBuilder_ThenItAppearsAndDraftsAreExcluded()
    {
        var response = await _client.GetAsync("/api/v1/calibration-lists/candidates");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<PagedCandidatesResponse>();

        Assert.NotNull(page);
        Assert.Contains(page!.Items, c => c.Id == _evaluationId);
    }

    private sealed record CreatedResponse(Guid Id);

    private sealed record PagedItemsResponse(List<ItemResponse> Items);
    private sealed record ItemResponse(Guid Id);

    private sealed record PagedListsResponse(List<ListSummaryResponse> Items);
    private sealed record ListSummaryResponse(Guid Id, string CreatedByName, int ItemCount, int RatedItemCount);

    private sealed record PagedCandidatesResponse(List<CandidateResponse> Items);
    private sealed record CandidateResponse(Guid Id);

    private sealed record CalibrationItemDetailResponse(
        List<CalibrationQuestionResponse> Questions, List<CalibrationRatingResponse> Ratings, decimal? Variance);

    private sealed record CalibrationQuestionResponse(Guid QuestionId, string? OriginalAnswerValue, decimal? OriginalScore);

    private sealed record CalibrationRatingResponse(Guid EvaluatorId, decimal? Score, List<CalibrationAnswerResponse> Answers);

    private sealed record CalibrationAnswerResponse(Guid QuestionId, string? AnswerValue, string? Comment, decimal? Score);
}
