using CareerPilot.Application.Authentication;
using CareerPilot.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CareerPilot.Infrastructure.Persistence.Seeding;

/// <summary>
/// Ensures the roles and permissions the application depends on exist.
/// </summary>
/// <remarks>
/// <para>
/// Idempotent and additive: it inserts what is missing and leaves everything else
/// alone. It never deletes a role, revokes a grant, or overwrites a description, so
/// running it against a database an administrator has customised cannot undo their
/// changes — and it is safe to run on every start.
/// </para>
/// <para>
/// It also creates no users. A seeded administrator account with a known or generated
/// password is a standing backdoor, and one that tends to survive into production; the
/// first administrator should be promoted deliberately from a registered account.
/// </para>
/// </remarks>
public sealed class IdentitySeeder(ApplicationDbContext context, ILogger<IdentitySeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var permissionsByName = await EnsurePermissionsAsync(cancellationToken);
        await EnsureRolesAsync(permissionsByName, cancellationToken);
    }

    private async Task<Dictionary<string, Permission>> EnsurePermissionsAsync(CancellationToken cancellationToken)
    {
        var existing = await context.Permissions.ToDictionaryAsync(
            permission => permission.Name,
            cancellationToken);

        foreach (var name in Permissions.All.Where(name => !existing.ContainsKey(name)))
        {
            var permission = Permission.Create(name, category: Permissions.Category);

            context.Permissions.Add(permission);
            existing[name] = permission;

            logger.LogInformation("Seeded permission {Permission}", name);
        }

        await context.SaveChangesAsync(cancellationToken);

        return existing;
    }

    private async Task EnsureRolesAsync(
        Dictionary<string, Permission> permissionsByName,
        CancellationToken cancellationToken)
    {
        // Administrator gets every permission this phase defines; a later phase that
        // adds permissions will grant them here on its next run.
        await EnsureRoleAsync(
            SystemRoles.Administrator,
            "Full administrative access.",
            Permissions.All,
            permissionsByName,
            cancellationToken);

        // No grants — see SystemRoles.User.
        await EnsureRoleAsync(
            SystemRoles.User,
            "Default role for registered accounts.",
            [],
            permissionsByName,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureRoleAsync(
        string name,
        string description,
        IReadOnlyCollection<string> permissionNames,
        Dictionary<string, Permission> permissionsByName,
        CancellationToken cancellationToken)
    {
        var normalized = Role.Normalize(name);

        var role = await context.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.NormalizedName == normalized, cancellationToken);

        if (role is null)
        {
            role = Role.Create(name, description, isSystemRole: true);
            context.Roles.Add(role);

            logger.LogInformation("Seeded system role {Role}", name);
        }

        var granted = role.RolePermissions
            .Select(rolePermission => rolePermission.PermissionId)
            .ToHashSet();

        foreach (var permissionName in permissionNames)
        {
            if (!permissionsByName.TryGetValue(permissionName, out var permission))
            {
                // Cannot happen after EnsurePermissionsAsync, but a missing grant is a
                // silent authorization hole, so it is logged rather than ignored.
                logger.LogWarning(
                    "Cannot grant {Permission} to {Role}: permission not found",
                    permissionName,
                    name);

                continue;
            }

            if (granted.Contains(permission.Id))
            {
                continue;
            }

            context.RolePermissions.Add(RolePermission.Create(role.Id, permission.Id));

            logger.LogInformation("Granted {Permission} to {Role}", permissionName, name);
        }
    }
}
