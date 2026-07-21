using CareerPilot.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareerPilot.Infrastructure.Persistence.Configurations.Identity;

public sealed class RolePermissionConfiguration : EntityTypeConfiguration<RolePermission>
{
    public override void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        base.Configure(builder);

        builder.ToTable("role_permissions");

        builder.HasIndex(rolePermission => new { rolePermission.RoleId, rolePermission.PermissionId })
            .IsUnique()
            .HasDatabaseName("ix_role_permissions_role_id_permission_id");

        builder.HasOne(rolePermission => rolePermission.Role)
            .WithMany(role => role.RolePermissions)
            .HasForeignKey(rolePermission => rolePermission.RoleId)
            // Removing a role should take its grants with it — an orphaned grant row
            // has no meaning.
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.HasOne(rolePermission => rolePermission.Permission)
            .WithMany(permission => permission.RolePermissions)
            .HasForeignKey(rolePermission => rolePermission.PermissionId)
            // Restrict: a permission still granted to a role is load-bearing, and
            // deleting it would quietly widen or break access rules.
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }
}
