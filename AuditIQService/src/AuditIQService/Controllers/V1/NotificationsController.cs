using Asp.Versioning;
using AuditIQ.Api.Extensions;
using AuditIQ.Application.Abstractions.Auth;
using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Notifications.Commands.MarkAllNotificationsRead;
using AuditIQ.Application.Notifications.Commands.MarkNotificationRead;
using AuditIQ.Application.Notifications.Queries.GetNotifications;
using AuditIQ.Application.Notifications.Queries.GetUnreadNotificationCount;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuditIQ.Api.Controllers.V1;

/// <summary>The header bell's feed — always scoped to the caller's own identity (ICurrentUserService),
/// same self-scoping rule as EvaluationsController's GetQueue/GetById: nobody can browse or mark
/// read another user's notifications, so RecipientUserId always comes from the token, never the
/// request.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/notifications")]
[Authorize]
public class NotificationsController(ISender sender, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is not { } selfId)
            return Forbid();

        var result = await sender.Send(new GetNotificationsQuery(selfId, unreadOnly, page, pageSize), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } selfId)
            return Forbid();

        var result = await sender.Send(new GetUnreadNotificationCountQuery(selfId), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } selfId)
            return Forbid();

        var result = await sender.Send(new MarkNotificationReadCommand(id, selfId), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } selfId)
            return Forbid();

        var result = await sender.Send(new MarkAllNotificationsReadCommand(selfId), cancellationToken);
        return result.ToActionResult(this);
    }
}
