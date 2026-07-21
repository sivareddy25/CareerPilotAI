namespace CareerPilot.Domain.Entities.Identity;

/// <summary>
/// A named bundle of permissions. Roles are what get assigned to users; permissions
/// are what get checked at the endpoint. Keeping the two separate means access rules
/// can be re-cut without touching either user assignments or endpoint attributes.
/// </summary>
public sealed class Role : AuditableEntity
{
    private readonly List<RolePermission> _rolePermissions = [];
    private readonly List<UserRole> _userRoles = [];

    private Role()
    {
        Name = string.Empty;
        NormalizedName = string.Empty;
    }

    private Role(string name, string? description, bool isSystemRole)
    {
        Name = name;
        NormalizedName = Normalize(name);
        Description = description;
        IsSystemRole = isSystemRole;
    }

    public string Name { get; private set; }

    public string NormalizedName { get; private set; }

    public string? Description { get; private set; }

    /// <summary>
    /// Seeded roles the application itself depends on. Flagged so administrative
    /// tooling can refuse to delete or rename them out from under the code.
    /// </summary>
    public bool IsSystemRole { get; private set; }

    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    public static Role Create(string name, string? description = null, bool isSystemRole = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Role(name.Trim(), description, isSystemRole);
    }

    public static string Normalize(string name) => name.Trim().ToUpperInvariant();

    public void Describe(string? description) => Description = description;
}
