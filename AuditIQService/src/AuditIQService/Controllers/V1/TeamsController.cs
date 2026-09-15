using Asp.Versioning;
using AuditIQ.Api.Extensions;
using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Users.Queries.GetTeams;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuditIQ.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/teams")]
[Authorize]
public class TeamsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? groupId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTeamsQuery(groupId), cancellationToken);
        return result.ToActionResult(this);
    }
}
