using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using AuditIQ.Application.Abstractions.Time;
using AuditIQ.Application.Dashboard.Dtos;
using AuditIQ.Application.Users;
using AuditIQ.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Dashboard.Queries.GetAgentDashboard;

public sealed class GetAgentDashboardQueryHandler(IApplicationDbContext db, IDateTimeProvider clock)
    : IQueryHandler<GetAgentDashboardQuery, AgentDashboardDto>
{
    public async Task<Result<AgentDashboardDto>> Handle(GetAgentDashboardQuery query, CancellationToken cancellationToken)
    {
        var agent = await db.Users.FindAsync([query.AgentId], cancellationToken);
        if (agent is null)
            return Result.Failure<AgentDashboardDto>(UserErrors.NotFound(query.AgentId));

        var team = await db.Teams
            .Where(t => t.UserTeams.Any(ut => ut.UserId == query.AgentId))
            .Select(t => new { t.Id, t.Name })
            .FirstOrDefaultAsync(cancellationToken);

        var cutoff = clock.UtcNow.AddDays(-query.Days);

        var myScores = await db.Evaluations
            .Where(e => e.AgentId == query.AgentId && e.Status != EvaluationStatus.Draft
                && (e.LegacyTotalScore ?? e.TotalScore) != null && e.SubmittedAt != null && e.SubmittedAt >= cutoff)
            .Select(e => new { e.SubmittedAt, Score = (e.LegacyTotalScore ?? e.TotalScore)!.Value })
            .ToListAsync(cancellationToken);

        List<(DateTimeOffset SubmittedAt, decimal Score)> teamScores = [];
        if (team is not null)
        {
            teamScores = (await db.Evaluations
                .Where(e => e.Status != EvaluationStatus.Draft && (e.LegacyTotalScore ?? e.TotalScore) != null
                    && e.SubmittedAt != null && e.SubmittedAt >= cutoff
                    && e.Agent!.UserTeams.Any(ut => ut.TeamId == team.Id))
                .Select(e => new { e.SubmittedAt, Score = (e.LegacyTotalScore ?? e.TotalScore)!.Value })
                .ToListAsync(cancellationToken))
                .Select(x => (x.SubmittedAt!.Value, x.Score))
                .ToList();
        }

        var myOverallScore = myScores.Count > 0 ? Math.Round(myScores.Average(s => s.Score), 2) : (decimal?)null;
        var teamOverallScore = teamScores.Count > 0 ? Math.Round(teamScores.Average(s => s.Score), 2) : (decimal?)null;

        var kudosCount = await db.Kudos.CountAsync(k => k.ToUserId == query.AgentId && k.CreatedAt >= cutoff, cancellationToken);

        var weeklyTrend = BuildWeeklyTrend(
            myScores.Select(s => (s.SubmittedAt!.Value, s.Score)),
            teamScores,
            clock.UtcNow,
            query.Days);

        var recentScores = await db.Evaluations
            .Where(e => e.AgentId == query.AgentId && e.Status != EvaluationStatus.Draft)
            .OrderByDescending(e => e.SubmittedAt)
            .Take(5)
            .Select(e => new RecentScoreDto(e.Id, e.Scorecard!.Name, e.Status.ToString(), e.SubmittedAt, e.LegacyTotalScore ?? e.TotalScore))
            .ToListAsync(cancellationToken);

        return new AgentDashboardDto(
            agent.DisplayName, team?.Name, myOverallScore, teamOverallScore, kudosCount,
            FlagCount: 0, weeklyTrend, recentScores);
    }

    private static IReadOnlyList<AgentWeeklyScorePointDto> BuildWeeklyTrend(
        IEnumerable<(DateTimeOffset SubmittedAt, decimal Score)> myScores,
        IEnumerable<(DateTimeOffset SubmittedAt, decimal Score)> teamScores,
        DateTimeOffset now, int days)
    {
        var weeks = Math.Max(1, (int)Math.Ceiling(days / 7.0));
        var currentWeekStart = DateOnly.FromDateTime(now.UtcDateTime.AddDays(-(int)now.UtcDateTime.DayOfWeek));

        var buckets = new List<(DateOnly WeekStart, List<decimal> Mine, List<decimal> Team)>();
        for (var i = weeks - 1; i >= 0; i--)
            buckets.Add((currentWeekStart.AddDays(-7 * i), [], []));

        void Fill(IEnumerable<(DateTimeOffset SubmittedAt, decimal Score)> scores, bool mine)
        {
            foreach (var (submittedAt, score) in scores)
            {
                var day = DateOnly.FromDateTime(submittedAt.UtcDateTime);
                var weekStart = day.AddDays(-(int)submittedAt.UtcDateTime.DayOfWeek);
                var bucket = buckets.FirstOrDefault(b => b.WeekStart == weekStart);
                (mine ? bucket.Mine : bucket.Team)?.Add(score);
            }
        }

        Fill(myScores, mine: true);
        Fill(teamScores, mine: false);

        return buckets
            .Select(b => new AgentWeeklyScorePointDto(
                b.WeekStart,
                b.Mine.Count > 0 ? Math.Round(b.Mine.Average(), 1) : null,
                b.Team.Count > 0 ? Math.Round(b.Team.Average(), 1) : null))
            .ToList();
    }
}
