using CareerPilot.Infrastructure.Persistence.Seeding;

namespace CareerPilot.Api.Extensions;

/// <summary>
/// Runs identity seeding at startup.
/// </summary>
public static class SeedingExtensions
{
    /// <summary>
    /// Ensures system roles and permissions exist before the application serves traffic.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Seeding is idempotent and additive, so running it on every start is safe. It is
    /// deliberately separate from migrations: this applies no schema changes and
    /// assumes the schema is already current.
    /// </para>
    /// <para>
    /// A failure here is logged but does not stop the host. The consequence of an
    /// unseeded database is that new registrations get no default role — degraded, and
    /// loudly logged, but not a reason to refuse every request including the health
    /// probe that would report the problem.
    /// </para>
    /// </remarks>
    public static async Task<WebApplication> SeedIdentityAsync(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        using var scope = app.Services.CreateScope();

        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("CareerPilot.Seeding");

        try
        {
            var seeder = scope.ServiceProvider.GetRequiredService<IdentitySeeder>();
            await seeder.SeedAsync();
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Identity seeding failed. Roles and permissions may be missing; run migrations and restart.");
        }

        return app;
    }
}
