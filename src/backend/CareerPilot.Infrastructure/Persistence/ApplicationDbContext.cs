using System.Linq.Expressions;
using CareerPilot.Domain.Abstractions;
using CareerPilot.Domain.Entities;
using CareerPilot.Domain.Entities.Identity;
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

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        // Globally convert all DateTime properties to UTC
        configurationBuilder
            .Properties<DateTime>()
            .HaveConversion<UtcDateTimeConverter>();
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
            // Global soft delete filter
            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
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
