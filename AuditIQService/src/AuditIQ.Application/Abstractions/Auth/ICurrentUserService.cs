namespace AuditIQ.Application.Abstractions.Auth;

/// <summary>Implemented in the API layer (backed by HttpContext/ClaimsPrincipal) so that
/// Infrastructure-layer code — namely AuditLoggingInterceptor — can learn who's making a change
/// without Infrastructure taking a dependency on ASP.NET Core's HTTP types.</summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
}
