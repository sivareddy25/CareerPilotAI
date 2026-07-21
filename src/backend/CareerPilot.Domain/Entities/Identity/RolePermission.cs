namespace CareerPilot.Domain.Entities.Identity;

/// <summary>
/// Grant of a <see cref="Permission"/> to a <see cref="Role"/>.
/// </summary>
/// <remarks>
/// Auditable for the same reason as <see cref="UserRole"/>: a widened role is a
/// privilege escalation for every user holding it, and that change needs a timestamp
/// and an actor. The pair is kept unique by index.
/// </remarks>
public sealed class RolePermission : AuditableEntity
{
    private RolePermission()
    {
    }

    private RolePermission(Guid roleId, Guid permissionId)
    {
        RoleId = roleId;
        PermissionId = permissionId;
    }

    public Guid RoleId { get; private set; }

    public Guid PermissionId { get; private set; }

    public Role? Role { get; private set; }

    public Permission? Permission { get; private set; }

    public static RolePermission Create(Guid roleId, Guid permissionId) => new(roleId, permissionId);
}
