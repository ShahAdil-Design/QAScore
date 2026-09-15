using Asp.Versioning;
using AuditIQ.Api.Auth;
using AuditIQ.Api.Contracts.Requests;
using AuditIQ.Api.Extensions;
using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Scorecards.Commands.CreateScorecardCategory;
using AuditIQ.Application.Scorecards.Commands.DeleteScorecardCategory;
using AuditIQ.Application.Scorecards.Commands.UpdateScorecardCategory;
using AuditIQ.Application.Scorecards.Queries.GetScorecardCategories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuditIQ.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/scorecard-categories")]
[Authorize]
public class ScorecardCategoriesController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetScorecardCategoriesQuery(), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.RequireAdmin)]
    public async Task<IActionResult> Create([FromBody] CreateScorecardCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateScorecardCategoryCommand(request.Name), cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetAll), new { version = "1.0" }, new { id = result.Value })
            : result.ToActionResult(this);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdmin)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateScorecardCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateScorecardCategoryCommand(id, request.Name), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdmin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteScorecardCategoryCommand(id), cancellationToken);
        return result.ToActionResult(this);
    }
}
