using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace AuditIQ.Api.Auth;

/// <summary>
/// Stands in for the real org SSO's JWT bearer validation (Authority/Audience are still TBD,
/// Section 13) so the frontend's dev-only login can reach a running API locally.
/// Registered only when the environment is Development (see Program.cs) — never wired into
/// Staging/Production, where the real JwtBearer scheme is used.
///
/// Two token shapes:
///  - "dev-placeholder-token" (the original, plain dev-login) — grants Admin directly, no email
///    claim, matching every curl command and test written against this session so far.
///  - "dev:{email}" — carries only an email claim and NO role claim, so
///    AuditIqRoleClaimsTransformation resolves the real AuditIQ role for that email from the
///    Users table, exactly like it would for a real Azure AD token. This is what lets the
///    frontend's "Continue as Admin" / "Continue as Agent" dev-login buttons actually exercise
///    real role-based behavior instead of always being Admin.
/// </summary>
public class DevAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Dev";
    private const string EmailTokenPrefix = "dev:";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var header) || !header.ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(AuthenticateResult.Fail("Missing bearer token."));

        var token = header.ToString()["Bearer ".Length..].Trim();

        var claims = new List<Claim> { new(ClaimTypes.Name, "dev-user") };
        if (token.StartsWith(EmailTokenPrefix, StringComparison.OrdinalIgnoreCase))
            claims.Add(new Claim(ClaimTypes.Email, token[EmailTokenPrefix.Length..]));
        else
            claims.Add(new Claim(ClaimTypes.Role, Roles.Admin));

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
