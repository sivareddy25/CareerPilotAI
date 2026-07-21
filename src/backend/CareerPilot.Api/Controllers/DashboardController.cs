using System.Security.Claims;
using CareerPilot.Application.Dashboard.Models;
using CareerPilot.Application.Dashboard.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareerPilot.Api.Controllers;

[Authorize]
[Route("api/v{version:apiVersion}/dashboard")]
public sealed class DashboardController : BaseApiController
{
    /// <summary>Retrieves executive metrics, activity stream, and upcoming interview widgets.</summary>
    [HttpGet("overview")]
    [ProducesResponseType(typeof(DashboardOverviewDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardOverviewDto>> GetOverview(CancellationToken cancellationToken)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userId = Guid.TryParse(userIdStr, out var id) ? id : Guid.NewGuid();

        var overview = await Queries.Query(new GetDashboardOverviewQuery(userId), cancellationToken);

        Response.Headers.CacheControl = "private, max-age=30";
        return Ok(overview);
    }
}
