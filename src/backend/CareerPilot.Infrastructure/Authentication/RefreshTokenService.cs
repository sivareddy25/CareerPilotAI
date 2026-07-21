using System.Security.Cryptography;
using System.Text;
using CareerPilot.Application.Abstractions.Authentication;
using CareerPilot.Application.Abstractions.Persistence;
using CareerPilot.Application.Authentication;
using CareerPilot.Domain.Entities.Identity;
using CareerPilot.Domain.Enums;
using CareerPilot.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace CareerPilot.Infrastructure.Authentication;

/// <summary>
/// Issues, validates, rotates and revokes refresh tokens.
/// </summary>
/// <remarks>
/// <para>
/// Tokens are 256 bits from a cryptographic RNG. That size is not arbitrary: the token
/// is a bearer credential with no other factor behind it, so guessability is the only
/// attack, and 256 bits puts it out of reach permanently.
/// </para>
/// <para>
/// Only the SHA-256 hash is stored. A database compromise therefore yields no usable
/// sessions — the same reasoning behind hashing passwords, except that a fast hash is
/// correct here because the input is already high-entropy and has no preimage worth
/// guessing.
/// </para>
/// <para>
/// This service does not call SaveChanges. Rotation and its accompanying access-token
/// issuance must land in one transaction, so the unit of work is committed by the
/// handler that owns the operation.
/// </para>
/// </remarks>
internal sealed class RefreshTokenService(
    IRefreshTokenRepository refreshTokens,
    ICurrentUserService currentUser,
    IOptions<JwtOptions> jwtOptions,
    IOptions<AuthenticationOptions> authenticationOptions)
    : IRefreshTokenService
{
    private const int TokenBytes = 32;

    private readonly JwtOptions _jwtOptions = jwtOptions.Value;
    private readonly AuthenticationOptions _authenticationOptions = authenticationOptions.Value;

    public Task<IssuedRefreshToken> IssueAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;
        var expiresAt = utcNow.AddDays(_jwtOptions.RefreshTokenDays);

        return Task.FromResult(Persist(userId, expiresAt, chainStartedAt: utcNow));
    }

    public async Task<RefreshTokenValidationResult> ValidateAsync(
        string rawToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
        {
            return new RefreshTokenValidationResult(RefreshTokenValidationStatus.NotFound, null);
        }

        var token = await refreshTokens.GetByTokenHashAsync(ComputeHash(rawToken), cancellationToken);

        if (token is null)
        {
            return new RefreshTokenValidationResult(RefreshTokenValidationStatus.NotFound, null);
        }

        var utcNow = DateTime.UtcNow;

        if (token.IsRevoked)
        {
            // A token revoked *by rotation* being presented again means someone still
            // holds a value that was already exchanged — the signature of a stolen or
            // replayed token. Other revocation reasons (sign-out, password change) are
            // expected and are not treated as an incident.
            var status = token.RevocationReason == RefreshTokenRevocationReason.ReplacedByRotation
                ? RefreshTokenValidationStatus.ReuseDetected
                : RefreshTokenValidationStatus.Revoked;

            return new RefreshTokenValidationResult(status, token);
        }

        if (token.IsExpired(utcNow))
        {
            return new RefreshTokenValidationResult(RefreshTokenValidationStatus.Expired, token);
        }

        return new RefreshTokenValidationResult(RefreshTokenValidationStatus.Valid, token);
    }

    public Task<IssuedRefreshToken> RotateAsync(
        RefreshToken current,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(current);

        var utcNow = DateTime.UtcNow;
        var successor = Persist(current.UserId, ResolveExpiry(current, utcNow), current.ChainStartedAt);

        // Marks the predecessor replaced rather than merely revoked, which is what
        // makes a later presentation of it detectable as reuse.
        current.MarkReplacedBy(successor.TokenId, utcNow, currentUser.IpAddress);

        return Task.FromResult(successor);
    }

    public Task RevokeAsync(
        RefreshToken token,
        RefreshTokenRevocationReason reason,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(token);

        token.Revoke(DateTime.UtcNow, reason, currentUser.IpAddress);

        return Task.CompletedTask;
    }

    public async Task RevokeAllForUserAsync(
        Guid userId,
        RefreshTokenRevocationReason reason,
        CancellationToken cancellationToken = default)
    {
        var active = await refreshTokens.GetActiveForUserAsync(userId, cancellationToken);
        var utcNow = DateTime.UtcNow;

        foreach (var token in active)
        {
            token.Revoke(utcNow, reason, currentUser.IpAddress);
        }
    }

    /// <summary>
    /// Sliding expiration, bounded. Each refresh pushes the window out, but never past
    /// the chain's absolute ceiling — otherwise an attacker holding a stolen token
    /// could keep the session alive indefinitely just by refreshing it.
    /// </summary>
    private DateTime ResolveExpiry(RefreshToken current, DateTime utcNow)
    {
        if (!_authenticationOptions.SlidingRefreshExpiration)
        {
            // Fixed window: the successor inherits the original expiry, so the session
            // ends on schedule no matter how active it is.
            return current.ExpiresAt;
        }

        var slidTo = utcNow.AddDays(_jwtOptions.RefreshTokenDays);
        var ceiling = current.ChainStartedAt.AddDays(_authenticationOptions.RefreshTokenAbsoluteLifetimeDays);

        return slidTo > ceiling ? ceiling : slidTo;
    }

    private IssuedRefreshToken Persist(Guid userId, DateTime expiresAt, DateTime chainStartedAt)
    {
        var rawToken = GenerateRawToken();

        var token = RefreshToken.Issue(
            userId,
            ComputeHash(rawToken),
            expiresAt,
            chainStartedAt,
            currentUser.IpAddress,
            Truncate(currentUser.UserAgent, 512));

        refreshTokens.Add(token);

        return new IssuedRefreshToken(rawToken, expiresAt, token.Id);
    }

    /// <summary>
    /// Base64url so the value is safe in headers, JSON and URLs without escaping.
    /// </summary>
    private static string GenerateRawToken() =>
        Base64UrlEncode(RandomNumberGenerator.GetBytes(TokenBytes));

    private static string ComputeHash(string rawToken) =>
        Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

    private static string? Truncate(string? value, int maxLength) =>
        value is null || value.Length <= maxLength ? value : value[..maxLength];
}
