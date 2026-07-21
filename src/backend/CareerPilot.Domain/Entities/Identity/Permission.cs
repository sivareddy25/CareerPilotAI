namespace CareerPilot.Domain.Entities.Identity;

/// <summary>
/// A single capability, named <c>resource.action</c> (for example <c>users.read</c>).
/// </summary>
/// <remarks>
/// Permissions are the unit endpoints actually authorize against. They are data rather
/// than an enum so that a deployment can add capabilities without a rebuild — the
/// constants in the Api layer are a compile-time convenience over these rows, not the
/// source of truth.
/// </remarks>
public sealed class Permission : AuditableEntity
{
    private readonly List<RolePermission> _rolePermissions = [];

    private Permission()
    {
        Name = string.Empty;
    }

    private Permission(string name, string? description, string? category)
    {
        Name = name;
        Description = description;
        Category = category;
    }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    /// <summary>Grouping label for administrative UIs. Carries no authorization meaning.</summary>
    public string? Category { get; private set; }

    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    public static Permission Create(string name, string? description = null, string? category = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Permission(name.Trim(), description, category);
    }
}
