using CareerPilot.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareerPilot.Infrastructure.Persistence.Configurations.Identity;

public sealed class UserConfiguration : EntityTypeConfiguration<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        base.Configure(builder);

        builder.ToTable("users");

        builder.Property(user => user.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(user => user.NormalizedEmail)
            .IsRequired()
            .HasMaxLength(256);

        // The uniqueness guarantee for accounts, and the index every login uses.
        // It is filtered on is_deleted so that soft-deleting a user frees their address
        // for re-registration — without the filter, a deleted account would block its
        // own email forever while remaining invisible to every query.
        builder.HasIndex(user => user.NormalizedEmail)
            .IsUnique()
            .HasDatabaseName("ix_users_normalized_email")
            .HasFilter("is_deleted = false");

        builder.Property(user => user.PasswordHash)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(user => user.FirstName).HasMaxLength(100);
        builder.Property(user => user.LastName).HasMaxLength(100);

        builder.Property(user => user.SecurityStamp)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(user => user.EmailConfirmed).HasDefaultValue(false);
        builder.Property(user => user.IsActive).HasDefaultValue(true);
        builder.Property(user => user.AccessFailedCount).HasDefaultValue(0);

        // Backing fields are the source of truth: the public members expose read-only
        // views, so EF must write through the field rather than the property.
        builder.Metadata
            .FindNavigation(nameof(User.UserRoles))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Metadata
            .FindNavigation(nameof(User.RefreshTokens))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
