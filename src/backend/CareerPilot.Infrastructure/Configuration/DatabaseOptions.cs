namespace CareerPilot.Infrastructure.Configuration;

/// <summary>PostgreSQL connection settings. Not consumed until EF Core lands.</summary>
public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    public string ConnectionString { get; set; } = string.Empty;

    public int MaxRetryCount { get; set; } = 3;

    public int CommandTimeoutSeconds { get; set; } = 30;

    /// <summary>Never enable outside local development — it logs parameter values.</summary>
    public bool EnableSensitiveDataLogging { get; set; }
}
