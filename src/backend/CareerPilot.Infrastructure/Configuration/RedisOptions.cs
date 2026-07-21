namespace CareerPilot.Infrastructure.Configuration;

/// <summary>Redis cache settings. Not consumed until the caching adapter lands.</summary>
public sealed class RedisOptions
{
    public const string SectionName = "Redis";

    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>Key prefix, so multiple environments can share one Redis instance safely.</summary>
    public string InstanceName { get; set; } = "careerpilot:";

    public int DefaultExpirationMinutes { get; set; } = 60;
}
