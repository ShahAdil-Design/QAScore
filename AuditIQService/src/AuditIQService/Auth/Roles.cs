namespace AuditIQ.Api.Auth;

/// <summary>
/// Roles from Section 5 (users table). Values must match the role claim issued by the org SSO —
/// confirm exact claim name/values against the SSO's OIDC token before go-live (Section 13).
/// </summary>
public static class Roles
{
    public const string Admin = "Admin";
    public const string Supervisor = "Supervisor";
    public const string TeamLead = "TeamLead";
    public const string QaEvaluator = "QaEvaluator";
    public const string Agent = "Agent";

    // Scorebuddy admin-panel roles — see UserRole.cs. Not used in any AuthorizationPolicies
    // RequireRole(...) list yet; exist so the role claim/DB value round-trips correctly.
    public const string GroupAdmin = "GroupAdmin";
    public const string TeamAdmin = "TeamAdmin";
    public const string ReportsAnalyst = "ReportsAnalyst";
    public const string CalibrationAnalyst = "CalibrationAnalyst";
}
