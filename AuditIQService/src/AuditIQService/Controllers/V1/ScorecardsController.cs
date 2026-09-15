using Asp.Versioning;
using AuditIQ.Api.Auth;
using AuditIQ.Api.Contracts.Requests;
using AuditIQ.Api.Extensions;
using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Scorecards.Commands;
using AuditIQ.Application.Scorecards.Commands.ArchiveScorecard;
using AuditIQ.Application.Scorecards.Commands.CreateScorecard;
using AuditIQ.Application.Scorecards.Commands.LockScorecard;
using AuditIQ.Application.Scorecards.Commands.UnlockScorecard;
using AuditIQ.Application.Scorecards.Commands.UpdateScorecard;
using AuditIQ.Application.Scorecards.Queries.GetScorecardById;
using AuditIQ.Application.Scorecards.Queries.GetScorecards;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuditIQ.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/scorecards")]
[Authorize]
public class ScorecardsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? categoryId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetScorecardsQuery(categoryId), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetScorecardByIdQuery(id), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.RequireAdmin)]
    public async Task<IActionResult> Create([FromBody] CreateScorecardRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateScorecardCommand(
            request.Name, request.Description, request.ScorecardType, request.CategoryId, request.Location,
            request.TargetPercentage, request.MaxScore, request.GroupIds, request.Questions.Select(ToInput).ToList());

        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value, version = "1.0" }, new { id = result.Value })
            : result.ToActionResult(this);
    }

    /// <summary>Never mutates the existing row — produces a new version (see UpdateScorecardCommand).</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdmin)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateScorecardRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateScorecardCommand(
            id, request.Name, request.Description, request.ScorecardType, request.CategoryId, request.Location,
            request.TargetPercentage, request.MaxScore, request.GroupIds, request.Questions.Select(ToInput).ToList());

        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(new { id = result.Value }) : result.ToActionResult(this);
    }

    [HttpPost("{id:guid}/lock")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdmin)]
    public async Task<IActionResult> Lock(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new LockScorecardCommand(id), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost("{id:guid}/unlock")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdmin)]
    public async Task<IActionResult> Unlock(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UnlockScorecardCommand(id), cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>Soft delete — archives the current version. Scorecards are never hard-deleted
    /// (Temporal Table + existing Evaluations hold a hard FK to a specific version).</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdmin)]
    public async Task<IActionResult> Archive(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ArchiveScorecardCommand(id), cancellationToken);
        return result.ToActionResult(this);
    }

    private static ScorecardQuestionInput ToInput(ScorecardQuestionRequest q) =>
        new(q.SectionName, q.Text, q.Weight, q.IsFailLogic, q.SortOrder, q.AnswerOptions.Select(ToInput).ToList());

    private static AnswerOptionInput ToInput(AnswerOptionRequest a) =>
        new(a.Label, a.Value, a.IsFailSection, a.IsFailAll, a.IsNotApplicable);
}
