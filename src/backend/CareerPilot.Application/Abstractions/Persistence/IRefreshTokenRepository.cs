using CareerPilot.Domain.Entities.Identity;

namespace CareerPilot.Application.Abstractions.Persistence;

public interface IRefreshTokenRepository
{
    /// <summary>
    /// Finds a token by the hash of its raw value. Returns revoked and expired tokens
    /// too — the caller must be able to tell "never existed" from "already used",
    /// because only the second is evidence of a compromised chain.
    /// </summary>
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RefreshToken>> GetActiveForUserAsync(Guid userId, CancellationToken cancellationToken = default);

    void Add(RefreshToken refreshToken);
}
