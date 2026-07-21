using CareerPilot.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CareerPilot.Infrastructure.Persistence;

/// <summary>
/// Factory used by EF Core CLI tooling (dotnet ef migrations) at design time.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../CareerPilot.Api");
        if (!Directory.Exists(basePath))
        {
            basePath = Directory.GetCurrentDirectory();
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? configuration["Database:ConnectionString"]
            ?? "Host=localhost;Port=5432;Database=careerpilot;Username=careerpilot;Password=careerpilot";

        var builder = new DbContextOptionsBuilder<ApplicationDbContext>();

        builder.UseNpgsql(connectionString, npgsql =>
        {
            npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
            npgsql.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorCodesToAdd: null);
        });

        builder.UseSnakeCaseNamingConvention();
        builder.AddInterceptors(new AuditableEntityInterceptor());

        return new ApplicationDbContext(builder.Options);
    }
}
