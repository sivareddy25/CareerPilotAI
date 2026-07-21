using System.Security.Claims;
using CareerPilot.Application.Dashboard.Commands;
using CareerPilot.Application.Dashboard.Models;
using CareerPilot.Application.Dashboard.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareerPilot.Api.Controllers;

[Authorize]
[Route("api/v{version:apiVersion}/notifications")]
public sealed class NotificationsController : BaseApiController
{
    /// <summary>Lists active in-app notifications for the caller.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<NotificationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<NotificationDto>>> GetNotifications(CancellationToken cancellationToken)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userId = Guid.TryParse(userIdStr, out var id) ? id : Guid.NewGuid();

        var notifications = await Queries.Query(new GetNotificationsQuery(userId), cancellationToken);
        Response.Headers.CacheControl = "private, max-age=15";
        return Ok(notifications);
    }

    /// <summary>Marks one in-app notification as read.</summary>
    [HttpPut("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken cancellationToken)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userId = Guid.TryParse(userIdStr, out var userIdGuid) ? userIdGuid : Guid.NewGuid();

        await Commands.Send(new MarkNotificationReadCommand(userId, id), cancellationToken);
        return NoContent();
    }
}
