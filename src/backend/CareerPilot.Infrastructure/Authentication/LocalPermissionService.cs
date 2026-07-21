using CareerPilot.Application.Abstractions.Authentication;

namespace CareerPilot.Infrastructure.Authentication;

/// <summary>
/// Grants every permission unconditionally, for Local mode.
/// </summary>
/// <remarks>
/// Local mode has exactly one operator, who owns the machine and the database. There is
/// no privilege boundary to enforce: a user who could be denied a permission here could
/// equally edit the SQLite file directly. Resolving permissions against role tables would
/// therefore add queries and a seeding requirement to reach a foregone conclusion.
///
/// This is deliberately *not* a fallback for SaaS mode. <see cref="PermissionService"/>
/// remains the real implementation; the registration in
/// <see cref="AuthenticationRegistration"/> selects between them by hosting mode.
/// </remarks>
internal sealed class LocalPermissionService : IPermissionService
{
    private static readonly IReadOnlyCollection<string> LocalRoles = ["Admin", "User"];

    public Task<IReadOnlyCollection<string>> GetRolesAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(LocalRoles);

    public Task<IReadOnlyCollection<string>> GetPermissionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<string>>([]);

    public Task<bool> HasPermissionAsync(
        Guid userId,
        string permission,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(true);
}
