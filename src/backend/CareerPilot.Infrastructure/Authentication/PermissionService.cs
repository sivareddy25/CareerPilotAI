using CareerPilot.Application.Abstractions.Authentication;
using CareerPilot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CareerPilot.Infrastructure.Authentication;

/// <summary>
/// Resolves effective roles and permissions by joining user → roles → permissions.
/// </summary>
/// <remarks>
/// Queries are <c>AsNoTracking</c> and project to strings: nothing here is mutated,
/// and materialising whole entity graphs into the change tracker for a read that
/// happens on every token issuance would be wasteful.
///
/// There is no caching layer. It would be the obvious optimisation, but a cached
/// permission set is a stale permission set, and stale authorization data fails in the
/// dangerous direction. If this becomes a bottleneck, cache with an explicit
/// invalidation on role change rather than a time-based expiry.
/// </remarks>
internal sealed class PermissionService(ApplicationDbContext context) : IPermissionService
{
    public async Task<IReadOnlyCollection<string>> GetRolesAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        await context.UserRoles
            .AsNoTracking()
            .Where(userRole => userRole.UserId == userId)
            .Select(userRole => userRole.Role!.Name)
            .Distinct()
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<string>> GetPermissionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        await context.UserRoles
            .AsNoTracking()
            .Where(userRole => userRole.UserId == userId)
            .SelectMany(userRole => userRole.Role!.RolePermissions)
            .Select(rolePermission => rolePermission.Permission!.Name)
            // Distinct because two roles granting the same permission is normal and
            // must not produce a duplicate claim.
            .Distinct()
            .ToListAsync(cancellationToken);

    public Task<bool> HasPermissionAsync(
        Guid userId,
        string permission,
        CancellationToken cancellationToken = default) =>
        context.UserRoles
            .AsNoTracking()
            .Where(userRole => userRole.UserId == userId)
            .SelectMany(userRole => userRole.Role!.RolePermissions)
            .AnyAsync(rolePermission => rolePermission.Permission!.Name == permission, cancellationToken);
}
