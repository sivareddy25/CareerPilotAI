using CareerPilot.Application.Abstractions.Persistence;
using CareerPilot.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace CareerPilot.Infrastructure.Persistence.Repositories;

internal sealed class RefreshTokenRepository(ApplicationDbContext context) : IRefreshTokenRepository
{
    /// <remarks>
    /// Returns revoked and expired tokens deliberately — see
    /// <see cref="IRefreshTokenRepository.GetByTokenHashAsync"/>. Filtering them out
    /// here would make replay of a rotated token indistinguishable from a token that
    /// never existed, and reuse detection would stop working.
    /// </remarks>
    public Task<RefreshToken?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default) =>
        context.RefreshTokens
            .FirstOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

    public async Task<IReadOnlyList<RefreshToken>> GetActiveForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        await context.RefreshTokens
            .Where(token => token.UserId == userId && token.RevokedAt == null)
            .ToListAsync(cancellationToken);

    public void Add(RefreshToken refreshToken) => context.RefreshTokens.Add(refreshToken);
}
