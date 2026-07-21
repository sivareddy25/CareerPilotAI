namespace CareerPilot.Api.Configuration;

/// <summary>
/// Optional delivery of the refresh token as an HTTP-only cookie instead of relying
/// solely on the response body.
/// </summary>
/// <remarks>
/// <para>
/// Disabled by default, so the baseline is a pure bearer-token architecture: nothing
/// is sent automatically by the browser, and therefore nothing is forgeable
/// cross-site. CSRF is not a concern in that mode because there is no ambient
/// credential for an attacker's page to ride on.
/// </para>
/// <para>
/// Turning this on trades that property for a different one. A cookie marked HttpOnly
/// is unreadable from JavaScript, which puts the refresh token out of reach of an XSS
/// payload — a real gain, since a stolen refresh token is a durable session. But it
/// reintroduces ambient authority, which is why <c>SameSite=Strict</c> is not
/// configurable here: it is what keeps the cookie from being attached to cross-site
/// requests, and it is the only thing standing between this mode and CSRF.
/// </para>
/// </remarks>
public sealed class RefreshTokenCookieOptions
{
    public const string SectionName = "Authentication:RefreshTokenCookie";

    public const string CookieName = "careerpilot_refresh_token";

    public bool Enabled { get; set; }

    /// <summary>
    /// Scoped to the refresh endpoint so the cookie is not attached to every request
    /// to the API. Narrower path, smaller exposure.
    /// </summary>
    public string Path { get; set; } = "/api/v1/auth";

    /// <summary>
    /// Leave empty to bind to the exact host. Setting a parent domain shares the
    /// cookie with every subdomain, including any that is compromised later.
    /// </summary>
    public string? Domain { get; set; }
}
