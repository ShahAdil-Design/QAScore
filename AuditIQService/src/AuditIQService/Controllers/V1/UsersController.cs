using Asp.Versioning;
using AuditIQ.Api.Auth;
using AuditIQ.Api.Contracts.Requests;
using AuditIQ.Api.Extensions;
using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Results;
using AuditIQ.Application.Users;
using AuditIQ.Application.Users.Commands.CreateUser;
using AuditIQ.Application.Users.Commands.DeactivateUser;
using AuditIQ.Application.Users.Commands.ReactivateUser;
using AuditIQ.Application.Users.Commands.SetUserGroups;
using AuditIQ.Application.Users.Commands.SetUserTeams;
using AuditIQ.Application.Users.Commands.UpdateUser;
using AuditIQ.Application.Users.Queries.GetDirectoryCandidates;
using AuditIQ.Application.Users.Queries.GetDirectReports;
using AuditIQ.Application.Users.Queries.GetUserById;
using AuditIQ.Application.Users.Queries.GetUsers;
using AuditIQ.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuditIQ.Api.Controllers.V1;

/// <summary>Roles & responsibilities: user profile, role assignment, and team/group membership.
/// Full Staff CRUD (bulk import, richer directory search) is still Phase 2 — this covers the
/// core roles/responsibilities workflow: create a user, change their role, manage what
/// teams/groups they belong to, and activate/deactivate them.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/users")]
[Authorize]
public class UsersController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] UserRole? role, [FromQuery] bool includeInactive = false, [FromQuery] Guid? groupId = null, CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetUsersQuery(role, includeInactive, groupId), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetUserByIdQuery(id), cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>Resolves "who am I" from AuditIqRoleClaimsTransformation's injected user-id
    /// claim — the real identity/role source, not whatever the SSO token itself claims. Returns
    /// User.NotProvisioned (403) if the caller authenticated but has no matching AuditIQ user.</summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(AuditIqRoleClaimsTransformation.UserIdClaimType)?.Value;
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
            return Result.Failure(UserErrors.NotProvisioned).ToActionResult(this);

        var result = await sender.Send(new GetUserByIdQuery(userId), cancellationToken);
        return result.ToActionResult(this);
    }

    // Anyone can view their own direct reports; only an Admin can view someone else's.
    [HttpGet("{id:guid}/direct-reports")]
    public async Task<IActionResult> GetDirectReports(Guid id, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(AuditIqRoleClaimsTransformation.UserIdClaimType)?.Value;
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var requestingUserId))
            return Result.Failure(UserErrors.NotProvisioned).ToActionResult(this);

        if (id != requestingUserId && !User.IsInRole(nameof(UserRole.Admin)))
            return Result.Failure(UserErrors.NotAuthorizedForUser).ToActionResult(this);

        var result = await sender.Send(new GetDirectReportsQuery(id), cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>Members of the configured Entra ID group — candidates for the Create User form's
    /// email picker, fetched live from Microsoft Graph (Application permission, not delegated).</summary>
    [HttpGet("directory-candidates")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdmin)]
    public async Task<IActionResult> GetDirectoryCandidates(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetDirectoryCandidatesQuery(), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.RequireAdmin)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateUserCommand(
            request.DisplayName, request.Email, request.Role, request.EmploymentType, request.TeamIds, request.GroupIds);

        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value, version = "1.0" }, new { id = result.Value })
            : result.ToActionResult(this);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdmin)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateUserCommand(id, request.DisplayName, request.Email, request.Role, request.EmploymentType, request.Notes);
        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPut("{id:guid}/teams")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdmin)]
    public async Task<IActionResult> SetTeams(Guid id, [FromBody] SetUserTeamsRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SetUserTeamsCommand(id, request.TeamIds), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPut("{id:guid}/groups")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdmin)]
    public async Task<IActionResult> SetGroups(Guid id, [FromBody] SetUserGroupsRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SetUserGroupsCommand(id, request.GroupIds), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdmin)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeactivateUserCommand(id), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost("{id:guid}/reactivate")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdmin)]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ReactivateUserCommand(id), cancellationToken);
        return result.ToActionResult(this);
    }
}
