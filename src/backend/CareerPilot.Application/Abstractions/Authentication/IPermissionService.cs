namespace CareerPilot.Application.Abstractions.Authentication;

/// <summary>
/// Resolves a user's effective roles and permissions by walking
/// user → roles → permissions.
/// </summary>
public interface IPermissionService
{
    Task<IReadOnlyCollection<string>> GetRolesAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Distinct permission names granted through every role the user holds.
    /// </summary>
    Task<IReadOnlyCollection<string>> GetPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Authoritative check against current database state, for the cases where the
    /// claims baked into a token may be stale.
    /// </summary>
    Task<bool> HasPermissionAsync(Guid userId, string permission, CancellationToken cancellationToken = default);
}
