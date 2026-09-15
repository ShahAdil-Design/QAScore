namespace AuditIQ.Application.Dashboard.Dtos;

public sealed record WeeklyScorePointDto(DateOnly WeekStart, decimal? AverageScore);

/// <summary>
/// Every figure here is computed from real Evaluations/Scorecards/CalibrationSessions data —
/// no placeholder numbers. "Completed" evaluations means any non-Draft status (Submitted through
/// Resolved), matching how EvaluationStatus already models the workflow.
/// </summary>
public sealed record DashboardSummaryDto(
    decimal? AverageScore,
    int EvaluationsCompleted,
    int DisputedCount,
    int FailsCount,
    decimal AgentCoveragePercent,
    decimal ScorecardAdoptionPercent,
    decimal CalibrationCompletionPercent,
    IReadOnlyList<WeeklyScorePointDto> WeeklyTrend);
