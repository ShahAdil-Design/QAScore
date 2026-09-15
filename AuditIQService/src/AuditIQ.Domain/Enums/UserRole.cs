namespace AuditIQ.Domain.Enums;

public enum UserRole
{
    Admin,
    Supervisor,
    TeamLead,
    QaEvaluator,
    Agent,

    // Scorebuddy "Manage Users" admin-panel roles (distinct resource from /staff, see
    // AuditIQ.Migration's AdminUserRoleMapper) — appended rather than interleaved so existing
    // rows' stored int values never shift. Deliberately absent from every AuthorizationPolicies
    // RequireRole(...) allow-list and seeded with ScreenPermissions.IsVisible = false everywhere:
    // people land in these roles able to authenticate but unable to do anything, until a
    // deliberate decision is made to grant them specific policy/screen access.
    GroupAdmin,
    TeamAdmin,
    ReportsAnalyst,
    CalibrationAnalyst,
}
