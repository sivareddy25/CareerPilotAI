using CareerPilot.Application.Abstractions.Storage;
using CareerPilot.Application.Authentication.Commands.ChangePassword;
using CareerPilot.Application.Profiles.Commands.DeactivateAccount;
using CareerPilot.Application.Profiles.Commands.DeleteAccount;
using CareerPilot.Application.Profiles.Commands.DeleteProfileImage;
using CareerPilot.Application.Profiles.Commands.UpdatePreferences;
using CareerPilot.Application.Profiles.Commands.UpdateProfile;
using CareerPilot.Application.Profiles.Commands.UploadProfileImage;
using CareerPilot.Application.Profiles.Models;
using CareerPilot.Application.Profiles.Queries.GetProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareerPilot.Api.Controllers;

/// <summary>
/// The signed-in user's own profile and account.
/// </summary>
/// <remarks>
/// <para>
/// <c>[Authorize]</c> sits on the controller, so every action inherits it and a new one
/// cannot be added unprotected by omission. It is redundant with the application's
/// fallback policy and kept anyway: defence that depends on a global setting staying
/// configured is defence that disappears when someone changes the global setting.
/// </para>
/// <para>
/// No route here takes a user id. Every action resolves the caller from their token,
/// which is what makes horizontal privilege escalation structurally impossible rather
/// than merely checked for — there is no parameter to tamper with.
/// </para>
/// </remarks>
[Authorize]
[Route("api/v{version:apiVersion}/profile")]
public sealed class ProfileController : BaseApiController
{
    /// <summary>Returns the caller's profile, including preferences.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProfileDto>> Get(CancellationToken cancellationToken)
    {
        var profile = await Queries.Query(new GetProfileQuery(), cancellationToken);

        // A profile is personal data. Even though it only ever reaches its owner, an
        // intermediary cache holding it would serve one user's details to the next.
        Response.Headers.CacheControl = "no-store, private";

        return Ok(profile);
    }

    /// <summary>Updates the caller's profile and returns the stored result.</summary>
    [HttpPut]
    [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ProfileDto>> Update(
        [FromBody] UpdateProfileCommand command,
        CancellationToken cancellationToken)
    {
        var profile = await Commands.Send(command, cancellationToken);

        return Ok(profile);
    }

    /// <summary>Updates display and notification preferences.</summary>
    /// <remarks>
    /// Not in the original endpoint list, but preferences have to be persisted by
    /// something. Kept off <c>PUT /profile</c> so changing a theme does not require
    /// round-tripping the whole profile — see <see cref="UpdatePreferencesCommand"/>.
    /// </remarks>
    [HttpPut("preferences")]
    [ProducesResponseType(typeof(PreferencesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PreferencesDto>> UpdatePreferences(
        [FromBody] UpdatePreferencesCommand command,
        CancellationToken cancellationToken)
    {
        var preferences = await Commands.Send(command, cancellationToken);

        return Ok(preferences);
    }

    /// <summary>Replaces the caller's profile picture.</summary>
    /// <remarks>
    /// <para>
    /// <c>RequestSizeLimit</c> is the outer guard. The handler also checks the length,
    /// but that check runs after the body has been buffered — this one makes Kestrel
    /// reject an oversized upload before it is read, so a large file cannot consume
    /// memory or disk on its way to being refused.
    /// </para>
    /// <para>
    /// The <c>IFormFile</c> is unwrapped into a transport-neutral
    /// <see cref="FileUploadRequest"/> here, so the ASP.NET type never crosses into the
    /// Application layer.
    /// </para>
    /// </remarks>
    [HttpPost("picture")]
    [RequestSizeLimit(4 * 1024 * 1024)]
    [ProducesResponseType(typeof(ProfileImageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProfileImageDto>> UploadPicture(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "No file was uploaded.",
                Detail = "Attach an image using the 'file' form field.",
            });
        }

        await using var stream = file.OpenReadStream();

        var result = await Commands.Send(
            new UploadProfileImageCommand(
                new FileUploadRequest(stream, file.FileName, file.ContentType, file.Length)),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>Removes the caller's profile picture. Idempotent.</summary>
    [HttpDelete("picture")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeletePicture(CancellationToken cancellationToken)
    {
        await Commands.Send(new DeleteProfileImageCommand(), cancellationToken);

        return NoContent();
    }

    /// <summary>Changes the caller's password and signs out every session.</summary>
    /// <remarks>
    /// Dispatches the existing <see cref="ChangePasswordCommand"/> from the
    /// authentication module rather than introducing a profile-specific duplicate.
    /// Password change is one behaviour with one set of rules; two implementations
    /// would be two places for a security fix to land, and one to be missed.
    /// </remarks>
    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordCommand command,
        CancellationToken cancellationToken)
    {
        await Commands.Send(command, cancellationToken);

        return NoContent();
    }

    /// <summary>Deactivates the account. Requires the current password.</summary>
    [HttpPost("deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Deactivate(
        [FromBody] DeactivateAccountCommand command,
        CancellationToken cancellationToken)
    {
        await Commands.Send(command, cancellationToken);

        return NoContent();
    }

    /// <summary>Soft-deletes the account. Requires the password and a typed confirmation.</summary>
    /// <remarks>
    /// A body on DELETE is unusual and deliberate: the password must not travel in a
    /// query string, where it would be captured by server logs, browser history and
    /// referrer headers.
    /// </remarks>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(
        [FromBody] DeleteAccountCommand command,
        CancellationToken cancellationToken)
    {
        await Commands.Send(command, cancellationToken);

        return NoContent();
    }
}
