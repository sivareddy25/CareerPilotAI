using CareerPilot.Application.Abstractions.Authentication;
using Microsoft.Extensions.Caching.Memory;

namespace CareerPilot.Infrastructure.Authentication;

/// <summary>
/// In-memory <see cref="ITokenBlacklist"/>, keyed by <c>jti</c>.
/// </summary>
/// <remarks>
/// <para>
/// Entries expire at the token's own expiry, so the list stays bounded by the number
/// of sign-outs within one access-token lifetime rather than growing without limit.
/// </para>
/// <para>
/// <b>Single-instance only.</b> Process memory is not shared, so behind more than one
/// replica a token blacklisted on one node stays usable on the others until it expires
/// naturally. The bound on that exposure is <c>Jwt:AccessTokenMinutes</c> — 15 by
/// default — and refresh revocation is unaffected because it is database-backed.
/// Before scaling out, reimplement this against the Redis connection that
/// <c>RedisOptions</c> already configures; the interface is deliberately async so that
/// swap needs no change above it.
/// </para>
/// </remarks>
internal sealed class TokenBlacklist(IMemoryCache cache) : ITokenBlacklist
{
    private const string KeyPrefix = "auth:blacklist:";

    public Task BlacklistAsync(string tokenId, DateTime expiresAt, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tokenId))
        {
            return Task.CompletedTask;
        }

        var lifetime = expiresAt - DateTime.UtcNow;

        // Already expired: signature validation rejects it, so there is nothing to
        // deny and no reason to hold a cache entry.
        if (lifetime <= TimeSpan.Zero)
        {
            return Task.CompletedTask;
        }

        cache.Set(KeyPrefix + tokenId, true, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = lifetime,
            // Pinned: eviction under memory pressure would silently un-revoke a token.
            // These entries are tiny and self-expiring, so keeping them is cheap.
            Priority = CacheItemPriority.NeverRemove,
        });

        return Task.CompletedTask;
    }

    public Task<bool> IsBlacklistedAsync(string tokenId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tokenId))
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(cache.TryGetValue(KeyPrefix + tokenId, out _));
    }
}
