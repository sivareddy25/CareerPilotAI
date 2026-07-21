using CareerPilot.Domain.Entities.Identity;

namespace CareerPilot.Application.Abstractions.Persistence;

public interface IRoleRepository
{
    /// <summary>
    /// <paramref name="normalizedName"/> must be the output of <see cref="Role.Normalize"/>.
    /// </summary>
    Task<Role?> GetByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken = default);
}
