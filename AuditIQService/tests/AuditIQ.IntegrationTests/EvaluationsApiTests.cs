using System.Net;
using System.Net.Http.Json;
using AuditIQ.Domain.Entities;
using AuditIQ.Domain.Enums;
using AuditIQ.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AuditIQ.IntegrationTests;

public class EvaluationsApiTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory = new();
    private HttpClient _client = null!;
    private Guid _scorecardId;
    private Guid _questionId;
    private Guid _agentId;
    private Guid _evaluatorId;

    public async Task InitializeAsync()
    {
        await _factory.InitializeDatabaseAsync();
        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-Test-Role", "QaEvaluator");

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
        question.AnswerOptions.Add(new QuestionAnswerOption { ScorecardQuestionId = question.Id, Label = "Fail", Value = 0 });

        var agent = new User { SsoSubjectId = "it-agent", DisplayName = "Agent", Email = "it-agent@test.com", Role = UserRole.Agent };
        var evaluator = new User { SsoSubjectId = "it-eval", DisplayName = "Evaluator", Email = "it-eval@test.com", Role = UserRole.QaEvaluator };

        db.ScorecardCategories.Add(category);
        db.Scorecards.Add(scorecard);
        db.ScorecardQuestions.Add(question);
        db.Users.AddRange(agent, evaluator);
        await db.SaveChangesAsync();

        _scorecardId = scorecard.Id;
        _questionId = question.Id;
        _agentId = agent.Id;
        _evaluatorId = evaluator.Id;
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        // Dispose the host (stopping Hangfire's background server) before dropping the
        // database — otherwise Hangfire's still-open connections deadlock against the drop.
        await _factory.DisposeAsync();
        await _factory.DropDatabaseAsync();
    }

    [Fact]
    public async Task GivenASeededScorecard_WhenRunningTheFullEvaluationLifecycle_ThenItSucceedsEndToEnd()
    {
        // Create
        var createResponse = await _client.PostAsJsonAsync("/api/v1/evaluations", new
        {
            scorecardId = _scorecardId,
            agentId = _agentId,
            evaluatorId = _evaluatorId,
            reference = "AQ-IT-1",
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<CreatedResponse>();
        Assert.NotNull(created);

        // Save draft answers
        var saveResponse = await _client.PutAsJsonAsync($"/api/v1/evaluations/{created!.Id}/answers", new
        {
            answers = new[] { new { questionId = _questionId, answerValue = "Pass", causeCode = (string?)null, comment = (string?)null, score = 100 } },
        });
        Assert.Equal(HttpStatusCode.NoContent, saveResponse.StatusCode);

        // Submit
        var submitResponse = await _client.PostAsJsonAsync($"/api/v1/evaluations/{created.Id}/submit", new { evaluatorNotes = "Great call" });
        Assert.Equal(HttpStatusCode.NoContent, submitResponse.StatusCode);

        // Fetch and verify
        var getResponse = await _client.GetAsync($"/api/v1/evaluations/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var detail = await getResponse.Content.ReadFromJsonAsync<EvaluationDetailResponse>();

        Assert.NotNull(detail);
        Assert.Equal("Submitted", detail!.Status);
        Assert.Equal(100m, detail.TotalScore);
        Assert.Single(detail.Answers);
        Assert.Equal("Pass", detail.Answers[0].AnswerValue);
    }

    [Fact]
    public async Task GivenNoAuthenticatedRole_WhenCreatingAnEvaluation_ThenForbiddenIsReturned()
    {
        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", "Agent");

        var response = await _client.PostAsJsonAsync("/api/v1/evaluations", new
        {
            scorecardId = _scorecardId,
            agentId = _agentId,
            evaluatorId = _evaluatorId,
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GivenAnAgentWithNoAgentIdFilter_WhenFetchingTheQueue_ThenItDefaultsToTheirOwn()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/v1/evaluations", new
        {
            scorecardId = _scorecardId,
            agentId = _agentId,
            evaluatorId = _evaluatorId,
        });
        var created = await createResponse.Content.ReadFromJsonAsync<CreatedResponse>();

        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Email", "it-agent@test.com");

        var response = await _client.GetAsync("/api/v1/evaluations");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var queue = await response.Content.ReadFromJsonAsync<QueueResponse>();

        Assert.NotNull(queue);
        Assert.Contains(queue!.Items, e => e.Id == created!.Id);
    }

    [Fact]
    public async Task GivenAnAgent_WhenRequestingSomeoneElsesAgentIdExplicitly_ThenItIsForbidden()
    {
        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Email", "it-agent@test.com");

        var response = await _client.GetAsync($"/api/v1/evaluations?agentId={_evaluatorId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GivenAnAgent_WhenViewingTheirOwnEvaluation_ThenEvaluatorNotesAreStripped()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/v1/evaluations", new
        {
            scorecardId = _scorecardId,
            agentId = _agentId,
            evaluatorId = _evaluatorId,
        });
        var created = await createResponse.Content.ReadFromJsonAsync<CreatedResponse>();

        await _client.PutAsJsonAsync($"/api/v1/evaluations/{created!.Id}/answers", new
        {
            answers = new[] { new { questionId = _questionId, answerValue = "Pass", causeCode = (string?)null, comment = (string?)null, score = 100 } },
        });
        await _client.PostAsJsonAsync($"/api/v1/evaluations/{created.Id}/submit", new { evaluatorNotes = "Internal-only observations" });

        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Email", "it-agent@test.com");

        var response = await _client.GetAsync($"/api/v1/evaluations/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var detail = await response.Content.ReadFromJsonAsync<EvaluationDetailResponse>();

        Assert.NotNull(detail);
        Assert.Null(detail!.EvaluatorNotes);
    }

    [Fact]
    public async Task GivenAnAgent_WhenViewingSomeoneElsesEvaluationById_ThenItIsForbidden()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/v1/evaluations", new
        {
            scorecardId = _scorecardId,
            agentId = _evaluatorId,
            evaluatorId = _evaluatorId,
        });
        var created = await createResponse.Content.ReadFromJsonAsync<CreatedResponse>();

        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Email", "it-agent@test.com");

        var response = await _client.GetAsync($"/api/v1/evaluations/{created!.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private sealed record QueueResponse(List<QueueItemResponse> Items);

    private sealed record QueueItemResponse(Guid Id);

    private sealed record CreatedResponse(Guid Id);

    private sealed record EvaluationDetailResponse(string Status, decimal? TotalScore, string? EvaluatorNotes, List<AnswerResponse> Answers);

    private sealed record AnswerResponse(string? AnswerValue);
}
