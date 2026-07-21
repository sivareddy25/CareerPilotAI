namespace CareerPilot.Application.Abstractions.Authentication;

/// <summary>
/// Denies specific access tokens before their natural expiry, by <c>jti</c>.
/// </summary>
/// <remarks>
/// JWTs are self-contained, so signing out cannot un-issue one that is already in the
/// wild. This list closes the gap between sign-out and expiry. Entries only need to
/// outlive the token itself, so an implementation should evict at the token's own
/// expiry rather than accumulate forever.
/// </remarks>
public interface ITokenBlacklist
{
    /// <summary>
    /// Blocks <paramref name="tokenId"/> until <paramref name="expiresAt"/>, after
    /// which normal expiry validation rejects it anyway.
    /// </summary>
    Task BlacklistAsync(string tokenId, DateTime expiresAt, CancellationToken cancellationToken = default);

    Task<bool> IsBlacklistedAsync(string tokenId, CancellationToken cancellationToken = default);
}
