using CareerPilot.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareerPilot.Infrastructure.Persistence.Configurations.Identity;

public sealed class UserRoleConfiguration : EntityTypeConfiguration<UserRole>
{
    public override void Configure(EntityTypeBuilder<UserRole> builder)
    {
        base.Configure(builder);

        builder.ToTable("user_roles");

        // Uniqueness lives in an index rather than a composite key, because the entity
        // keeps a surrogate id so it can carry an audit trail. The guarantee is the
        // same; only the mechanism differs.
        builder.HasIndex(userRole => new { userRole.UserId, userRole.RoleId })
            .IsUnique()
            .HasDatabaseName("ix_user_roles_user_id_role_id");

        builder.HasOne(userRole => userRole.User)
            .WithMany(user => user.UserRoles)
            .HasForeignKey(userRole => userRole.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.HasOne(userRole => userRole.Role)
            .WithMany(role => role.UserRoles)
            .HasForeignKey(userRole => userRole.RoleId)
            // Restrict, not cascade: deleting a role that is still assigned must fail
            // loudly. Silently stripping privileges from every holder is the kind of
            // change that should require a deliberate migration.
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasQueryFilter(userRole => !userRole.User!.IsDeleted);
    }
}
