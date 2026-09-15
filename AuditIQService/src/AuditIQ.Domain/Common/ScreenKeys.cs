namespace AuditIQ.Domain.Common;

/// <summary>Canonical catalog of frontend screens a Role's visibility can be toggled for
/// (ScreenPermission.ScreenKey). Adding a screen means adding a key here plus seeding its
/// default-visibility rows — never a schema change.</summary>
public static class ScreenKeys
{
    public const string Dashboard = "dashboard";
    public const string Score = "score";
    public const string Review = "review";
    public const string Calibration = "calibration";
    public const string Reports = "reports";
    public const string Scorecards = "scorecards";
    public const string Staff = "staff";

    public static readonly IReadOnlyList<string> All =
    [
        Dashboard, Score, Review, Calibration, Reports, Scorecards, Staff,
    ];
}
