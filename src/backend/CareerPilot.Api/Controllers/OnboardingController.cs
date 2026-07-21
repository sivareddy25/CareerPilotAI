using CareerPilot.Application.Onboarding.Commands;
using CareerPilot.Application.Onboarding.Models;
using CareerPilot.Application.Onboarding.Queries;
using Microsoft.AspNetCore.Mvc;

namespace CareerPilot.Api.Controllers;

[Route("api/v{version:apiVersion}/onboarding")]
public sealed class OnboardingController : BaseApiController
{
    /// <summary>Checks if initial candidate onboarding setup is completed.</summary>
    [HttpGet("status")]
    [ProducesResponseType(typeof(OnboardingStatusDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<OnboardingStatusDto>> GetStatus(CancellationToken cancellationToken)
    {
        var status = await Queries.Query(new GetOnboardingStatusQuery(), cancellationToken);
        return Ok(status);
    }

    /// <summary>Saves first-time candidate setup details used by Playwright ATS auto-filling.</summary>
    [HttpPost("complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CompleteOnboarding([FromBody] CompleteOnboardingCommand command, CancellationToken cancellationToken)
    {
        var success = await Commands.Send(command, cancellationToken);
        return Ok(new { success, message = "Onboarding completed successfully." });
    }
}
