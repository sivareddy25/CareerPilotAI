using CareerPilot.Application.Automation.Commands;
using CareerPilot.Application.Automation.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareerPilot.Api.Controllers;

[Authorize]
[Route("api/v{version:apiVersion}/automation")]
public sealed class AutomationController : BaseApiController
{
    [HttpGet("answers")]
    [ProducesResponseType(typeof(IReadOnlyList<CandidateAnswerDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAnswers(CancellationToken cancellationToken)
    {
        var result = await Queries.Query(new GetCandidateAnswersQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost("answers")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<IActionResult> SaveAnswer([FromBody] SaveCandidateAnswerCommand command, CancellationToken cancellationToken)
    {
        var id = await Commands.Send(command, cancellationToken);
        return Ok(new { id });
    }

    [HttpPost("apply")]
    [HttpPost("/api/v1/automation/apply")]
    [ProducesResponseType(typeof(AutoApplyResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExecuteAutoApply([FromBody] ExecutePlaywrightAutoApplyCommand command, CancellationToken cancellationToken)
    {
        var result = await Commands.Send(command, cancellationToken);
        return Ok(result);
    }
}
