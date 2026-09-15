using Asp.Versioning;
using AuditIQ.Api.Auth;
using AuditIQ.Api.Contracts.Requests;
using AuditIQ.Api.Extensions;
using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Results;
using AuditIQ.Application.Reviews.Commands.GiveKudos;
using AuditIQ.Application.Reviews.Queries.GetAgentPerformanceSummary;
using AuditIQ.Application.Reviews.Queries.GetKudosForUser;
using AuditIQ.Application.Reviews.Queries.GetTeamResults;
using AuditIQ.Application.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuditIQ.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/reviews")]
[Authorize]
public class ReviewsController(ISender sender) : ControllerBase
{
    /// <summary>Backs the agent self-service dashboard — same self-scoping rule as
    /// DashboardController.GetAgentDashboard: a plain Agent can only ever open their own
    /// performance summary; Admin/Supervisor/TeamLead/QaEvaluator can open anyone's.</summary>
    [HttpGet("agents/{agentId:guid}/performance")]
    public async Task<IActionResult> GetAgentPerformance(Guid agentId, CancellationToken cancellationToken)
    {
        if (User.IsInRole(Roles.Agent))
        {
            var userIdClaim = User.FindFirst(AuditIqRoleClaimsTransformation.UserIdClaimType)?.Value;
            if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var selfId) || agentId != selfId)
                return Forbid();
        }

        var result = await sender.Send(new GetAgentPerformanceSummaryQuery(agentId), cancellationToken);
        return result.ToActionResult(this);
    }

    // The role policy below only gates "some kind of supervisor", not "supervisor of *this*
    // team" — the handler enforces that separately using the caller's real id, resolved from
    // AuditIqRoleClaimsTransformation's injected claim (same source GetMe uses), not a
    // client-supplied value that could be spoofed to view another supervisor's team.
    [HttpGet("teams/{teamId:guid}/results")]
    [Authorize(Policy = AuthorizationPolicies.RequireSupervisorOrAbove)]
    public async Task<IActionResult> GetTeamResults(Guid teamId, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(AuditIqRoleClaimsTransformation.UserIdClaimType)?.Value;
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var requestingUserId))
            return Result.Failure(UserErrors.NotProvisioned).ToActionResult(this);

        var result = await sender.Send(new GetTeamResultsQuery(teamId, requestingUserId), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpGet("kudos/{userId:guid}")]
    public async Task<IActionResult> GetKudos(Guid userId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetKudosForUserQuery(userId), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost("kudos")]
    public async Task<IActionResult> GiveKudos([FromBody] GiveKudosRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GiveKudosCommand(request.FromUserId, request.ToUserId, request.Message), cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetKudos), new { userId = request.ToUserId, version = "1.0" }, new { id = result.Value })
            : result.ToActionResult(this);
    }
}
