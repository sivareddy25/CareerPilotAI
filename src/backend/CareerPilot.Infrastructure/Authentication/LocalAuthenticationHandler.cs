using System.Security.Claims;
using System.Text.Encodings.Web;
using CareerPilot.Application.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;

namespace CareerPilot.Infrastructure.Authentication;

/// <summary>
/// Authenticates every request as the single local user, for Local mode.
/// </summary>
/// <remarks>
/// Local mode issues no tokens, so there is no bearer scheme. The authorization
/// middleware still runs, and without *any* registered scheme it cannot even build a
/// challenge — every request fails with "No authenticationScheme was specified" rather
/// than being allowed through. Registering this handler as the default scheme is what
/// makes <c>[Authorize]</c> endpoints reachable on a local install.
///
/// The identity it produces mirrors <see cref="LocalCurrentUserService"/>; permissions
/// are left off the principal deliberately, because <see cref="LocalPermissionService"/>
/// grants them and duplicating the list here would create a second source of truth.
/// </remarks>
internal sealed class LocalAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "LocalMode";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var identity = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, LocalUserProvider.DefaultLocalUserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, "local.user@careerpilot.internal"),
                new Claim(AuthenticationClaimTypes.Role, "Admin"),
                new Claim(AuthenticationClaimTypes.Role, "User"),
            ],
            authenticationType: SchemeName,
            nameType: JwtRegisteredClaimNames.Sub,
            roleType: AuthenticationClaimTypes.Role);

        var principal = new ClaimsPrincipal(identity);

        return Task.FromResult(
            AuthenticateResult.Success(new AuthenticationTicket(principal, SchemeName)));
    }
}
