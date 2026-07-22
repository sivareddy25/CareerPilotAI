using CareerPilot.Domain.Entities.Profiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareerPilot.Infrastructure.Persistence.Configurations.Profiles;

internal sealed class ProfileSkillConfiguration : IEntityTypeConfiguration<ProfileSkill>
{
    public void Configure(EntityTypeBuilder<ProfileSkill> builder)
    {
        builder.ToTable("profile_skill");

        builder.Property(skill => skill.Name)
            .HasMaxLength(100)
            .IsRequired();

        // One row per skill per profile — a duplicate would double-count in the overlap score.
        // The domain de-duplicates on write; this makes the database enforce it too.
        builder.HasIndex(skill => new { skill.ProfileId, skill.Name })
            .IsUnique()
            .HasDatabaseName("ix_profile_skill_profile_name");
    }
}
