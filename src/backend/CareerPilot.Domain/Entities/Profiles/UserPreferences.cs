using CareerPilot.Domain.Enums;

namespace CareerPilot.Domain.Entities.Profiles;

/// <summary>
/// Display and notification preferences.
/// </summary>
/// <remarks>
/// Modelled as an owned type rather than its own entity: preferences have no identity
/// or lifetime apart from the profile that holds them, are always loaded and saved with
/// it, and are never queried independently. EF stores them as columns on the same row,
/// so reading a profile stays a single-table read.
///
/// Not a record, despite being value-like — EF Core needs a mutable reference type it
/// can materialise and change-track in place.
/// </remarks>
public sealed class UserPreferences
{
    public ThemePreference Theme { get; private set; } = ThemePreference.System;

    public DateFormatPreference DateFormat { get; private set; } = DateFormatPreference.IsoYearMonthDay;

    public TimeFormatPreference TimeFormat { get; private set; } = TimeFormatPreference.TwentyFourHour;

    /// <summary>
    /// Transactional email about the user's own activity. Defaults on, because a user
    /// who cannot receive account notices has no way to learn about changes to their
    /// own account.
    /// </summary>
    public bool EmailNotifications { get; private set; } = true;

    /// <summary>
    /// In-application notifications. Distinct from email so a user can stay informed
    /// while signed in without adding to their inbox.
    /// </summary>
    public bool InAppNotifications { get; private set; } = true;

    /// <summary>
    /// Product news and announcements. Defaults <b>off</b>: marketing contact is
    /// opt-in, not opt-out, and pre-ticking it is the pattern GDPR Article 7 and
    /// similar regimes treat as invalid consent.
    /// </summary>
    public bool MarketingEmails { get; private set; }

    /// <summary>Periodic digest of activity.</summary>
    public bool WeeklySummaryEmails { get; private set; } = true;

    public static UserPreferences CreateDefault() => new();

    public void Update(
        ThemePreference theme,
        DateFormatPreference dateFormat,
        TimeFormatPreference timeFormat)
    {
        Theme = theme;
        DateFormat = dateFormat;
        TimeFormat = timeFormat;
    }

    public void UpdateNotifications(
        bool emailNotifications,
        bool inAppNotifications,
        bool marketingEmails,
        bool weeklySummaryEmails)
    {
        EmailNotifications = emailNotifications;
        InAppNotifications = inAppNotifications;
        MarketingEmails = marketingEmails;
        WeeklySummaryEmails = weeklySummaryEmails;
    }
}
