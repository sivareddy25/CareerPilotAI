using CareerPilot.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareerPilot.Infrastructure.Persistence.Configurations.Identity;

public sealed class RefreshTokenConfiguration : EntityTypeConfiguration<RefreshToken>
{
    public override void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        base.Configure(builder);

        builder.ToTable("refresh_tokens");

        // Base64 SHA-256 is 44 characters. Sized exactly so a row that is not a hash
        // is rejected by the database rather than stored.
        builder.Property(token => token.TokenHash)
            .IsRequired()
            .HasMaxLength(64);

        // Unique because a collision would mean one presented token resolving to two
        // accounts. Also the index every refresh call looks up by.
        builder.HasIndex(token => token.TokenHash)
            .IsUnique()
            .HasDatabaseName("ix_refresh_tokens_token_hash");

        // Supports "revoke everything for this user", which runs on sign-out,
        // password change, and reuse detection.
        builder.HasIndex(token => new { token.UserId, token.RevokedAt })
            .HasDatabaseName("ix_refresh_tokens_user_id_revoked_at");

        builder.Property(token => token.CreatedByIp).HasMaxLength(64);
        builder.Property(token => token.RevokedByIp).HasMaxLength(64);
        builder.Property(token => token.CreatedByUserAgent).HasMaxLength(512);

        builder.Property(token => token.RevocationReason).HasConversion<int>();

        builder.HasOne(token => token.User)
            .WithMany(user => user.RefreshTokens)
            .HasForeignKey(token => token.UserId)
            // Cascade is correct here and only here: a token has no meaning without
            // its user, and hard-deleting a user must not leave usable credentials
            // behind. Users are soft-deleted in normal operation, so this fires only
            // on a genuine purge.
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        // Mirrors the owning user's soft-delete filter. EF otherwise warns that a
        // required navigation points at a filtered entity, and more importantly a
        // deleted user's tokens would still be queryable.
        builder.HasQueryFilter(token => !token.User!.IsDeleted);
    }
}
