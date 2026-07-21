using CareerPilot.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CareerPilot.Infrastructure;

/// <summary>
/// Composition entry point for the Infrastructure layer — the only member of this
/// assembly the Api project is permitted to call.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions(configuration);

        // Adapters (DbContext, Redis, Playwright, AI clients) register here as
        // each arrives, always bound to a port declared in the Application layer.

        return services;
    }

    /// <summary>
    /// Binds every configuration section to its options type. Binding is deliberately
    /// separated from consumption: the shape of configuration is settled now, while the
    /// services that read it arrive later.
    /// </summary>
    private static void AddOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));
        services.Configure<RedisOptions>(configuration.GetSection(RedisOptions.SectionName));
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<OpenAIOptions>(configuration.GetSection(OpenAIOptions.SectionName));
        services.Configure<PlaywrightOptions>(configuration.GetSection(PlaywrightOptions.SectionName));
    }
}
