using CareerPilot.Api.Configuration;
using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Authentication.Commands.Login;
using CareerPilot.Application.Authentication.Commands.Logout;
using CareerPilot.Application.Authentication.Commands.RefreshToken;
using CareerPilot.Application.Authentication.Commands.Register;
using CareerPilot.Application.Authentication.Models;
using CareerPilot.Application.Authentication.Queries.GetCurrentUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace CareerPilot.Api.Controllers;

/// <summary>
/// Local authentication: registration, sign-in, token refresh, sign-out, and the
/// caller's own profile.
/// </summary>
/// <remarks>
/// The controller does no authentication work itself. It binds, dispatches, and shapes
/// the response — every decision lives in a handler, so the rules cannot differ
/// between this transport and any other added later.
/// </remarks>
[Route("api/v{version:apiVersion}/auth")]
public sealed class AuthController(IOptions<RefreshTokenCookieOptions> cookieOptions) : BaseApiController
{
    private readonly RefreshTokenCookieOptions _cookieOptions = cookieOptions.Value;

    /// <summary>Creates an account and signs the new user in.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingOptions.AuthenticationPolicy)]
    [ProducesResponseType(typeof(AuthenticationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AuthenticationResult>> Register(
        [FromBody] RegisterCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Commands.Send(command, cancellationToken);

        return AuthenticationResponse(result);
    }

    /// <summary>Exchanges credentials for a token pair.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingOptions.AuthenticationPolicy)]
    [ProducesResponseType(typeof(AuthenticationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AuthenticationResult>> Login(
        [FromBody] LoginCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Commands.Send(command, cancellationToken);

        return AuthenticationResponse(result);
    }

    /// <summary>Rotates a refresh token into a new pair.</summary>
    /// <remarks>
    /// Anonymous on purpose. The caller's access token has usually expired by the time
    /// they refresh — that is what refreshing is for — so requiring a valid one would
    /// make the endpoint unreachable exactly when it is needed. The refresh token is
    /// itself the credential.
    /// </remarks>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingOptions.RefreshPolicy)]
    [ProducesResponseType(typeof(AuthenticationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AuthenticationResult>> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var token = ResolveRefreshToken(request.RefreshToken);

        var result = await Commands.Send(new RefreshTokenCommand(token), cancellationToken);

        return AuthenticationResponse(result);
    }

    /// <summary>Revokes the session and blacklists the presented access token.</summary>
    /// <remarks>
    /// Requires authentication, so that sign-out is always attributable to a caller and
    /// cannot be used anonymously to probe which refresh tokens exist.
    /// </remarks>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(
        [FromBody] RefreshTokenRequest? request,
        CancellationToken cancellationToken)
    {
        var token = ResolveRefreshToken(request?.RefreshToken);

        await Commands.Send(new LogoutCommand(token), cancellationToken);

        ClearRefreshTokenCookie();

        return NoContent();
    }

    /// <summary>Returns the authenticated caller's profile with current roles and permissions.</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserProfile), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserProfile>> Me(CancellationToken cancellationToken)
    {
        var profile = await Queries.Query(new GetCurrentUserQuery(), cancellationToken);

        return Ok(profile);
    }

    /// <summary>
    /// Prefers the request body, falling back to the cookie when cookie mode is on.
    /// Returning an empty string rather than throwing lets the handler produce the
    /// single generic 401 that every refresh failure shares.
    /// </summary>
    private string ResolveRefreshToken(string? fromBody)
    {
        if (!string.IsNullOrWhiteSpace(fromBody))
        {
            return fromBody;
        }

        if (_cookieOptions.Enabled &&
            Request.Cookies.TryGetValue(RefreshTokenCookieOptions.CookieName, out var fromCookie))
        {
            return fromCookie;
        }

        return string.Empty;
    }

    private ActionResult<AuthenticationResult> AuthenticationResponse(AuthenticationResult result)
    {
        // Never cached, anywhere. The body carries both tokens, and a shared or disk
        // cache holding them would outlive the session they belong to.
        Response.Headers.CacheControl = "no-store";
        Response.Headers.Pragma = "no-cache";

        SetRefreshTokenCookie(result);

        return Ok(result);
    }

    private void SetRefreshTokenCookie(AuthenticationResult result)
    {
        if (!_cookieOptions.Enabled)
        {
            return;
        }

        Response.Cookies.Append(
            RefreshTokenCookieOptions.CookieName,
            result.RefreshToken,
            new CookieOptions
            {
                // Unreachable from JavaScript: the point of the mode.
                HttpOnly = true,
                // Never sent over cleartext, even if a downgrade is attempted.
                Secure = true,
                // Not configurable — see RefreshTokenCookieOptions.
                SameSite = SameSiteMode.Strict,
                Expires = result.RefreshTokenExpiresAt,
                Path = _cookieOptions.Path,
                Domain = string.IsNullOrWhiteSpace(_cookieOptions.Domain) ? null : _cookieOptions.Domain,
                IsEssential = true,
            });
    }

    private void ClearRefreshTokenCookie()
    {
        if (!_cookieOptions.Enabled)
        {
            return;
        }

        // Attributes must match those used when writing, or the browser treats this as
        // a different cookie and the original survives sign-out.
        Response.Cookies.Delete(
            RefreshTokenCookieOptions.CookieName,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = _cookieOptions.Path,
                Domain = string.IsNullOrWhiteSpace(_cookieOptions.Domain) ? null : _cookieOptions.Domain,
            });
    }
}

/// <summary>
/// Body for refresh and sign-out. Optional, because in cookie mode the token is not in
/// the body at all.
/// </summary>
public sealed record RefreshTokenRequest(string? RefreshToken);
