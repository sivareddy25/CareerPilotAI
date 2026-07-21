using CareerPilot.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareerPilot.Infrastructure.Persistence.Configurations.Identity;

public sealed class RoleConfiguration : EntityTypeConfiguration<Role>
{
    public override void Configure(EntityTypeBuilder<Role> builder)
    {
        base.Configure(builder);

        builder.ToTable("roles");

        builder.Property(role => role.Name)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(role => role.NormalizedName)
            .IsRequired()
            .HasMaxLength(64);

        builder.HasIndex(role => role.NormalizedName)
            .IsUnique()
            .HasDatabaseName("ix_roles_normalized_name");

        builder.Property(role => role.Description).HasMaxLength(256);
        builder.Property(role => role.IsSystemRole).HasDefaultValue(false);

        builder.Metadata
            .FindNavigation(nameof(Role.RolePermissions))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Metadata
            .FindNavigation(nameof(Role.UserRoles))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
