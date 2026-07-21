namespace CareerPilot.Application.Authentication;

/// <summary>
/// Claim type names used in issued access tokens.
/// </summary>
/// <remarks>
/// Short, literal names rather than the SOAP-era
/// <c>http://schemas.microsoft.com/ws/2008/06/identity/claims/…</c> URIs that .NET maps
/// to by default. The mapping is switched off at validation so that what is issued is
/// exactly what is read back — with mapping on, a claim's name silently differs
/// between the code that writes it and the code that checks it.
/// </remarks>
public static class AuthenticationClaimTypes
{
    /// <summary>Role name. Matches <c>TokenValidationParameters.RoleClaimType</c>.</summary>
    public const string Role = "role";

    /// <summary>
    /// A single granted permission. Repeated once per permission — a JWT may carry the
    /// same claim type many times, so no delimiter parsing is involved.
    /// </summary>
    public const string Permission = "permission";

    /// <summary>
    /// The user's <c>SecurityStamp</c> at issue time. Compared at refresh to reject
    /// tokens minted before a credential change.
    /// </summary>
    public const string SecurityStamp = "security_stamp";
}
