namespace CareerPilot.Domain.Enums;

/// <summary>
/// Why a refresh token stopped being usable. Recorded rather than inferred:
/// <see cref="ReplacedByRotation"/> is routine, while <see cref="ReuseDetected"/>
/// is a breach signal and must stay distinguishable in the audit trail.
/// </summary>
public enum RefreshTokenRevocationReason
{
    None = 0,

    /// <summary>Normal rotation — the token was exchanged for a successor.</summary>
    ReplacedByRotation = 1,

    /// <summary>The user signed out explicitly.</summary>
    SignedOut = 2,

    /// <summary>Password changed or reset; the whole session family is cut.</summary>
    CredentialsChanged = 3,

    /// <summary>
    /// An already-rotated token was presented again. Either a replay or a stolen
    /// token; the entire descendant chain is revoked as a result.
    /// </summary>
    ReuseDetected = 4,

    /// <summary>Revoked by an administrator or by account deactivation.</summary>
    RevokedByAdministrator = 5,
}
