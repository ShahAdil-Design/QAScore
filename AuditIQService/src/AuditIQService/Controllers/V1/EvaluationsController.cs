using Asp.Versioning;
using AuditIQ.Api.Auth;
using AuditIQ.Api.Contracts.Requests;
using AuditIQ.Api.Extensions;
using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Evaluations.Commands.AcknowledgeEvaluation;
using AuditIQ.Application.Evaluations.Commands.CreateEvaluation;
using AuditIQ.Application.Evaluations.Commands.DisputeEvaluation;
using AuditIQ.Application.Evaluations.Commands.ResolveDispute;
using AuditIQ.Application.Evaluations.Commands.SaveDraftAnswers;
using AuditIQ.Application.Evaluations.Commands.SubmitEvaluation;
using AuditIQ.Application.Evaluations.Queries.GetEvaluationById;
using AuditIQ.Application.Evaluations.Queries.GetEvaluationQueue;
using AuditIQ.Application.Users.Queries.GetDirectReports;
using AuditIQ.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuditIQ.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/evaluations")]
[Authorize]
public class EvaluationsController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Admin/QaEvaluator can browse anyone's queue with any filter. A plain Agent can only ever
    /// see their own — Epic 4's self-service Review page needs this, but nothing stops it from
    /// being pointed at someone else's agentId, so that's enforced here rather than trusted from
    /// the query string. No agentId supplied defaults to "my own" for an Agent, since that's the
    /// only legitimate case; an explicit agentId that isn't their own is rejected. A Supervisor/
    /// TeamLead is restricted to their own team (self + direct reports, User.SupervisorId) —
    /// same "don't trust the query string" reasoning, just a set instead of a single id.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetQueue(
        [FromQuery] Guid? agentId,
        [FromQuery] Guid? evaluatorId,
        [FromQuery] Guid? teamId,
        [FromQuery] EvaluationStatus? status,
        [FromQuery] Guid? scorecardId,
        [FromQuery] DateTimeOffset? dateFrom,
        [FromQuery] DateTimeOffset? dateTo,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        if (User.IsInRole(Roles.Agent))
        {
            var userIdClaim = User.FindFirst(AuditIqRoleClaimsTransformation.UserIdClaimType)?.Value;
            if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var selfId))
                return Forbid();

            if (agentId is null)
                agentId = selfId;
            else if (agentId != selfId)
                return Forbid();
        }

        IReadOnlyList<Guid>? restrictToAgentIds = null;
        if (User.IsInRole(Roles.Supervisor) || User.IsInRole(Roles.TeamLead))
        {
            var userIdClaim = User.FindFirst(AuditIqRoleClaimsTransformation.UserIdClaimType)?.Value;
            if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var selfId))
                return Forbid();

            var directReportsResult = await sender.Send(new GetDirectReportsQuery(selfId), cancellationToken);
            if (!directReportsResult.IsSuccess)
                return directReportsResult.ToActionResult(this);

            restrictToAgentIds = [selfId, .. directReportsResult.Value.Select(u => u.Id)];
            if (agentId is not null && !restrictToAgentIds.Contains(agentId.Value))
                return Forbid();
        }

        var query = new GetEvaluationQueueQuery(agentId, evaluatorId, teamId, status, scorecardId, dateFrom, dateTo, page, pageSize, restrictToAgentIds);
        var result = await sender.Send(query, cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>
    /// Same self-scoping rule as GetQueue: a plain Agent can only ever open their own evaluation
    /// (guessing another agent's evaluation id must not work), and even their own comes back with
    /// EvaluatorNotes stripped — that field is the evaluator's internal working notes, not
    /// agent-facing content.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetEvaluationByIdQuery(id), cancellationToken);
        if (!result.IsSuccess)
            return result.ToActionResult(this);

        var evaluation = result.Value;

        if (User.IsInRole(Roles.Agent))
        {
            var userIdClaim = User.FindFirst(AuditIqRoleClaimsTransformation.UserIdClaimType)?.Value;
            if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var selfId) || evaluation.AgentId != selfId)
                return Forbid();

            evaluation = evaluation with { EvaluatorNotes = null };
        }

        return Ok(evaluation);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.RequireEvaluator)]
    public async Task<IActionResult> Create([FromBody] CreateEvaluationRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateEvaluationCommand(
            request.ScorecardId, request.AgentId, request.EvaluatorId,
            request.EventTypeId, request.EventSubTypeId, request.Reference,
            request.EventOccurredAt, request.EventDurationSeconds);

        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value, version = "1.0" }, new { id = result.Value })
            : result.ToActionResult(this);
    }

    [HttpPut("{id:guid}/answers")]
    [Authorize(Policy = AuthorizationPolicies.RequireEvaluator)]
    public async Task<IActionResult> SaveDraftAnswers(Guid id, [FromBody] SaveDraftAnswersRequest request, CancellationToken cancellationToken)
    {
        var answers = request.Answers
            .Select(a => new AnswerInput(a.QuestionId, a.AnswerValue, a.CauseCode, a.Comment))
            .ToList();

        var result = await sender.Send(new SaveDraftAnswersCommand(id, answers), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost("{id:guid}/submit")]
    [Authorize(Policy = AuthorizationPolicies.RequireEvaluator)]
    public async Task<IActionResult> Submit(Guid id, [FromBody] SubmitEvaluationRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SubmitEvaluationCommand(id, request.EvaluatorNotes), cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>
    /// Only the agent being evaluated can acknowledge their own evaluation — same self-scoping
    /// rule as GetQueue/GetById: the caller's identity claim is the source of truth for "who is
    /// acting", never a client-supplied agentId, since that would let anyone acknowledge on
    /// another agent's behalf just by knowing their id.
    /// </summary>
    [HttpPost("{id:guid}/acknowledge")]
    [Authorize(Roles = Roles.Agent)]
    public async Task<IActionResult> Acknowledge(Guid id, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(AuditIqRoleClaimsTransformation.UserIdClaimType)?.Value;
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var selfId))
            return Forbid();

        var result = await sender.Send(new AcknowledgeEvaluationCommand(id, selfId), cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>Same self-scoping rule as Acknowledge above.</summary>
    [HttpPost("{id:guid}/dispute")]
    [Authorize(Roles = Roles.Agent)]
    public async Task<IActionResult> Dispute(Guid id, [FromBody] DisputeEvaluationRequest request, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(AuditIqRoleClaimsTransformation.UserIdClaimType)?.Value;
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var selfId))
            return Forbid();

        var result = await sender.Send(new DisputeEvaluationCommand(id, selfId, request.DisputeReason), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost("{id:guid}/resolve-dispute")]
    [Authorize(Policy = AuthorizationPolicies.RequireSupervisorOrAbove)]
    public async Task<IActionResult> ResolveDispute(Guid id, [FromBody] ResolveDisputeRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ResolveDisputeCommand(id, request.ResolutionNotes), cancellationToken);
        return result.ToActionResult(this);
    }
}
