using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CareerPilot.Application.Abstractions.Authentication;
using CareerPilot.Application.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace CareerPilot.Infrastructure.Authentication;

/// <summary>
/// Reads the current request's identity from the validated <see cref="ClaimsPrincipal"/>.
/// </summary>
/// <remarks>
/// Every value here comes from a token whose signature and expiry the authentication
/// middleware has already checked, so the claims can be trusted. Nothing is read from
/// the request body, the query string, or a client-supplied header — those are
/// attacker-controlled, and treating them as identity is the classic way this class
/// becomes an impersonation vector.
///
/// <see cref="IpAddress"/> and <see cref="UserAgent"/> are the exceptions: they *are*
/// client-influenced and are recorded for forensics only. Nothing branches on them.
/// </remarks>
internal sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public Guid? UserId =>
        Guid.TryParse(FindClaim(JwtRegisteredClaimNames.Sub) ?? FindClaim(ClaimTypes.NameIdentifier), out var id)
            ? id
            : null;

    public string? Email => FindClaim(JwtRegisteredClaimNames.Email) ?? FindClaim(ClaimTypes.Email);

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public string? TokenId => FindClaim(JwtRegisteredClaimNames.Jti);

    public DateTime? TokenExpiresAt =>
        long.TryParse(FindClaim(JwtRegisteredClaimNames.Exp), out var exp)
            // exp is seconds since the Unix epoch, and EpochTime returns UTC — the Kind
            // matters because the blacklist computes a TTL by subtracting DateTime.UtcNow.
            ? EpochTime.DateTime(exp)
            : null;

    public string? SecurityStamp => FindClaim(AuthenticationClaimTypes.SecurityStamp);

    public IReadOnlyCollection<string> Roles => FindAll(AuthenticationClaimTypes.Role);

    public IReadOnlyCollection<string> Permissions => FindAll(AuthenticationClaimTypes.Permission);

    /// <summary>
    /// Connection-level remote address. Behind a proxy this is the proxy unless
    /// forwarded-headers middleware is configured — deliberately not read from
    /// <c>X-Forwarded-For</c> here, since that header is trivially spoofed by any
    /// client and this value ends up in audit records.
    /// </summary>
    public string? IpAddress =>
        httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

    public string? UserAgent =>
        httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString() is { Length: > 0 } agent
            ? agent
            : null;

    private string? FindClaim(string type) => Principal?.FindFirst(type)?.Value;

    private string[] FindAll(string type) =>
        Principal?.FindAll(type).Select(claim => claim.Value).ToArray() ?? [];
}
