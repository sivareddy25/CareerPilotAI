using CareerPilot.Domain.Entities.Profiles;

namespace CareerPilot.Application.Abstractions.Persistence;

public interface IUserProfileRepository
{
    /// <summary>
    /// Loads a profile with its owning <see cref="Domain.Entities.Identity.User"/>.
    /// </summary>
    /// <remarks>
    /// The user is included because first name, last name and email live there — every
    /// profile read needs them, so fetching them separately would guarantee a second
    /// round-trip on the hot path.
    /// </remarks>
    Task<UserProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    void Add(UserProfile profile);
}
