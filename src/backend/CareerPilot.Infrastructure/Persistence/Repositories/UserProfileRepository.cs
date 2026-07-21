using CareerPilot.Application.Abstractions.Persistence;
using CareerPilot.Domain.Entities.Profiles;
using Microsoft.EntityFrameworkCore;

namespace CareerPilot.Infrastructure.Persistence.Repositories;

internal sealed class UserProfileRepository(ApplicationDbContext context) : IUserProfileRepository
{
    /// <remarks>
    /// Tracked and with the user included. Tracked because every caller mutates what it
    /// loads; included because name and email live on the user and the DTO cannot be
    /// built without them — a lazy or separate load would turn one query into two on
    /// the most frequently hit endpoint in the module.
    /// </remarks>
    public Task<UserProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        context.UserProfiles
            .Include(profile => profile.User)
            .FirstOrDefaultAsync(profile => profile.UserId == userId, cancellationToken);

    public void Add(UserProfile profile) => context.UserProfiles.Add(profile);
}
