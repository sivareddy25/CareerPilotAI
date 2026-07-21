using CareerPilot.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareerPilot.Infrastructure.Persistence.Configurations.Identity;

public sealed class PermissionConfiguration : EntityTypeConfiguration<Permission>
{
    public override void Configure(EntityTypeBuilder<Permission> builder)
    {
        base.Configure(builder);

        builder.ToTable("permissions");

        builder.Property(permission => permission.Name)
            .IsRequired()
            .HasMaxLength(128);

        // Permission names are matched literally against policy strings, so two rows
        // differing only by case would silently create two distinct capabilities.
        builder.HasIndex(permission => permission.Name)
            .IsUnique()
            .HasDatabaseName("ix_permissions_name");

        builder.Property(permission => permission.Description).HasMaxLength(256);
        builder.Property(permission => permission.Category).HasMaxLength(64);

        builder.Metadata
            .FindNavigation(nameof(Permission.RolePermissions))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
