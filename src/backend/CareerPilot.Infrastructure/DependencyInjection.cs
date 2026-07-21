using CareerPilot.Application.Abstractions.Persistence;
using CareerPilot.Infrastructure.Authentication;
using CareerPilot.Infrastructure.Configuration;
using CareerPilot.Infrastructure.Persistence;
using CareerPilot.Infrastructure.Persistence.Interceptors;
using CareerPilot.Infrastructure.Persistence.Repositories;
using CareerPilot.Infrastructure.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CareerPilot.Infrastructure;

/// <summary>
/// Composition entry point for the Infrastructure layer.
/// Registers DbContext pooling, PostgreSQL connections, interceptors, and persistence options.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        services.AddOptions(configuration);
        services.AddPersistenceInfrastructure(configuration);
        services.AddAuthenticationInfrastructure(configuration, environment);

        return services;
    }

    private static void AddOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));
        services.Configure<RedisOptions>(configuration.GetSection(RedisOptions.SectionName));
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<OpenAIOptions>(configuration.GetSection(OpenAIOptions.SectionName));
        services.Configure<PlaywrightOptions>(configuration.GetSection(PlaywrightOptions.SectionName));
    }

    private static void AddPersistenceInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var dbOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>()
            ?? new DatabaseOptions();

        var connectionString = !string.IsNullOrWhiteSpace(dbOptions.ConnectionString)
            ? dbOptions.ConnectionString
            : configuration.GetConnectionString("DefaultConnection") ?? string.Empty;

        services.AddSingleton<AuditableEntityInterceptor>();

        services.AddDbContextPool<ApplicationDbContext>((serviceProvider, options) =>
        {
            var interceptor = serviceProvider.GetRequiredService<AuditableEntityInterceptor>();

            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                npgsql.CommandTimeout(dbOptions.CommandTimeoutSeconds);
                npgsql.EnableRetryOnFailure(
                    maxRetryCount: dbOptions.MaxRetryCount,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
            });

            options.UseSnakeCaseNamingConvention();
            options.AddInterceptors(interceptor);

            if (dbOptions.EnableSensitiveDataLogging)
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        services.AddScoped<IdentitySeeder>();
    }
}
