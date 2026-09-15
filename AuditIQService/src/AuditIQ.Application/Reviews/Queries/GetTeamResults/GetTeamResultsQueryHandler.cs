using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Persistence;
using AuditIQ.Application.Abstractions.Results;
using AuditIQ.Application.Reviews.Dtos;
using AuditIQ.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AuditIQ.Application.Reviews.Queries.GetTeamResults;

public sealed class GetTeamResultsQueryHandler(IApplicationDbContext db) : IQueryHandler<GetTeamResultsQuery, TeamResultsDto>
{
    public async Task<Result<TeamResultsDto>> Handle(GetTeamResultsQuery query, CancellationToken cancellationToken)
    {
        var team = await db.Teams.FirstOrDefaultAsync(t => t.Id == query.TeamId, cancellationToken);
        if (team is null)
            return Result.Failure<TeamResultsDto>(ReviewErrors.TeamNotFound(query.TeamId));

        var requester = await db.Users.FirstOrDefaultAsync(u => u.Id == query.RequestingUserId, cancellationToken);
        if (requester is null)
            return Result.Failure<TeamResultsDto>(ReviewErrors.UserNotFound(query.RequestingUserId));

        if (requester.Role != UserRole.Admin)
        {
            var supervisesTeamMember = await db.Users
                .AnyAsync(u => u.SupervisorId == requester.Id && u.UserTeams.Any(ut => ut.TeamId == query.TeamId), cancellationToken);
            if (!supervisesTeamMember)
                return Result.Failure<TeamResultsDto>(ReviewErrors.NotAuthorizedForTeam);
        }

        var agentIds = await db.Users
            .Where(u => u.UserTeams.Any(ut => ut.TeamId == query.TeamId))
            .Select(u => new { u.Id, u.DisplayName })
            .ToListAsync(cancellationToken);

        var agents = new List<AgentResultDto>();
        foreach (var agent in agentIds)
        {
            var scores = await db.Evaluations
                .Where(e => e.AgentId == agent.Id && (e.LegacyTotalScore ?? e.TotalScore) != null)
                .Select(e => (e.LegacyTotalScore ?? e.TotalScore)!.Value)
                .ToListAsync(cancellationToken);

            var average = scores.Count > 0 ? Math.Round(scores.Average(), 2) : (decimal?)null;
            agents.Add(new AgentResultDto(agent.Id, agent.DisplayName, average, scores.Count));
        }

        return new TeamResultsDto(team.Id, team.Name, agents);
    }
}
