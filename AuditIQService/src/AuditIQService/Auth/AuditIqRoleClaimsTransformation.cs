using System.Security.Claims;
using AuditIQ.Application.Abstractions.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AuditIQ.Api.Auth;

/// <summary>
/// The org's Azure AD SSO only proves *identity* — it is never the source of truth for what a
/// person can do inside AuditIQ. Role/responsibility assignment lives entirely in AuditIQ's own
/// Users table, managed via the Staff page. This runs after any successful authentication
/// (Azure AD JWT in real environments, DevAuthHandler locally) and resolves the caller's AuditIQ
/// role by matching their token's email against our own database — it never trusts a role/group
/// claim the identity provider might happen to send.
///
/// If a token authenticates as someone with no matching (active, non-deleted) AuditIQ user, the
/// principal is left with no role claim at all — RequireAdmin/RequireEvaluator/etc. then
/// correctly deny access (403), rather than silently defaulting to some role. Provisioning a
/// person in AuditIQ (via the Staff page) is what grants them any access at all, independent of
/// whether they can authenticate.
///
/// Requests with no resolvable email claim (e.g. DevAuthHandler's plain dev-placeholder-token,
/// which carries no email) pass through untouched, preserving that handler's existing
/// Admin-for-local-dev behavior.
/// </summary>
public sealed class AuditIqRoleClaimsTransformation(IApplicationDbContext db, ILogger<AuditIqRoleClaimsTransformation> logger) : IClaimsTransformation
{
    public const string UserIdClaimType = "auditiq:user_id";

    private static readonly string[] EmailClaimTypes =
    [
        ClaimTypes.Email, "email", "preferred_username", "upn",
        ClaimTypes.Upn, "unique_name",
    ];

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var email = EmailClaimTypes
            .Select(principal.FindFirst)
            .FirstOrDefault(c => c is not null)
            ?.Value;

        if (email is null)
        {
            // TEMPORARY diagnostic (dev01 403 investigation) — dumps every claim type actually
            // present on the token so we can see which one carries the email/UPN, since v1 Azure
            // AD access tokens for a custom API don't always include the claims we guessed at.
            // Remove once resolved.
            logger.LogWarning(
                "AuditIqRoleClaimsTransformation: no email-like claim found. Claims present: {Claims}",
                string.Join(", ", principal.Claims.Select(c => $"{c.Type}={c.Value}")));
            return principal;
        }

        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Email == email && u.IsActive && !u.IsDeleted);

        if (user is null)
        {
            logger.LogWarning(
                "AuditIqRoleClaimsTransformation: resolved email claim '{Email}' but no matching active Users row.",
                email);
            return principal;
        }

        if (principal.Identity is not ClaimsIdentity identity)
            return principal;

        identity.AddClaim(new Claim(ClaimTypes.Role, user.Role.ToString()));
        identity.AddClaim(new Claim(UserIdClaimType, user.Id.ToString()));
        identity.AddClaim(new Claim(ClaimTypes.Name, user.DisplayName));

        return principal;
    }
}
