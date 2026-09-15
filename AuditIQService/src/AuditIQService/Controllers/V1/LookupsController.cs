using Asp.Versioning;
using AuditIQ.Api.Auth;
using AuditIQ.Api.Contracts.Requests;
using AuditIQ.Api.Extensions;
using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Lookups.Commands.CreateCauseCode;
using AuditIQ.Application.Lookups.Commands.CreateComment;
using AuditIQ.Application.Lookups.Commands.DeleteCauseCode;
using AuditIQ.Application.Lookups.Commands.DeleteComment;
using AuditIQ.Application.Lookups.Commands.UpdateCauseCode;
using AuditIQ.Application.Lookups.Commands.UpdateComment;
using AuditIQ.Application.Lookups.Queries.GetCauseCodes;
using AuditIQ.Application.Lookups.Queries.GetCommentLibraryItems;
using AuditIQ.Application.Lookups.Queries.GetEventTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuditIQ.Api.Controllers.V1;

/// <summary>Managed lookups feeding the scoring form's cause-code and comment dropdowns
/// (Section 5, table 10). Reads are open to any authenticated user (needed to populate the
/// dropdowns); writes are admin-gated, matching ScorecardCategory's CRUD pattern.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/lookups")]
[Authorize]
public class LookupsController(ISender sender) : ControllerBase
{
    [HttpGet("cause-codes")]
    public async Task<IActionResult> GetCauseCodes(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCauseCodesQuery(), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost("cause-codes")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdmin)]
    public async Task<IActionResult> CreateCauseCode([FromBody] CreateLookupItemRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateCauseCodeCommand(request.Text), cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetCauseCodes), new { version = "1.0" }, new { id = result.Value })
            : result.ToActionResult(this);
    }

    [HttpPut("cause-codes/{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdmin)]
    public async Task<IActionResult> UpdateCauseCode(Guid id, [FromBody] UpdateLookupItemRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateCauseCodeCommand(id, request.Text), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpDelete("cause-codes/{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdmin)]
    public async Task<IActionResult> DeleteCauseCode(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteCauseCodeCommand(id), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpGet("comments")]
    public async Task<IActionResult> GetComments(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCommentLibraryItemsQuery(), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost("comments")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdmin)]
    public async Task<IActionResult> CreateComment([FromBody] CreateLookupItemRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateCommentCommand(request.Text), cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetComments), new { version = "1.0" }, new { id = result.Value })
            : result.ToActionResult(this);
    }

    [HttpPut("comments/{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdmin)]
    public async Task<IActionResult> UpdateComment(Guid id, [FromBody] UpdateLookupItemRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateCommentCommand(id, request.Text), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpDelete("comments/{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdmin)]
    public async Task<IActionResult> DeleteComment(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteCommentCommand(id), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpGet("event-types")]
    public async Task<IActionResult> GetEventTypes([FromQuery] Guid scorecardId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetEventTypesQuery(scorecardId), cancellationToken);
        return result.ToActionResult(this);
    }
}
