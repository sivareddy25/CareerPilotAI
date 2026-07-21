namespace CareerPilot.Domain.Entities.Identity;

/// <summary>
/// Assignment of a <see cref="Role"/> to a <see cref="User"/>.
/// </summary>
/// <remarks>
/// Modelled as an entity with its own surrogate key rather than a composite-key join,
/// so it inherits the audit trail from <see cref="AuditableEntity"/>. Knowing *when*
/// and *by whom* a privilege was granted is the first question asked after an
/// incident, and a bare join table cannot answer it. Uniqueness of the pair is
/// enforced by a database index instead of by the key.
/// </remarks>
public sealed class UserRole : AuditableEntity
{
    private UserRole()
    {
    }

    private UserRole(Guid userId, Guid roleId)
    {
        UserId = userId;
        RoleId = roleId;
    }

    public Guid UserId { get; private set; }

    public Guid RoleId { get; private set; }

    public User? User { get; private set; }

    public Role? Role { get; private set; }

    public static UserRole Create(Guid userId, Guid roleId) => new(userId, roleId);
}
