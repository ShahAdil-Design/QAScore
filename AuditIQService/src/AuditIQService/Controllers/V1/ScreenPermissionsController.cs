using Asp.Versioning;
using AuditIQ.Api.Auth;
using AuditIQ.Api.Contracts.Requests;
using AuditIQ.Api.Extensions;
using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Permissions.Commands.UpdateScreenPermissions;
using AuditIQ.Application.Permissions.Queries.GetScreenPermissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuditIQ.Api.Controllers.V1;

/// <summary>Screen-visibility only — which roles see which frontend nav items/routes. This is
/// NOT a substitute for the API-level [Authorize(Policy = ...)] checks elsewhere, which remain
/// the real security boundary; a role could in principle be granted visibility of a screen here
/// while its underlying API calls still 403 under the existing fixed policies.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/screen-permissions")]
[Authorize]
public class ScreenPermissionsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetScreenPermissionsQuery(), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPut]
    [Authorize(Policy = AuthorizationPolicies.RequireAdmin)]
    public async Task<IActionResult> Update([FromBody] UpdateScreenPermissionsRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateScreenPermissionsCommand(
            request.Permissions.Select(p => new ScreenPermissionInput(p.Role, p.ScreenKey, p.IsVisible)).ToList());

        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult(this);
    }
}
