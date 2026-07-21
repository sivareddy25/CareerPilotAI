namespace CareerPilot.Infrastructure.Configuration;

/// <summary>PostgreSQL connection and resiliency settings.</summary>
public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    public string ConnectionString { get; set; } = string.Empty;

    public int MaxRetryCount { get; set; } = 3;

    public int CommandTimeoutSeconds { get; set; } = 30;

    /// <summary>Never enable outside local development — it logs parameter values.</summary>
    public bool EnableSensitiveDataLogging { get; set; }

    /// <summary>Maximum connections held in the Npgsql pool.</summary>
    public int MaxPoolSize { get; set; } = 100;

    /// <summary>Minimum connections kept warm in the pool.</summary>
    public int MinPoolSize { get; set; } = 5;

    /// <summary>Seconds a connection can sit idle before being pruned.</summary>
    public int ConnectionIdleLifetimeSeconds { get; set; } = 300;
}
