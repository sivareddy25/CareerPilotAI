namespace CareerPilot.Application.System.Models;

public sealed record ComponentHealthDto(
    string ComponentName,
    bool IsHealthy,
    string StatusText,
    string Details,
    DateTimeOffset CheckedAt);

public sealed record SystemHealthStatusDto(
    bool IsOverallHealthy,
    ComponentHealthDto PostgresDb,
    ComponentHealthDto RedisCache,
    ComponentHealthDto OllamaLlmEngine,
    ComponentHealthDto PlaywrightBrowserDriver,
    ComponentHealthDto OAuthIntegrations,
    DateTimeOffset CheckedAt);
