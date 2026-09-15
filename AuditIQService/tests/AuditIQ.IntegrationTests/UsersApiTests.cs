using System.Net;
using System.Net.Http.Json;
using AuditIQ.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AuditIQ.IntegrationTests;

public class UsersApiTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory = new();
    private HttpClient _client = null!;
    private Guid _groupId;
    private Guid _teamId;

    public async Task InitializeAsync()
    {
        await _factory.InitializeDatabaseAsync();
        _client = _factory.CreateClient();
        // Defaults to Admin via TestAuthHandler — user writes require RequireAdmin.

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuditIQ.Infrastructure.Persistence.AuditIQDbContext>();

        var group = new Group { Name = "Collections" };
        var team = new Team { Name = "Early Stage", GroupId = group.Id };
        db.Groups.Add(group);
        db.Teams.Add(team);
        await db.SaveChangesAsync();

        _groupId = group.Id;
        _teamId = team.Id;
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _factory.DropDatabaseAsync();
    }

    [Fact]
    public async Task GivenValidInput_WhenCreatingAUser_ThenItIsReturnedWithTeamAndGroupMemberships()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/v1/users", new
        {
            displayName = "Nadia Ahmed",
            email = "nadia.ahmed@test.com",
            role = "QaEvaluator",
            employmentType = "Full-time",
            teamIds = new[] { _teamId },
            groupIds = new[] { _groupId },
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<CreatedResponse>();
        Assert.NotNull(created);

        var getResponse = await _client.GetAsync($"/api/v1/users/{created!.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var detail = await getResponse.Content.ReadFromJsonAsync<UserDetailResponse>();

        Assert.NotNull(detail);
        Assert.Equal("Nadia Ahmed", detail!.DisplayName);
        Assert.Equal("QaEvaluator", detail.Role);
        Assert.True(detail.IsActive);
        Assert.Single(detail.Teams);
        Assert.Single(detail.Groups);
    }

    [Fact]
    public async Task GivenAnExistingEmail_WhenCreatingAUser_ThenItIsRejected()
    {
        await _client.PostAsJsonAsync("/api/v1/users", new
        {
            displayName = "First User",
            email = "duplicate@test.com",
            role = "Agent",
            employmentType = (string?)null,
            teamIds = Array.Empty<Guid>(),
            groupIds = Array.Empty<Guid>(),
        });

        var secondResponse = await _client.PostAsJsonAsync("/api/v1/users", new
        {
            displayName = "Second User",
            email = "duplicate@test.com",
            role = "Agent",
            employmentType = (string?)null,
            teamIds = Array.Empty<Guid>(),
            groupIds = Array.Empty<Guid>(),
        });

        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    [Fact]
    public async Task GivenAnExistingUser_WhenChangingTheirRole_ThenTheChangePersists()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/v1/users", new
        {
            displayName = "Role Change Target",
            email = "role-change@test.com",
            role = "Agent",
            employmentType = (string?)null,
            teamIds = Array.Empty<Guid>(),
            groupIds = Array.Empty<Guid>(),
        });
        var created = await createResponse.Content.ReadFromJsonAsync<CreatedResponse>();

        var updateResponse = await _client.PutAsJsonAsync($"/api/v1/users/{created!.Id}", new
        {
            displayName = "Role Change Target",
            email = "role-change@test.com",
            role = "Supervisor",
            employmentType = (string?)null,
            notes = "Promoted",
        });
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/v1/users/{created.Id}");
        var detail = await getResponse.Content.ReadFromJsonAsync<UserDetailResponse>();
        Assert.Equal("Supervisor", detail!.Role);
    }

    [Fact]
    public async Task GivenAnExistingUser_WhenReplacingTeamMemberships_ThenOnlyTheNewSetRemains()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/v1/users", new
        {
            displayName = "Team Swap Target",
            email = "team-swap@test.com",
            role = "Agent",
            employmentType = (string?)null,
            teamIds = new[] { _teamId },
            groupIds = Array.Empty<Guid>(),
        });
        var created = await createResponse.Content.ReadFromJsonAsync<CreatedResponse>();

        var setTeamsResponse = await _client.PutAsJsonAsync($"/api/v1/users/{created!.Id}/teams", new { teamIds = Array.Empty<Guid>() });
        Assert.Equal(HttpStatusCode.NoContent, setTeamsResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/v1/users/{created.Id}");
        var detail = await getResponse.Content.ReadFromJsonAsync<UserDetailResponse>();
        Assert.Empty(detail!.Teams);
    }

    [Fact]
    public async Task GivenAnActiveUser_WhenDeactivatingThenReactivating_ThenStatusFlipsBothWays()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/v1/users", new
        {
            displayName = "Lifecycle Target",
            email = "lifecycle@test.com",
            role = "Agent",
            employmentType = (string?)null,
            teamIds = Array.Empty<Guid>(),
            groupIds = Array.Empty<Guid>(),
        });
        var created = await createResponse.Content.ReadFromJsonAsync<CreatedResponse>();

        var deactivateResponse = await _client.PostAsync($"/api/v1/users/{created!.Id}/deactivate", null);
        Assert.Equal(HttpStatusCode.NoContent, deactivateResponse.StatusCode);

        var secondDeactivateResponse = await _client.PostAsync($"/api/v1/users/{created.Id}/deactivate", null);
        Assert.Equal(HttpStatusCode.Conflict, secondDeactivateResponse.StatusCode);

        var reactivateResponse = await _client.PostAsync($"/api/v1/users/{created.Id}/reactivate", null);
        Assert.Equal(HttpStatusCode.NoContent, reactivateResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/v1/users/{created.Id}");
        var detail = await getResponse.Content.ReadFromJsonAsync<UserDetailResponse>();
        Assert.True(detail!.IsActive);
    }

    private sealed record CreatedResponse(Guid Id);

    private sealed record TeamMembershipResponse(Guid TeamId, string TeamName, Guid GroupId, string GroupName);

    private sealed record GroupMembershipResponse(Guid GroupId, string GroupName);

    private sealed record UserDetailResponse(
        Guid Id, string DisplayName, string Email, string Role, bool IsActive,
        string? EmploymentType, string? Notes,
        List<TeamMembershipResponse> Teams, List<GroupMembershipResponse> Groups);
}
