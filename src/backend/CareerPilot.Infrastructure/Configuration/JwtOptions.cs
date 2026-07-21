namespace CareerPilot.Infrastructure.Configuration;

/// <summary>
/// Token issuance settings. Bound now so the shape is settled; no authentication
/// is wired in this phase. <see cref="SigningKey"/> must come from a secret store
/// in any deployed environment, never from appsettings.
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public string SigningKey { get; set; } = string.Empty;

    public int AccessTokenMinutes { get; set; } = 15;

    public int RefreshTokenDays { get; set; } = 7;
}
