using CareerPilot.Application.Abstractions.Jobs;
using CareerPilot.Application.Abstractions.Persistence;
using CareerPilot.Application.Jobs.Commands;
using CareerPilot.Application.Jobs.Models;
using CareerPilot.Application.Jobs.Queries;
using CareerPilot.Domain.Jobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareerPilot.Api.Controllers;

[Authorize]
[Route("api/v{version:apiVersion}/jobs")]
public sealed class JobsController : BaseApiController
{
    /// <summary>Lists aggregated jobs with faceted filtering and pagination.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedJobsResultDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedJobsResultDto>> GetJobs(
        [FromQuery] string? search,
        [FromQuery] string? country,
        [FromQuery] string? city,
        [FromQuery] RemoteType? remoteType,
        [FromQuery] ExperienceLevel? experienceLevel,
        [FromQuery] EmploymentType? employmentType,
        [FromQuery] decimal? minSalary,
        [FromQuery] string? skill,
        [FromQuery] Guid? companyId,
        [FromQuery] bool sortByMatch = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var filter = new JobFilterParams(
            search,
            country,
            city,
            remoteType,
            experienceLevel,
            employmentType,
            minSalary,
            skill,
            companyId,
            pageNumber,
            pageSize);

        var result = await Queries.Query(new GetJobsQuery(filter, sortByMatch), cancellationToken);

        Response.Headers.CacheControl = "private, max-age=60";
        return Ok(result);
    }

    /// <summary>Returns full details for one aggregated job posting.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(JobDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<JobDto>> GetJobById(Guid id, CancellationToken cancellationToken)
    {
        var job = await Queries.Query(new GetJobByIdQuery(id), cancellationToken);
        if (job is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Job not found",
                Detail = $"Job with id '{id}' was not found.",
            });
        }

        Response.Headers.CacheControl = "private, max-age=120";
        return Ok(job);
    }

    /// <summary>AI explanation of why one job fits the caller's career profile.</summary>
    /// <remarks>
    /// Separate from <see cref="GetJobById"/> because it makes a local model call that can take
    /// seconds — kept off the fast job-detail read so opening a job stays instant and the
    /// explanation is fetched only when the user asks for it.
    /// </remarks>
    [HttpGet("{id:guid}/match-explanation")]
    [ProducesResponseType(typeof(JobMatchExplanationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<JobMatchExplanationDto>> ExplainMatch(Guid id, CancellationToken cancellationToken)
    {
        var explanation = await Queries.Query(new ExplainJobMatchQuery(id), cancellationToken);

        if (explanation is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Job not found",
                Detail = $"Job with id '{id}' was not found.",
            });
        }

        return Ok(explanation);
    }

    /// <summary>Triggers job synchronization across external ATS providers.</summary>
    [HttpPost("synchronize")]
    [ProducesResponseType(typeof(IReadOnlyList<JobSyncResult>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<JobSyncResult>>> Synchronize(
        [FromQuery] JobProviderKind? provider,
        CancellationToken cancellationToken)
    {
        var results = await Commands.Send(new SynchronizeJobsCommand(provider), cancellationToken);
        return Ok(results);
    }

    /// <summary>Imports raw job payloads into the normalized domain model.</summary>
    [HttpPost("import")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> Import(
        [FromBody] IReadOnlyList<RawJobPayload> payloads,
        CancellationToken cancellationToken)
    {
        if (payloads is null || payloads.Count == 0)
        {
            return BadRequest("Payload collection cannot be empty.");
        }

        var importedCount = await Commands.Send(new ImportJobsCommand(payloads), cancellationToken);
        return Ok(importedCount);
    }
}
