namespace CareerPilot.Application.Authentication;

/// <summary>
/// Tunable authentication policy: password strength, lockout thresholds, and role
/// defaults.
/// </summary>
/// <remarks>
/// These live in the Application layer, not Infrastructure, because the validators
/// that enforce them do. They are configuration rather than constants so a deployment
/// can tighten policy without a rebuild — but note that raising
/// <see cref="MinimumPasswordLength"/> only affects new and changed passwords;
/// existing hashes are unaffected and are not retroactively invalidated.
/// </remarks>
public sealed class AuthenticationOptions
{
    public const string SectionName = "Authentication";

    /// <summary>
    /// NIST SP 800-63B puts length ahead of composition rules; 12 is the floor here,
    /// with character-class requirements kept mild so users are not pushed toward
    /// predictable substitutions.
    /// </summary>
    public int MinimumPasswordLength { get; set; } = 12;

    /// <summary>
    /// Upper bound exists to cap BCrypt's work, not to constrain the user. Note that
    /// BCrypt itself silently truncates input beyond 72 bytes, which is why the
    /// validator rejects anything longer rather than letting it pass unnoticed.
    /// </summary>
    public int MaximumPasswordLength { get; set; } = 72;

    public bool RequireUppercase { get; set; } = true;

    public bool RequireLowercase { get; set; } = true;

    public bool RequireDigit { get; set; } = true;

    public bool RequireNonAlphanumeric { get; set; } = true;

    /// <summary>Failed attempts before lockout. Zero disables lockout entirely.</summary>
    public int MaxFailedAccessAttempts { get; set; } = 5;

    public int LockoutMinutes { get; set; } = 15;

    /// <summary>BCrypt cost factor. Each increment doubles hashing time.</summary>
    public int PasswordHashWorkFactor { get; set; } = 12;

    /// <summary>Role granted to every self-registered account.</summary>
    public string DefaultRole { get; set; } = "User";

    /// <summary>
    /// When true, a refresh extends the session window instead of keeping the original
    /// absolute expiry — convenient for active users, at the cost of sessions that can
    /// live indefinitely. <see cref="RefreshTokenAbsoluteLifetimeDays"/> is the backstop.
    /// </summary>
    public bool SlidingRefreshExpiration { get; set; } = true;

    /// <summary>
    /// Hard ceiling on a session's age regardless of activity. Without this, sliding
    /// expiration means a stolen refresh token is valid forever.
    /// </summary>
    public int RefreshTokenAbsoluteLifetimeDays { get; set; } = 30;
}
