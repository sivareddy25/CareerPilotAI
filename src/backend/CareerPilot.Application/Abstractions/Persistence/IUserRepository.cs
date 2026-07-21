using CareerPilot.Domain.Entities.Identity;

namespace CareerPilot.Application.Abstractions.Persistence;

/// <summary>
/// Reads and writes user aggregates. Deliberately narrow: it exposes the handful of
/// lookups authentication actually performs rather than a generic queryable, which
/// keeps EF Core out of the Application layer entirely.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Looks a user up by normalized email. <paramref name="normalizedEmail"/> must be
    /// the output of <see cref="User.NormalizeEmail"/>.
    /// </summary>
    Task<User?> GetByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);

    Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<bool> ExistsByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);

    void Add(User user);
}
