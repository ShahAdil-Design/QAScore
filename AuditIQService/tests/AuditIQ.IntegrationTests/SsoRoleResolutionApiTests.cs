using System.Net;
using System.Net.Http.Json;
using AuditIQ.Domain.Entities;
using AuditIQ.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AuditIQ.IntegrationTests;

/// <summary>
/// Proves the actual design decision: the identity provider (Azure AD in production, this test's
/// X-Test-Email header standing in for it) only proves who someone is. What they're allowed to do
/// comes from AuditIQ's own Users.Role — resolved by AuditIqRoleClaimsTransformation, which is a
/// real DI-registered service in Program.cs and therefore runs in this WebApplicationFactory host
/// exactly as it would in production, regardless of which auth scheme authenticated the request.
/// </summary>
public class SsoRoleResolutionApiTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory = new();
    private HttpClient _client = null!;
    private Guid _agentUserId;

    public async Task InitializeAsync()
    {
        await _factory.InitializeDatabaseAsync();
        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuditIQ.Infrastructure.Persistence.AuditIQDbContext>();

        var admin = new User { SsoSubjectId = "sso:admin", DisplayName = "Real Admin", Email = "real.admin@test.com", Role = UserRole.Admin };
        var agent = new User { SsoSubjectId = "sso:agent", DisplayName = "Real Agent", Email = "real.agent@test.com", Role = UserRole.Agent };
        db.Users.AddRange(admin, agent);
        await db.SaveChangesAsync();
        _agentUserId = agent.Id;
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _factory.DropDatabaseAsync();
    }

    [Fact]
    public async Task GivenAnEmailMatchingAnAdminUser_WhenCallingAnAdminGatedEndpoint_ThenItSucceeds()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Email", "real.admin@test.com");

        var response = await _client.PostAsJsonAsync("/api/v1/scorecard-categories", new { name = "SSO-resolved category" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GivenAnEmailMatchingAnAgentUser_WhenCallingAnAdminGatedEndpoint_ThenItIsForbidden()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Email", "real.agent@test.com");

        var response = await _client.PostAsJsonAsync("/api/v1/scorecard-categories", new { name = "Should never be created" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GivenAnEmailWithNoMatchingAuditIqUser_WhenCallingAnyGatedEndpoint_ThenAccessIsDeniedNotDefaulted()
    {
        // Authenticates fine (a real identity) but AuditIQ has never provisioned this person —
        // must NOT silently fall back to some default role.
        _client.DefaultRequestHeaders.Add("X-Test-Email", "nobody-provisioned@test.com");

        var response = await _client.PostAsJsonAsync("/api/v1/scorecard-categories", new { name = "Should never be created" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GivenAnEmailMatchingARealUser_WhenCallingMe_ThenItResolvesTheirOwnProfile()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Email", "real.agent@test.com");

        var response = await _client.GetAsync("/api/v1/users/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var me = await response.Content.ReadFromJsonAsync<MeResponse>();
        Assert.Equal(_agentUserId, me!.Id);
        Assert.Equal("Real Agent", me.DisplayName);
        Assert.Equal("Agent", me.Role);
    }

    [Fact]
    public async Task GivenAnUnprovisionedEmail_WhenCallingMe_ThenItReturnsNotProvisioned()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Email", "nobody-provisioned@test.com");

        var response = await _client.GetAsync("/api/v1/users/me");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private sealed record MeResponse(Guid Id, string DisplayName, string Role);
}
