using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace AuditIQ.IntegrationTests;

public class LookupsApiTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory = new();
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        await _factory.InitializeDatabaseAsync();
        _client = _factory.CreateClient();
        // Defaults to Admin via TestAuthHandler — lookup writes require RequireAdmin.
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _factory.DropDatabaseAsync();
    }

    [Fact]
    public async Task GivenValidText_WhenCreatingACauseCode_ThenItAppearsInTheList()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/v1/lookups/cause-codes", new { text = "Agent failed to verify identity" });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var listResponse = await _client.GetAsync("/api/v1/lookups/cause-codes");
        var list = await listResponse.Content.ReadFromJsonAsync<List<LookupItemResponse>>();
        Assert.Contains(list!, i => i.Text == "Agent failed to verify identity");
    }

    [Fact]
    public async Task GivenADuplicateText_WhenCreatingACauseCode_ThenItIsRejected()
    {
        await _client.PostAsJsonAsync("/api/v1/lookups/cause-codes", new { text = "Duplicate cause" });
        var secondResponse = await _client.PostAsJsonAsync("/api/v1/lookups/cause-codes", new { text = "Duplicate cause" });

        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    [Fact]
    public async Task GivenAnExistingCauseCode_WhenUpdatingThenDeleting_ThenBothSucceedAndItDisappears()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/v1/lookups/cause-codes", new { text = "Original text" });
        var created = await createResponse.Content.ReadFromJsonAsync<CreatedResponse>();

        var updateResponse = await _client.PutAsJsonAsync($"/api/v1/lookups/cause-codes/{created!.Id}", new { text = "Revised text" });
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var deleteResponse = await _client.DeleteAsync($"/api/v1/lookups/cause-codes/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var listResponse = await _client.GetAsync("/api/v1/lookups/cause-codes");
        var list = await listResponse.Content.ReadFromJsonAsync<List<LookupItemResponse>>();
        Assert.DoesNotContain(list!, i => i.Id == created.Id);
    }

    [Fact]
    public async Task GivenValidText_WhenCreatingAComment_ThenItAppearsInTheList()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/v1/lookups/comments", new { text = "Great empathy shown throughout the call" });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var listResponse = await _client.GetAsync("/api/v1/lookups/comments");
        var list = await listResponse.Content.ReadFromJsonAsync<List<LookupItemResponse>>();
        Assert.Contains(list!, i => i.Text == "Great empathy shown throughout the call");
    }

    [Fact]
    public async Task GivenADuplicateText_WhenCreatingAComment_ThenItIsRejected()
    {
        await _client.PostAsJsonAsync("/api/v1/lookups/comments", new { text = "Duplicate comment" });
        var secondResponse = await _client.PostAsJsonAsync("/api/v1/lookups/comments", new { text = "Duplicate comment" });

        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    private sealed record CreatedResponse(Guid Id);

    private sealed record LookupItemResponse(Guid Id, string Text);
}
