using System.Linq.Expressions;
using CareerPilot.Domain.Abstractions;
using CareerPilot.Domain.Entities;
using CareerPilot.Domain.Entities.Identity;
using CareerPilot.Domain.Entities.Profiles;
using CareerPilot.Domain.Resumes;
using CareerPilot.Infrastructure.Persistence.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CareerPilot.Infrastructure.Persistence;

/// <summary>
/// Main EF Core DbContext for CareerPilot AI persistence.
/// Configured with DbContext pooling, UTC date handling, soft deletion, and snake_case naming.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();

    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

    public DbSet<Resume> Resumes => Set<Resume>();

    public DbSet<CareerPilot.Domain.Jobs.Entities.Job> Jobs => Set<CareerPilot.Domain.Jobs.Entities.Job>();

    public DbSet<CareerPilot.Domain.Jobs.Entities.Company> Companies => Set<CareerPilot.Domain.Jobs.Entities.Company>();

    public DbSet<CareerPilot.Domain.Jobs.Entities.JobSyncLog> JobSyncLogs => Set<CareerPilot.Domain.Jobs.Entities.JobSyncLog>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        // Globally convert all DateTime properties to UTC
        configurationBuilder
            .Properties<DateTime>()
            .HaveConversion<UtcDateTimeConverter>();

        // Same for DateTimeOffset, which Npgsql rejects outright unless the offset is zero.
        configurationBuilder
            .Properties<DateTimeOffset>()
            .HaveConversion<UtcDateTimeOffsetConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Ignore domain events collection from EF Core schema generation
        modelBuilder.Ignore<IDomainEvent>();

        // Auto-discover and apply IEntityTypeConfiguration<T> classes in assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Apply global filters and concurrency tokens across entity hierarchy
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Global soft delete filter, applied only where a configuration has not
            // already defined one. EF permits a single filter per entity, so applying
            // this unconditionally would silently replace the richer filters written in
            // IEntityTypeConfiguration classes — for example the one on UserProfile
            // that also excludes profiles whose owning user is deleted.
            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType)
                && entityType.GetQueryFilter() is null)
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(GetSoftDeleteFilter(entityType.ClrType));
            }

            // Optimistic concurrency configuration for AuditableEntity
            if (typeof(AuditableEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(AuditableEntity.Version))
                    .IsConcurrencyToken();
            }
        }
    }

    private static LambdaExpression GetSoftDeleteFilter(Type entityType)
    {
        var parameter = Expression.Parameter(entityType, "e");
        var property = Expression.Property(parameter, nameof(ISoftDelete.IsDeleted));
        var falseConstant = Expression.Constant(false);
        var comparison = Expression.Equal(property, falseConstant);

        return Expression.Lambda(comparison, parameter);
    }
}
