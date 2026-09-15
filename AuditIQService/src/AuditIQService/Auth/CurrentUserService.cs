using AuditIQ.Application.Abstractions.Auth;

namespace AuditIQ.Api.Auth;

/// <summary>Reads the caller's AuditIQ user id (populated by AuditIqRoleClaimsTransformation)
/// off the current request's ClaimsPrincipal. Null outside a request (background jobs) or when
/// the principal never resolved to an AuditIQ user (e.g. DevAuthHandler's placeholder token).</summary>
public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid? UserId
    {
        get
        {
            var claim = httpContextAccessor.HttpContext?.User.FindFirst(AuditIqRoleClaimsTransformation.UserIdClaimType)?.Value;
            return Guid.TryParse(claim, out var id) ? id : null;
        }
    }
}
