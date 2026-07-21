using CareerPilot.Application.Profiles.Validation;
using CareerPilot.Domain.Entities.Profiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareerPilot.Infrastructure.Persistence.Configurations.Profiles;

public sealed class UserProfileConfiguration : EntityTypeConfiguration<UserProfile>
{
    public override void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        base.Configure(builder);

        builder.ToTable("user_profiles");

        // Unique rather than merely indexed: the relationship is one-to-one, and the
        // database is what guarantees a second profile row cannot be created for an
        // account by a concurrent request.
        builder.HasIndex(profile => profile.UserId)
            .IsUnique()
            .HasDatabaseName("ix_user_profiles_user_id");

        builder.Property(profile => profile.DisplayName).HasMaxLength(ProfileRules.DisplayNameMaxLength);
        builder.Property(profile => profile.PhoneNumber).HasMaxLength(ProfileRules.PhoneMaxLength);
        builder.Property(profile => profile.Country).HasMaxLength(2).IsFixedLength();
        builder.Property(profile => profile.State).HasMaxLength(ProfileRules.LocationMaxLength);
        builder.Property(profile => profile.City).HasMaxLength(ProfileRules.LocationMaxLength);
        builder.Property(profile => profile.TimeZone).HasMaxLength(ProfileRules.TimeZoneMaxLength);
        builder.Property(profile => profile.PreferredLanguage).HasMaxLength(ProfileRules.LanguageMaxLength);
        builder.Property(profile => profile.Bio).HasMaxLength(ProfileRules.BioMaxLength);
        builder.Property(profile => profile.ProfilePictureUrl).HasMaxLength(ProfileRules.UrlMaxLength);
        builder.Property(profile => profile.LinkedInUrl).HasMaxLength(ProfileRules.UrlMaxLength);
        builder.Property(profile => profile.GitHubUrl).HasMaxLength(ProfileRules.UrlMaxLength);
        builder.Property(profile => profile.PortfolioUrl).HasMaxLength(ProfileRules.UrlMaxLength);

        // Owned: preferences become columns on this table rather than a joined row.
        // They are always read and written with the profile, so a separate table would
        // buy nothing and cost a join on every profile load.
        builder.OwnsOne(profile => profile.Preferences, preferences =>
        {
            // Enums persist as int. Storing the name instead would be more readable in
            // the database but would make renaming a member a breaking data change.
            preferences.Property(p => p.Theme).HasColumnName("pref_theme").HasConversion<int>();
            preferences.Property(p => p.DateFormat).HasColumnName("pref_date_format").HasConversion<int>();
            preferences.Property(p => p.TimeFormat).HasColumnName("pref_time_format").HasConversion<int>();

            preferences.Property(p => p.EmailNotifications)
                .HasColumnName("pref_email_notifications").HasDefaultValue(true);
            preferences.Property(p => p.InAppNotifications)
                .HasColumnName("pref_in_app_notifications").HasDefaultValue(true);
            preferences.Property(p => p.MarketingEmails)
                .HasColumnName("pref_marketing_emails").HasDefaultValue(false);
            preferences.Property(p => p.WeeklySummaryEmails)
                .HasColumnName("pref_weekly_summary_emails").HasDefaultValue(true);
        });

        // Required, so EF always materialises it rather than leaving a null the domain
        // has no way to represent.
        builder.Navigation(profile => profile.Preferences).IsRequired();

        builder.HasOne(profile => profile.User)
            .WithOne()
            .HasForeignKey<UserProfile>(profile => profile.UserId)
            // A profile has no meaning without its account. Users are soft-deleted in
            // normal operation, so this only fires on a genuine purge.
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        // Mirrors the user's soft-delete filter in addition to the profile's own, so a
        // deleted account cannot surface through its profile. Written explicitly
        // because the convention in OnModelCreating only knows about this entity's own
        // IsDeleted flag.
        builder.HasQueryFilter(profile => !profile.IsDeleted && !profile.User!.IsDeleted);
    }
}
