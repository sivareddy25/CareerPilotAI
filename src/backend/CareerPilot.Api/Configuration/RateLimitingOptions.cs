namespace CareerPilot.Api.Configuration;

/// <summary>
/// Rate limits for the authentication endpoints.
/// </summary>
/// <remarks>
/// Account lockout stops an attacker guessing many passwords against *one* account.
/// It does nothing about the inverse — one common password tried against thousands of
/// accounts — because no single account ever accumulates failures. Rate limiting by
/// caller is what covers that case, so the two mechanisms are complements, not
/// alternatives.
/// </remarks>
public sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    /// <summary>Policy applied to sign-in and registration.</summary>
    public const string AuthenticationPolicy = "auth";

    /// <summary>Policy applied to token refresh.</summary>
    public const string RefreshPolicy = "auth-refresh";

    /// <summary>Attempts permitted per window, per caller, for sign-in and registration.</summary>
    public int AuthenticationPermitLimit { get; set; } = 10;

    public int AuthenticationWindowSeconds { get; set; } = 60;

    /// <summary>
    /// Refresh gets a higher limit than sign-in: it is a legitimate, automated,
    /// repeating call, and several browser tabs refreshing at once must not look like
    /// an attack.
    /// </summary>
    public int RefreshPermitLimit { get; set; } = 30;

    public int RefreshWindowSeconds { get; set; } = 60;
}
