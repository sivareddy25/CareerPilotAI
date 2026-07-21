using CareerPilot.Domain.Enums;

namespace CareerPilot.Domain.Entities.Identity;

/// <summary>
/// One issued refresh token, stored as a hash.
/// </summary>
/// <remarks>
/// The raw token is returned to the caller once and never persisted: a database leak
/// therefore yields no usable credentials. SHA-256 is the right hash here — unlike a
/// password, the token is 256 bits of cryptographic randomness, so it has no
/// guessable preimage and a deliberately slow hash would only cost latency.
///
/// Tokens form a chain. Each rotation links the predecessor to its successor via
/// <see cref="ReplacedByTokenId"/>, which is what makes reuse detection possible:
/// presenting an already-rotated token proves either replay or theft.
/// </remarks>
public sealed class RefreshToken : AuditableEntity
{
    private RefreshToken()
    {
        TokenHash = string.Empty;
    }

    private RefreshToken(
        Guid userId,
        string tokenHash,
        DateTime expiresAt,
        DateTime chainStartedAt,
        string? createdByIp,
        string? createdByUserAgent)
    {
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        ChainStartedAt = chainStartedAt;
        CreatedByIp = createdByIp;
        CreatedByUserAgent = createdByUserAgent;
    }

    public Guid UserId { get; private set; }

    /// <summary>Base64 SHA-256 of the raw token. Unique — a collision would be a cross-account leak.</summary>
    public string TokenHash { get; private set; }

    public DateTime ExpiresAt { get; private set; }

    /// <summary>
    /// When the *first* token in this rotation chain was issued, carried forward
    /// unchanged through every rotation.
    /// </summary>
    /// <remarks>
    /// This is what makes sliding expiration safe. Sliding alone lets a session renew
    /// forever, so a refresh token stolen from an active user never expires. Anchoring
    /// to the chain's origin gives an absolute ceiling that no amount of activity can
    /// push out.
    /// </remarks>
    public DateTime ChainStartedAt { get; private set; }

    public DateTime? RevokedAt { get; private set; }

    public RefreshTokenRevocationReason RevocationReason { get; private set; }

    /// <summary>The token issued in this one's place, when rotated.</summary>
    public Guid? ReplacedByTokenId { get; private set; }

    /// <summary>Origin of issuance. Forensic context only — never used to make an auth decision.</summary>
    public string? CreatedByIp { get; private set; }

    public string? CreatedByUserAgent { get; private set; }

    public string? RevokedByIp { get; private set; }

    public User? User { get; private set; }

    public bool IsExpired(DateTime utcNow) => utcNow >= ExpiresAt;

    public bool IsRevoked => RevokedAt is not null;

    /// <summary>Usable only while neither revoked nor expired.</summary>
    public bool IsActive(DateTime utcNow) => !IsRevoked && !IsExpired(utcNow);

    public static RefreshToken Issue(
        Guid userId,
        string tokenHash,
        DateTime expiresAt,
        DateTime chainStartedAt,
        string? createdByIp = null,
        string? createdByUserAgent = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);

        return new RefreshToken(userId, tokenHash, expiresAt, chainStartedAt, createdByIp, createdByUserAgent);
    }

    public void Revoke(DateTime utcNow, RefreshTokenRevocationReason reason, string? revokedByIp = null)
    {
        // Idempotent: the first revocation wins, so a reuse-detection sweep cannot
        // overwrite the reason that originally explains why a token was cut.
        if (IsRevoked)
        {
            return;
        }

        RevokedAt = utcNow;
        RevocationReason = reason;
        RevokedByIp = revokedByIp;
    }

    public void MarkReplacedBy(Guid successorTokenId, DateTime utcNow, string? revokedByIp = null)
    {
        Revoke(utcNow, RefreshTokenRevocationReason.ReplacedByRotation, revokedByIp);
        ReplacedByTokenId = successorTokenId;
    }
}
