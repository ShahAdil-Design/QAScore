namespace AuditIQ.Application.Dashboard.Dtos;

public sealed record AgentWeeklyScorePointDto(DateOnly WeekStart, decimal? MyScore, decimal? TeamScore);

public sealed record RecentScoreDto(Guid EvaluationId, string ScorecardName, string Status, DateTimeOffset? SubmittedAt, decimal? TotalScore);

/// <summary>
/// The agent's personal "Employee view" — their own score against their team's, kudos received,
/// and recent activity. "FlagCount" is a placeholder (always 0) — there's no "flagged evaluation"
/// concept in the schema yet; wire it up once that's actually defined instead of guessing.
/// </summary>
public sealed record AgentDashboardDto(
    string AgentName,
    string? TeamName,
    decimal? MyOverallScore,
    decimal? TeamOverallScore,
    int KudosCount,
    int FlagCount,
    IReadOnlyList<AgentWeeklyScorePointDto> WeeklyTrend,
    IReadOnlyList<RecentScoreDto> RecentScores);
