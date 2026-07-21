using System.Security.Claims;
using CareerPilot.Application.Communication.Commands;
using CareerPilot.Application.Communication.Models;
using CareerPilot.Application.Communication.Queries;
using CareerPilot.Domain.Communication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareerPilot.Api.Controllers;

[Authorize]
[Route("api/v{version:apiVersion}/communication")]
public sealed class CommunicationController : BaseApiController
{
    /// <summary>Lists connected email and calendar OAuth accounts for the user.</summary>
    [HttpGet("accounts")]
    [ProducesResponseType(typeof(IReadOnlyList<ConnectedAccountDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ConnectedAccountDto>>> GetAccounts(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var accounts = await Queries.Query(new GetConnectedAccountsQuery(userId), cancellationToken);
        return Ok(accounts);
    }

    /// <summary>Connects a new Microsoft 365 or Google account via OAuth authorization code.</summary>
    [HttpPost("accounts/connect")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ConnectAccount([FromBody] ConnectAccountRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var success = await Commands.Send(new ConnectProviderCommand(userId, request.ProviderKind, request.AuthCode), cancellationToken);
        return Ok(new { success });
    }

    /// <summary>Disconnects a connected email/calendar account.</summary>
    [HttpDelete("accounts/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DisconnectAccount(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await Commands.Send(new DisconnectProviderCommand(userId, id), cancellationToken);
        return NoContent();
    }

    /// <summary>Fetches categorized recruiter email threads in the inbox.</summary>
    [HttpGet("inbox")]
    [ProducesResponseType(typeof(IReadOnlyList<RecruiterThreadDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RecruiterThreadDto>>> GetInbox([FromQuery] EmailCategory? category, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var threads = await Queries.Query(new GetRecruiterInboxQuery(userId, category), cancellationToken);
        return Ok(threads);
    }

    /// <summary>Generates an AI reply draft for a recruiter email thread.</summary>
    [HttpPost("replies/generate")]
    [ProducesResponseType(typeof(ReplyDraftDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ReplyDraftDto>> GenerateReplyDraft([FromBody] GenerateDraftRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var draft = await Commands.Send(new GenerateReplyDraftCommand(userId, request.ThreadId, request.Tone, request.CustomInstruction), cancellationToken);
        return Ok(draft);
    }

    /// <summary>Sends an explicitly user-approved and edited reply email to the recruiter.</summary>
    [HttpPost("replies/send")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SendApprovedReply([FromBody] SendApprovedReplyRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var success = await Commands.Send(new SendApprovedReplyCommand(userId, request.ThreadId, request.ToAddress, request.Subject, request.ApprovedBody), cancellationToken);
        return Ok(new { success, message = "Reply sent successfully via connected email account." });
    }

    /// <summary>Retrieves upcoming interview calendar events with prep checklists.</summary>
    [HttpGet("calendar/upcoming")]
    [ProducesResponseType(typeof(IReadOnlyList<InterviewEventDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<InterviewEventDto>>> GetUpcomingInterviews(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var events = await Queries.Query(new GetUpcomingInterviewEventsQuery(userId), cancellationToken);
        return Ok(events);
    }

    private Guid GetUserId()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdStr, out var id) ? id : Guid.NewGuid();
    }
}

public sealed record ConnectAccountRequest(CommunicationProviderKind ProviderKind, string AuthCode);
public sealed record GenerateDraftRequest(Guid ThreadId, ReplyTone Tone, string? CustomInstruction);
public sealed record SendApprovedReplyRequest(Guid ThreadId, string ToAddress, string Subject, string ApprovedBody);
