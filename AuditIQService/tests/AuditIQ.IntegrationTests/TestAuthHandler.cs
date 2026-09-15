using System.Security.Claims;
using System.Text.Encodings.Web;
using AuditIQ.Api.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AuditIQ.IntegrationTests;

/// <summary>
/// Stands in for JWT bearer auth in tests, since the real SSO Authority/Audience
/// are still TBD (Section 13). Send the "X-Test-Role" header to control which
/// AuditIQ role the request authenticates as; defaults to Admin.
///
/// Send "X-Test-Email" instead (not combined with X-Test-Role — pick one) to exercise the real
/// AuditIqRoleClaimsTransformation pipeline: it adds a role claim by looking up that email
/// against the test database's Users table, exactly like it would for a real Azure AD token.
/// With no matching user, the request authenticates with an identity but no role at all — the
/// same "authenticated but not provisioned" case the real pipeline produces.
/// </summary>
public class TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Test";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new List<Claim> { new(ClaimTypes.Name, "test-user") };

        if (Request.Headers.TryGetValue("X-Test-Email", out var email))
            claims.Add(new Claim(ClaimTypes.Email, email.ToString()));
        else
            claims.Add(new Claim(ClaimTypes.Role, Request.Headers.TryGetValue("X-Test-Role", out var role) ? role.ToString() : Roles.Admin));

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
