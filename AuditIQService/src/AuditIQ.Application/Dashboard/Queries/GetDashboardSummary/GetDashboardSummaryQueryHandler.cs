using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using AuditIQ.Application.Abstractions.Time;
using AuditIQ.Application.Dashboard.Dtos;
using AuditIQ.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Dashboard.Queries.GetDashboardSummary;

public sealed class GetDashboardSummaryQueryHandler(IApplicationDbContext db, IDateTimeProvider clock)
    : IQueryHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    private const int TrendWeeks = 12;

    public async Task<Result<DashboardSummaryDto>> Handle(GetDashboardSummaryQuery query, CancellationToken cancellationToken)
    {
        var completedEvaluations = db.Evaluations.Where(e => e.Status != EvaluationStatus.Draft);

        // "All Scorecards" / "All Supervisors" filters, matching Scorebuddy's Overview dropdowns.
        // A chosen scorecard filters to every version sharing its ScorecardGroupId — an
        // evaluation may have been scored against an older version of the same template.
        if (query.ScorecardId is { } scorecardId)
        {
            var groupId = await db.Scorecards
                .Where(s => s.Id == scorecardId)
                .Select(s => s.ScorecardGroupId)
                .FirstOrDefaultAsync(cancellationToken);
            var versionIds = db.Scorecards.Where(s => s.ScorecardGroupId == groupId).Select(s => s.Id);
            completedEvaluations = completedEvaluations.Where(e => versionIds.Contains(e.ScorecardId));
        }

        if (query.SupervisorId is { } supervisorId)
        {
            var directReportIds = db.Users.Where(u => u.SupervisorId == supervisorId).Select(u => u.Id);
            completedEvaluations = completedEvaluations.Where(e => directReportIds.Contains(e.AgentId));
        }

        var averageScore = await completedEvaluations
            .Where(e => (e.LegacyTotalScore ?? e.TotalScore) != null)
            .Select(e => e.LegacyTotalScore ?? e.TotalScore)
            .AverageAsync(cancellationToken);

        var evaluationsCompleted = await completedEvaluations.CountAsync(cancellationToken);
        var disputedCount = await completedEvaluations.CountAsync(e => e.Status == EvaluationStatus.Disputed, cancellationToken);
        // Proxy for Scorebuddy's "Fails" — a scored evaluation that landed at 0%, which is what
        // both the Fail All cascade and a run of all-zero answers produce (ScoreCalculator.cs).
        var failsCount = await completedEvaluations
            .CountAsync(e => (e.LegacyTotalScore ?? e.TotalScore) == 0m, cancellationToken);

        var totalAgents = await db.Users.CountAsync(u => u.Role == UserRole.Agent && u.IsActive && !u.IsDeleted, cancellationToken);
        var agentsWithEvaluations = await completedEvaluations.Select(e => e.AgentId).Distinct().CountAsync(cancellationToken);
        var agentCoveragePercent = Percent(agentsWithEvaluations, totalAgents);

        var totalActiveScorecards = await db.Scorecards.CountAsync(s => s.IsCurrentVersion && !s.IsArchived, cancellationToken);
        var activeGroupIds = db.Scorecards.Where(s => s.IsCurrentVersion && !s.IsArchived).Select(s => s.ScorecardGroupId);
        var usedActiveScorecards = await completedEvaluations
            .Join(db.Scorecards, e => e.ScorecardId, s => s.Id, (e, s) => s.ScorecardGroupId)
            .Distinct()
            .Where(groupId => activeGroupIds.Contains(groupId))
            .CountAsync(cancellationToken);
        var scorecardAdoptionPercent = Percent(usedActiveScorecards, totalActiveScorecards);

        // "Complete" per calibration list item = at least one calibrator has rated it
        // (lists themselves have no status now — visibility is scope-based, not a
        // pre-invited-participant workflow, so completion is tracked per item instead).
        var totalCalibrationItems = await db.CalibrationListItems.CountAsync(cancellationToken);
        var ratedCalibrationItems = await db.CalibrationListItems
            .CountAsync(i => i.Ratings.Any(r => r.Score != null), cancellationToken);
        var calibrationCompletionPercent = Percent(ratedCalibrationItems, totalCalibrationItems);

        var cutoff = clock.UtcNow.AddDays(-7 * TrendWeeks);
        var recentScores = await completedEvaluations
            .Where(e => (e.LegacyTotalScore ?? e.TotalScore) != null && e.SubmittedAt != null && e.SubmittedAt >= cutoff)
            .Select(e => new { e.SubmittedAt, Score = e.LegacyTotalScore ?? e.TotalScore })
            .ToListAsync(cancellationToken);

        var trend = BuildWeeklyTrend(recentScores.Select(r => (r.SubmittedAt!.Value, r.Score!.Value)), clock.UtcNow, TrendWeeks);

        return new DashboardSummaryDto(
            averageScore,
            evaluationsCompleted,
            disputedCount,
            failsCount,
            agentCoveragePercent,
            scorecardAdoptionPercent,
            calibrationCompletionPercent,
            trend);
    }

    private static decimal Percent(int part, int whole) => whole > 0 ? Math.Round(part * 100m / whole, 1) : 0m;

    private static IReadOnlyList<WeeklyScorePointDto> BuildWeeklyTrend(
        IEnumerable<(DateTimeOffset SubmittedAt, decimal Score)> scores, DateTimeOffset now, int weeks)
    {
        var currentWeekStart = DateOnly.FromDateTime(now.UtcDateTime.AddDays(-(int)now.UtcDateTime.DayOfWeek));
        var buckets = new List<(DateOnly WeekStart, List<decimal> Scores)>();
        for (var i = weeks - 1; i >= 0; i--)
            buckets.Add((currentWeekStart.AddDays(-7 * i), []));

        foreach (var (submittedAt, score) in scores)
        {
            var day = DateOnly.FromDateTime(submittedAt.UtcDateTime);
            var weekStart = day.AddDays(-(int)submittedAt.UtcDateTime.DayOfWeek);
            var bucket = buckets.FirstOrDefault(b => b.WeekStart == weekStart);
            bucket.Scores?.Add(score);
        }

        return buckets
            .Select(b => new WeeklyScorePointDto(b.WeekStart, b.Scores.Count > 0 ? Math.Round(b.Scores.Average(), 1) : null))
            .ToList();
    }
}
