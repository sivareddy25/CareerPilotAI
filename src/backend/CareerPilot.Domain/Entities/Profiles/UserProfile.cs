using CareerPilot.Domain.Entities.Identity;

namespace CareerPilot.Domain.Entities.Profiles;

/// <summary>
/// A user's profile: everything about who they are, as opposed to how they authenticate.
/// </summary>
/// <remarks>
/// <para>
/// One profile per <see cref="User"/>, created alongside the account so that reads
/// never have to cope with a missing profile.
/// </para>
/// <para>
/// Deliberately does <b>not</b> hold first name, last name, or email. Those live on
/// <see cref="User"/> and stay there: name appears in the JWT and in
/// <c>/auth/me</c>, and email is the login identifier and the uniqueness key. Copying
/// them here would create a second source of truth that drifts the first time one side
/// is updated without the other. The profile API still exposes and edits them — it
/// reaches through to the <see cref="User"/> in the same transaction.
/// </para>
/// </remarks>
public sealed class UserProfile : SoftDeleteEntity
{
    private UserProfile()
    {
        Preferences = UserPreferences.CreateDefault();
    }

    private UserProfile(Guid userId)
    {
        UserId = userId;
        Preferences = UserPreferences.CreateDefault();
    }

    public Guid UserId { get; private set; }

    /// <summary>
    /// Name shown to others. Optional — the client falls back to the user's real name
    /// when unset, so an empty display name is a valid state rather than a defect.
    /// </summary>
    public string? DisplayName { get; private set; }

    /// <summary>E.164-ish, stored as supplied. Never used as an identifier or a factor.</summary>
    public string? PhoneNumber { get; private set; }

    /// <summary>ISO 3166-1 alpha-2, upper case. Validated against the runtime's region list.</summary>
    public string? Country { get; private set; }

    public string? State { get; private set; }

    public string? City { get; private set; }

    /// <summary>IANA or Windows time zone id, validated against the host's database.</summary>
    public string? TimeZone { get; private set; }

    /// <summary>BCP 47 language tag, for example <c>en-GB</c>.</summary>
    public string? PreferredLanguage { get; private set; }

    /// <summary>
    /// Location of the stored avatar, produced by the file storage abstraction. Held as
    /// a string rather than a local path so the value stays meaningful when storage
    /// moves to a cloud provider.
    /// </summary>
    public string? ProfilePictureUrl { get; private set; }

    public string? Bio { get; private set; }

    public string? LinkedInUrl { get; private set; }

    public string? GitHubUrl { get; private set; }

    public string? PortfolioUrl { get; private set; }

    public UserPreferences Preferences { get; private set; }

    public User? User { get; private set; }

    public static UserProfile CreateFor(Guid userId) => new(userId);

    /// <summary>
    /// Replaces the editable profile fields wholesale.
    /// </summary>
    /// <remarks>
    /// Full replacement rather than patch semantics: the edit form submits every field,
    /// so a null means "cleared", not "unchanged". Treating null as "leave alone" would
    /// make it impossible to remove a phone number or a link once set.
    /// </remarks>
    public void UpdateDetails(
        string? displayName,
        string? phoneNumber,
        string? country,
        string? state,
        string? city,
        string? timeZone,
        string? preferredLanguage,
        string? bio,
        string? linkedInUrl,
        string? gitHubUrl,
        string? portfolioUrl)
    {
        DisplayName = Normalize(displayName);
        PhoneNumber = Normalize(phoneNumber);
        // Upper-cased so a stored country always compares equal regardless of how it
        // was typed; the validator accepts either case.
        Country = Normalize(country)?.ToUpperInvariant();
        State = Normalize(state);
        City = Normalize(city);
        TimeZone = Normalize(timeZone);
        PreferredLanguage = Normalize(preferredLanguage);
        Bio = Normalize(bio);
        LinkedInUrl = Normalize(linkedInUrl);
        GitHubUrl = Normalize(gitHubUrl);
        PortfolioUrl = Normalize(portfolioUrl);
    }

    public void SetProfilePicture(string url)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        ProfilePictureUrl = url;
    }

    /// <summary>
    /// Clears the avatar reference and hands back the previous value so the caller can
    /// delete the underlying file.
    /// </summary>
    /// <remarks>
    /// The entity does not delete the file itself — it has no business knowing about
    /// storage. Returning the old location keeps the two steps ordered correctly:
    /// the row is updated first, and the blob is removed only once that commits.
    /// </remarks>
    public string? ClearProfilePicture()
    {
        var previous = ProfilePictureUrl;
        ProfilePictureUrl = null;

        return previous;
    }

    /// <summary>
    /// Whitespace-only input becomes null, so "empty" has exactly one representation in
    /// the database and queries never have to test for both.
    /// </summary>
    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
