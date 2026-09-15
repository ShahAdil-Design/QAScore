using Microsoft.AspNetCore.Authorization;

namespace AuditIQ.Api.Auth;

/// <summary>
/// RBAC enforced at the API layer (Section 9) — never rely on the UI hiding an action.
/// </summary>
public static class AuthorizationPolicies
{
    public const string RequireAdmin = "RequireAdmin";
    public const string RequireSupervisorOrAbove = "RequireSupervisorOrAbove";
    public const string RequireEvaluator = "RequireEvaluator";

    public static void AddAuditIQPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(RequireAdmin, policy =>
            policy.RequireRole(Roles.Admin));

        options.AddPolicy(RequireSupervisorOrAbove, policy =>
            policy.RequireRole(Roles.Admin, Roles.Supervisor, Roles.TeamLead));

        options.AddPolicy(RequireEvaluator, policy =>
            policy.RequireRole(Roles.Admin, Roles.Supervisor, Roles.TeamLead, Roles.QaEvaluator));
    }
}
