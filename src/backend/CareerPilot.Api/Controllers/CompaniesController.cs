using CareerPilot.Application.Jobs.Models;
using CareerPilot.Application.Jobs.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareerPilot.Api.Controllers;

[Authorize]
[Route("api/v{version:apiVersion}/companies")]
public sealed class CompaniesController : BaseApiController
{
    /// <summary>Lists all hiring companies in the database.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CompanyDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CompanyDto>>> GetCompanies(CancellationToken cancellationToken)
    {
        var companies = await Queries.Query(new GetCompaniesQuery(), cancellationToken);
        Response.Headers.CacheControl = "private, max-age=300";
        return Ok(companies);
    }
}
