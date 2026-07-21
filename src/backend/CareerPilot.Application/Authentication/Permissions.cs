namespace CareerPilot.Application.Authentication;

/// <summary>
/// Compile-time names for the permissions this phase defines.
/// </summary>
/// <remarks>
/// The database rows are the source of truth; these constants exist so endpoints and
/// the seeder reference the same literals and a typo becomes a build error rather than
/// a policy that silently never matches.
///
/// Scope is intentionally limited to identity administration. Permissions for other
/// areas belong to the phases that introduce them — declaring them here would create
/// grantable privileges that guard nothing.
/// </remarks>
public static class Permissions
{
    public const string Category = "Identity";

    public const string UsersRead = "users.read";
    public const string UsersWrite = "users.write";
    public const string UsersDelete = "users.delete";

    public const string RolesRead = "roles.read";
    public const string RolesWrite = "roles.write";

    public const string PermissionsRead = "permissions.read";

    public static IReadOnlyList<string> All { get; } =
    [
        UsersRead,
        UsersWrite,
        UsersDelete,
        RolesRead,
        RolesWrite,
        PermissionsRead,
    ];
}

/// <summary>
/// Names of the roles the seeder creates and marks as system roles.
/// </summary>
public static class SystemRoles
{
    /// <summary>Holds every permission. Assigned manually, never by registration.</summary>
    public const string Administrator = "Administrator";

    /// <summary>
    /// Default for self-registration. Deliberately granted no permissions — a new
    /// account should start with an authenticated identity and nothing else, and
    /// privileges added here would be granted to every sign-up automatically.
    /// </summary>
    public const string User = "User";
}
