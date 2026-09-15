using System.Net;
using System.Net.Http.Json;
using AuditIQ.Domain.Entities;
using AuditIQ.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AuditIQ.IntegrationTests;

public class ScorecardsApiTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory = new();
    private HttpClient _client = null!;
    private Guid _categoryId;
    private Guid _groupId;

    public async Task InitializeAsync()
    {
        await _factory.InitializeDatabaseAsync();
        _client = _factory.CreateClient();
        // Defaults to Admin via TestAuthHandler — Scorecard/category writes require RequireAdmin.

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuditIQDbContext>();

        var category = new ScorecardCategory { Name = "Phone Call" };
        var group = new Group { Name = "Sigma" };
        db.ScorecardCategories.Add(category);
        db.Groups.Add(group);
        await db.SaveChangesAsync();

        _categoryId = category.Id;
        _groupId = group.Id;
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _factory.DropDatabaseAsync();
    }

    private object PassFailAnswers() => new[]
    {
        new { label = "Pass", value = 100, isFailSection = false, isFailAll = false, isNotApplicable = false },
        new { label = "Fail", value = 0, isFailSection = false, isFailAll = false, isNotApplicable = false },
    };

    [Fact]
    public async Task GivenValidInput_WhenCreatingAScorecard_ThenItIsReturnedAsTheCurrentVersionWithAnswerValuesAndGroups()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/v1/scorecards", new
        {
            name = "Core QA",
            description = "Baseline QA scorecard",
            scorecardType = "Standard",
            categoryId = _categoryId,
            maxScore = 100,
            location = "London",
            groupIds = new[] { _groupId },
            questions = new[]
            {
                new
                {
                    sectionName = "Opening",
                    text = "Verified customer identity",
                    weight = 100,
                    isFailLogic = false,
                    sortOrder = 0,
                    answerOptions = PassFailAnswers(),
                },
            },
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<CreatedResponse>();
        Assert.NotNull(created);

        var getResponse = await _client.GetAsync($"/api/v1/scorecards/{created!.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var detail = await getResponse.Content.ReadFromJsonAsync<ScorecardDetailResponse>();

        Assert.NotNull(detail);
        Assert.Equal("Core QA", detail!.Name);
        Assert.Equal("London", detail.Location);
        Assert.Equal(1, detail.Version);
        Assert.False(detail.IsLocked);
        Assert.False(detail.IsArchived);
        Assert.Contains("Sigma", detail.GroupNames);
        Assert.Single(detail.Questions);
        Assert.Equal(2, detail.Questions[0].AnswerOptions.Count);
        Assert.Equal(100m, detail.Questions[0].MaxScore);
        Assert.Equal(100m, detail.Questions[0].AnswerOptions[0].Value);
    }

    [Fact]
    public async Task GivenAScorecardWithNoQuestionsYet_WhenCreating_ThenItIsSavedAsAShell()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/v1/scorecards", new
        {
            name = "Shell Scorecard",
            description = (string?)null,
            scorecardType = "Standard",
            categoryId = _categoryId,
            maxScore = 100,
            location = (string?)null,
            groupIds = Array.Empty<Guid>(),
            questions = Array.Empty<object>(),
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<CreatedResponse>();

        var getResponse = await _client.GetAsync($"/api/v1/scorecards/{created!.Id}");
        var detail = await getResponse.Content.ReadFromJsonAsync<ScorecardDetailResponse>();
        Assert.NotNull(detail);
        Assert.Empty(detail!.Questions);
        Assert.Empty(detail.GroupNames);
    }

    [Fact]
    public async Task GivenAFailSectionOrNotApplicableAnswer_WhenCreatingWithANonZeroValue_ThenItIsRejected()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/scorecards", new
        {
            name = "Invalid Scorecard",
            description = (string?)null,
            scorecardType = "Standard",
            categoryId = _categoryId,
            maxScore = 100,
            location = (string?)null,
            groupIds = Array.Empty<Guid>(),
            questions = new[]
            {
                new
                {
                    sectionName = "Opening",
                    text = "Verified identity",
                    weight = 100,
                    isFailLogic = false,
                    sortOrder = 0,
                    answerOptions = new[]
                    {
                        new { label = "Breach", value = 50, isFailSection = true, isFailAll = false, isNotApplicable = false },
                    },
                },
            },
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GivenAnExistingScorecard_WhenUpdating_ThenANewVersionIsCreatedAndTheOldOneIsSuperseded()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/v1/scorecards", new
        {
            name = "Core QA",
            description = (string?)null,
            scorecardType = "Standard",
            categoryId = _categoryId,
            maxScore = 100,
            location = (string?)null,
            groupIds = Array.Empty<Guid>(),
            questions = new[]
            {
                new { sectionName = "Opening", text = "Verified identity", weight = 100, isFailLogic = false, sortOrder = 0, answerOptions = PassFailAnswers() },
            },
        });
        var created = await createResponse.Content.ReadFromJsonAsync<CreatedResponse>();

        var updateResponse = await _client.PutAsJsonAsync($"/api/v1/scorecards/{created!.Id}", new
        {
            name = "Core QA v2",
            description = "Revised wording",
            scorecardType = "Standard",
            categoryId = _categoryId,
            maxScore = 100,
            location = (string?)null,
            groupIds = Array.Empty<Guid>(),
            questions = new[]
            {
                new { sectionName = "Opening", text = "Verified identity and DOB", weight = 100, isFailLogic = false, sortOrder = 0, answerOptions = PassFailAnswers() },
            },
        });

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<IdResponse>();
        Assert.NotNull(updated);
        Assert.NotEqual(created.Id, updated!.Id);

        var newGetResponse = await _client.GetAsync($"/api/v1/scorecards/{updated.Id}");
        var newDetail = await newGetResponse.Content.ReadFromJsonAsync<ScorecardDetailResponse>();
        Assert.NotNull(newDetail);
        Assert.Equal("Core QA v2", newDetail!.Name);
        Assert.Equal(2, newDetail.Version);

        // The old version is still readable by id (evaluations may still reference it) but no
        // longer appears in the active list — only the new current version does.
        var oldGetResponse = await _client.GetAsync($"/api/v1/scorecards/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, oldGetResponse.StatusCode);

        var listResponse = await _client.GetAsync("/api/v1/scorecards");
        var list = await listResponse.Content.ReadFromJsonAsync<List<ScorecardSummaryResponse>>();
        Assert.DoesNotContain(list!, s => s.Id == created.Id);
        Assert.Contains(list!, s => s.Id == updated.Id);
    }

    [Fact]
    public async Task GivenACurrentScorecard_WhenArchiving_ThenItNoLongerAppearsInTheActiveList()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/v1/scorecards", new
        {
            name = "Retiring Scorecard",
            description = (string?)null,
            scorecardType = "Standard",
            categoryId = _categoryId,
            maxScore = 100,
            location = (string?)null,
            groupIds = Array.Empty<Guid>(),
            questions = new[]
            {
                new { sectionName = "Opening", text = "Verified identity", weight = 100, isFailLogic = false, sortOrder = 0, answerOptions = PassFailAnswers() },
            },
        });
        var created = await createResponse.Content.ReadFromJsonAsync<CreatedResponse>();

        var deleteResponse = await _client.DeleteAsync($"/api/v1/scorecards/{created!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var listResponse = await _client.GetAsync("/api/v1/scorecards");
        var list = await listResponse.Content.ReadFromJsonAsync<List<ScorecardSummaryResponse>>();
        Assert.DoesNotContain(list!, s => s.Id == created.Id);

        // Archiving twice is a conflict, not a silent no-op.
        var secondDeleteResponse = await _client.DeleteAsync($"/api/v1/scorecards/{created.Id}");
        Assert.Equal(HttpStatusCode.Conflict, secondDeleteResponse.StatusCode);
    }

    [Fact]
    public async Task GivenACategoryInUseByAScorecard_WhenDeletingTheCategory_ThenItIsRejected()
    {
        await _client.PostAsJsonAsync("/api/v1/scorecards", new
        {
            name = "Uses The Category",
            description = (string?)null,
            scorecardType = "Standard",
            categoryId = _categoryId,
            maxScore = 100,
            location = (string?)null,
            groupIds = Array.Empty<Guid>(),
            questions = new[]
            {
                new { sectionName = "Opening", text = "Verified identity", weight = 100, isFailLogic = false, sortOrder = 0, answerOptions = PassFailAnswers() },
            },
        });

        var deleteResponse = await _client.DeleteAsync($"/api/v1/scorecard-categories/{_categoryId}");
        Assert.Equal(HttpStatusCode.Conflict, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task GivenNoScorecardsReferenceIt_WhenDeletingACategory_ThenItSucceeds()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/v1/scorecard-categories", new { name = "Unused Category" });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<CreatedResponse>();

        var deleteResponse = await _client.DeleteAsync($"/api/v1/scorecard-categories/{created!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    private sealed record CreatedResponse(Guid Id);

    private sealed record IdResponse(Guid Id);

    private sealed record ScorecardSummaryResponse(Guid Id, string Name);

    private sealed record ScorecardDetailResponse(
        Guid Id, string Name, string? Description, string ScorecardType, string CategoryName, string? Location,
        int Version, bool IsLocked, bool IsArchived, List<Guid> GroupIds, List<string> GroupNames,
        List<ScorecardQuestionResponse> Questions, List<string> TipSheetTitles);

    private sealed record ScorecardQuestionResponse(
        Guid Id, string SectionName, string Text, int Weight, bool IsFailLogic, int SortOrder,
        List<AnswerOptionResponse> AnswerOptions, decimal MaxScore);

    private sealed record AnswerOptionResponse(string Label, decimal Value, bool IsFailSection, bool IsFailAll, bool IsNotApplicable);
}
