using CareerPilot.Application.Abstractions.Persistence;
using CareerPilot.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace CareerPilot.Infrastructure.Persistence.Repositories;

internal sealed class RoleRepository(ApplicationDbContext context) : IRoleRepository
{
    public Task<Role?> GetByNormalizedNameAsync(
        string normalizedName,
        CancellationToken cancellationToken = default) =>
        context.Roles
            .FirstOrDefaultAsync(role => role.NormalizedName == normalizedName, cancellationToken);
}
