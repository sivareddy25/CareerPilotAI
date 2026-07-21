namespace CareerPilot.Api.Configuration;

/// <summary>
/// Cross-origin settings. Origins are configuration-driven rather than hard-coded so
/// production hosts are set per environment without a rebuild.
/// </summary>
public sealed class CorsOptions
{
    public const string SectionName = "Cors";

    public const string PolicyName = "CareerPilotCors";

    /// <summary>
    /// Explicit origin allow-list. Empty means no cross-origin access is granted —
    /// a missing configuration section fails closed, not open.
    /// </summary>
    public string[] AllowedOrigins { get; set; } = [];

    /// <summary>
    /// Required for cookie-based auth later. Only legal alongside an explicit
    /// origin list — the CORS spec forbids credentials with a wildcard origin.
    /// </summary>
    public bool AllowCredentials { get; set; } = true;
}
