using CareerPilot.Domain.Entities.Identity;
using CareerPilot.Domain.Enums;

namespace CareerPilot.Application.Abstractions.Authentication;

/// <summary>
/// A newly issued refresh token. <see cref="RawToken"/> exists only in this object —
/// the persisted record holds a hash — so it must be returned to the client and then
/// dropped, never logged and never stored.
/// </summary>
public sealed record IssuedRefreshToken(string RawToken, DateTime ExpiresAt, Guid TokenId);

/// <summary>
/// Outcome of presenting a refresh token.
/// </summary>
/// <remarks>
/// <see cref="ReuseDetected"/> is deliberately separate from a plain failure. A token
/// that exists but was already rotated means the chain has been compromised: the
/// legitimate client and an attacker both hold it, and there is no way to tell which
/// one is calling. The only safe response is to revoke the user's whole token family.
/// </remarks>
public enum RefreshTokenValidationStatus
{
    Valid = 0,
    NotFound = 1,
    Expired = 2,
    Revoked = 3,
    ReuseDetected = 4,
}

public sealed record RefreshTokenValidationResult(
    RefreshTokenValidationStatus Status,
    RefreshToken? Token)
{
    public bool IsValid => Status == RefreshTokenValidationStatus.Valid && Token is not null;
}

/// <summary>
/// Issues, validates, rotates and revokes refresh tokens.
/// </summary>
public interface IRefreshTokenService
{
    /// <summary>Mints a fresh token for a new session and persists its hash.</summary>
    Task<IssuedRefreshToken> IssueAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Looks the token up by hash and classifies it. Does not mutate state — the
    /// caller decides what to do with a <see cref="RefreshTokenValidationStatus.ReuseDetected"/>.
    /// </summary>
    Task<RefreshTokenValidationResult> ValidateAsync(string rawToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exchanges a valid token for a successor and links the two, so the predecessor
    /// becomes reuse-detectable rather than merely invalid.
    /// </summary>
    Task<IssuedRefreshToken> RotateAsync(RefreshToken current, CancellationToken cancellationToken = default);

    Task RevokeAsync(
        RefreshToken token,
        RefreshTokenRevocationReason reason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes every active token for the user — the response to sign-out-everywhere,
    /// a credential change, or detected reuse.
    /// </summary>
    Task RevokeAllForUserAsync(
        Guid userId,
        RefreshTokenRevocationReason reason,
        CancellationToken cancellationToken = default);
}
