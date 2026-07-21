using CareerPilot.Domain.Entities;

namespace CareerPilot.Domain.Entities.Identity;

/// <summary>
/// A local-authentication user account.
/// </summary>
/// <remarks>
/// Setters are private and state changes go through intention-revealing methods, so
/// invariants that matter for security — a password hash is never null, a lockout
/// counter only resets deliberately — cannot be broken by an assignment somewhere in
/// the application layer. EF Core materialises private setters directly.
/// </remarks>
public sealed class User : SoftDeleteEntity
{
    private readonly List<UserRole> _userRoles = [];
    private readonly List<RefreshToken> _refreshTokens = [];

    // Required by EF Core's materialiser. Not for application use.
    private User()
    {
        Email = string.Empty;
        NormalizedEmail = string.Empty;
        PasswordHash = string.Empty;
        SecurityStamp = string.Empty;
    }

    private User(string email, string passwordHash, string? firstName, string? lastName)
    {
        Email = email;
        NormalizedEmail = NormalizeEmail(email);
        PasswordHash = passwordHash;
        FirstName = firstName;
        LastName = lastName;
        SecurityStamp = Guid.NewGuid().ToString("N");
        IsActive = true;
    }

    /// <summary>Address as the user typed it — preserved for display and correspondence.</summary>
    public string Email { get; private set; }

    /// <summary>
    /// Upper-invariant form of <see cref="Email"/>. Uniqueness and lookups run against
    /// this column so that <c>Ada@x.com</c> and <c>ada@x.com</c> cannot both register,
    /// and so lookups never depend on database collation.
    /// </summary>
    public string NormalizedEmail { get; private set; }

    /// <summary>BCrypt hash. The plaintext password never leaves the request that carried it.</summary>
    public string PasswordHash { get; private set; }

    public string? FirstName { get; private set; }

    public string? LastName { get; private set; }

    public bool EmailConfirmed { get; private set; }

    /// <summary>
    /// Administrative enable/disable, independent of soft deletion. A deactivated user
    /// keeps their data and history; a deleted one is hidden by the global query filter.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Regenerated whenever credentials or authorization-relevant state change. Access
    /// tokens carry the stamp they were minted with, so a mismatch at refresh time
    /// invalidates every token issued before the change.
    /// </summary>
    public string SecurityStamp { get; private set; }

    /// <summary>Consecutive failed sign-in attempts since the last success.</summary>
    public int AccessFailedCount { get; private set; }

    /// <summary>When set and in the future, authentication is refused regardless of password.</summary>
    public DateTime? LockoutEndsAt { get; private set; }

    public DateTime? LastLoginAt { get; private set; }

    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    public static User Create(string email, string passwordHash, string? firstName = null, string? lastName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        return new User(email.Trim(), passwordHash, firstName?.Trim(), lastName?.Trim());
    }

    public static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();

    /// <summary>
    /// True when the account may currently authenticate. Checked before password
    /// verification is even attempted for disabled accounts, and after it for locked
    /// ones — see the login handler for why the ordering matters.
    /// </summary>
    public bool IsLockedOut(DateTime utcNow) => LockoutEndsAt is not null && LockoutEndsAt > utcNow;

    public void SetPasswordHash(string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        PasswordHash = passwordHash;

        // Rotating the stamp is what actually severs existing sessions; without it a
        // stolen token would outlive the password change that was meant to stop it.
        RegenerateSecurityStamp();
        AccessFailedCount = 0;
        LockoutEndsAt = null;
    }

    /// <summary>
    /// Replaces the stored hash with an equivalent one at a stronger cost factor.
    /// </summary>
    /// <remarks>
    /// Distinct from <see cref="SetPasswordHash"/> because the password itself has not
    /// changed. Rotating the security stamp here would sign the user out during a
    /// successful login, which is exactly when a transparent upgrade happens.
    /// </remarks>
    public void UpgradePasswordHash(string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        PasswordHash = passwordHash;
    }

    public void RegenerateSecurityStamp() => SecurityStamp = Guid.NewGuid().ToString("N");

    public void RecordSuccessfulLogin(DateTime utcNow)
    {
        AccessFailedCount = 0;
        LockoutEndsAt = null;
        LastLoginAt = utcNow;
    }

    /// <summary>
    /// Counts a failed attempt and locks the account once the threshold is reached.
    /// Thresholds are policy, so they are passed in rather than baked into the entity.
    /// </summary>
    public void RecordFailedLogin(DateTime utcNow, int maxAttempts, TimeSpan lockoutDuration)
    {
        AccessFailedCount++;

        if (maxAttempts > 0 && AccessFailedCount >= maxAttempts)
        {
            LockoutEndsAt = utcNow.Add(lockoutDuration);
        }
    }

    /// <summary>
    /// Grants a role. Idempotent, so a repeated call cannot produce the duplicate row
    /// that the database's unique index would reject.
    /// </summary>
    public UserRole AssignRole(Guid roleId)
    {
        var existing = _userRoles.Find(userRole => userRole.RoleId == roleId);
        if (existing is not null)
        {
            return existing;
        }

        var assignment = UserRole.Create(Id, roleId);
        _userRoles.Add(assignment);

        return assignment;
    }

    public void ConfirmEmail() => EmailConfirmed = true;

    public void Activate() => IsActive = true;

    public void Deactivate()
    {
        IsActive = false;
        RegenerateSecurityStamp();
    }
}
