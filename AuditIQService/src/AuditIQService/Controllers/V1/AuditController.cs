using Asp.Versioning;
using AuditIQ.Api.Auth;
using AuditIQ.Api.Extensions;
using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Audit.Queries.GetAuditLog;
using AuditIQ.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuditIQ.Api.Controllers.V1;

/// <summary>"Who did X" — Admin-only, since it exposes every user's activity across the system.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/audit")]
[Authorize(Policy = AuthorizationPolicies.RequireAdmin)]
public class AuditController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetLog(
        [FromQuery] string? entityName,
        [FromQuery] Guid? entityId,
        [FromQuery] Guid? userId,
        [FromQuery] AuditAction? action,
        [FromQuery] DateTimeOffset? dateFrom,
        [FromQuery] DateTimeOffset? dateTo,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAuditLogQuery(entityName, entityId, userId, action, dateFrom, dateTo, page, pageSize);
        var result = await sender.Send(query, cancellationToken);
        return result.ToActionResult(this);
    }
}
