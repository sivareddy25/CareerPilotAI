using CareerPilot.Domain.Entities.Profiles;
using CareerPilot.Domain.Enums;

namespace CareerPilot.Application.Profiles.Models;

/// <summary>
/// The full profile as returned to its owner.
/// </summary>
/// <remarks>
/// Composed from two aggregates: name and email come from the <c>User</c>, everything
/// else from the <c>UserProfile</c>. The client sees one resource and does not need to
/// know the seam exists.
/// </remarks>
public sealed record ProfileDto(
    Guid UserId,
    string Email,
    bool EmailConfirmed,
    string? FirstName,
    string? LastName,
    string? DisplayName,
    string? PhoneNumber,
    string? Country,
    string? State,
    string? City,
    string? TimeZone,
    string? PreferredLanguage,
    string? ProfilePictureUrl,
    string? Bio,
    string? LinkedInUrl,
    string? GitHubUrl,
    string? PortfolioUrl,
    PreferencesDto Preferences)
{
    public static ProfileDto From(UserProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        // The repository always includes the user; a null here means a profile row
        // outlived its account, which is a data-integrity fault worth failing loudly on
        // rather than papering over with empty strings.
        var user = profile.User
            ?? throw new InvalidOperationException(
                $"Profile {profile.Id} was loaded without its user.");

        return new ProfileDto(
            profile.UserId,
            user.Email,
            user.EmailConfirmed,
            user.FirstName,
            user.LastName,
            profile.DisplayName,
            profile.PhoneNumber,
            profile.Country,
            profile.State,
            profile.City,
            profile.TimeZone,
            profile.PreferredLanguage,
            profile.ProfilePictureUrl,
            profile.Bio,
            profile.LinkedInUrl,
            profile.GitHubUrl,
            profile.PortfolioUrl,
            PreferencesDto.From(profile.Preferences));
    }
}

public sealed record PreferencesDto(
    ThemePreference Theme,
    DateFormatPreference DateFormat,
    TimeFormatPreference TimeFormat,
    bool EmailNotifications,
    bool InAppNotifications,
    bool MarketingEmails,
    bool WeeklySummaryEmails)
{
    public static PreferencesDto From(UserPreferences preferences) =>
        new(
            preferences.Theme,
            preferences.DateFormat,
            preferences.TimeFormat,
            preferences.EmailNotifications,
            preferences.InAppNotifications,
            preferences.MarketingEmails,
            preferences.WeeklySummaryEmails);
}
