using Asp.Versioning;
using AuditIQ.Api.Auth;
using AuditIQ.Api.Extensions;
using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Dashboard.Queries.GetAgentDashboard;
using AuditIQ.Application.Dashboard.Queries.GetAgentsOverview;
using AuditIQ.Application.Dashboard.Queries.GetDashboardSummary;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuditIQ.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/dashboard")]
[Authorize]
public class DashboardController(ISender sender) : ControllerBase
{
    /// <summary>Team/org-wide rollup — never an Agent's own data, so Agent is excluded entirely
    /// rather than scoped to "self" (there's no meaningful self-scope for an aggregate).</summary>
    [HttpGet("summary")]
    [Authorize(Policy = AuthorizationPolicies.RequireEvaluator)]
    public async Task<IActionResult> GetSummary(
        [FromQuery] Guid? scorecardId, [FromQuery] Guid? supervisorId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetDashboardSummaryQuery(scorecardId, ResolveEffectiveSupervisorId(supervisorId)), cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>Supervisor Dashboard's "Agents" tab — one row per agent's aggregate performance.
    /// Never an Agent's own data (it's everyone's), so Agent is excluded entirely, same as GetSummary.</summary>
    [HttpGet("agents-overview")]
    [Authorize(Policy = AuthorizationPolicies.RequireEvaluator)]
    public async Task<IActionResult> GetAgentsOverview([FromQuery] Guid? supervisorId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAgentsOverviewQuery(ResolveEffectiveSupervisorId(supervisorId)), cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>A Supervisor/TeamLead can only ever see their own team — the requested supervisorId
    /// (including "All Supervisors", i.e. null) is ignored and forced to their own id, same
    /// "don't trust the query string" reasoning as EvaluationsController's Agent self-scoping.
    /// Admin/QaEvaluator/dev keep the free "All Supervisors" picker.</summary>
    private Guid? ResolveEffectiveSupervisorId(Guid? requestedSupervisorId)
    {
        if (!User.IsInRole(Roles.Supervisor) && !User.IsInRole(Roles.TeamLead))
            return requestedSupervisorId;

        var userIdClaim = User.FindFirst(AuditIqRoleClaimsTransformation.UserIdClaimType)?.Value;
        return Guid.TryParse(userIdClaim, out var selfId) ? selfId : requestedSupervisorId;
    }

    /// <summary>The agent's own "Employee view" — see AgentDashboardDto for what's real vs placeholder.
    /// Same self-scoping rule as EvaluationsController.GetById: a plain Agent can only ever open
    /// their own dashboard (guessing another agent's id must not work); Admin/Supervisor/TeamLead/
    /// QaEvaluator can open anyone's, e.g. jumping here from the Agents tab.</summary>
    [HttpGet("agents/{agentId:guid}")]
    public async Task<IActionResult> GetAgentDashboard(Guid agentId, [FromQuery] int days = 30, CancellationToken cancellationToken = default)
    {
        if (User.IsInRole(Roles.Agent))
        {
            var userIdClaim = User.FindFirst(AuditIqRoleClaimsTransformation.UserIdClaimType)?.Value;
            if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var selfId) || agentId != selfId)
                return Forbid();
        }

        var result = await sender.Send(new GetAgentDashboardQuery(agentId, days), cancellationToken);
        return result.ToActionResult(this);
    }
}
