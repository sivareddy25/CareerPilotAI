using CareerPilot.Application.Abstractions.Persistence;
using CareerPilot.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace CareerPilot.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository(ApplicationDbContext context) : IUserRepository
{
    /// <remarks>
    /// Tracked, not <c>AsNoTracking</c>: every caller of this method goes on to mutate
    /// the user — failed-attempt counters, last-login stamps, password hashes — and
    /// relies on the unit of work to persist it.
    /// </remarks>
    public Task<User?> GetByNormalizedEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken = default) =>
        context.Users
            .FirstOrDefaultAsync(user => user.NormalizedEmail == normalizedEmail, cancellationToken);

    public Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        context.Users.FirstOrDefaultAsync(user => user.Id == userId, cancellationToken);

    public Task<bool> ExistsByNormalizedEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken = default) =>
        context.Users.AnyAsync(user => user.NormalizedEmail == normalizedEmail, cancellationToken);

    public void Add(User user) => context.Users.Add(user);
}
