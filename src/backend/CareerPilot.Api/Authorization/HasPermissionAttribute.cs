using Microsoft.AspNetCore.Authorization;

namespace CareerPilot.Api.Authorization;

/// <summary>
/// Requires a permission on an endpoint: <c>[HasPermission(Permissions.UsersRead)]</c>.
/// </summary>
/// <remarks>
/// Encodes the permission into the policy name, which
/// <see cref="PermissionPolicyProvider"/> decodes and turns into a
/// <see cref="PermissionRequirement"/>. The alternative — registering one named policy
/// per permission at startup — means every new permission needs a registration, and a
/// forgotten one throws at request time on an endpoint that looked protected.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    /// <summary>
    /// Separates the marker from the permission name. A colon cannot appear in a
    /// permission name, so the split is unambiguous.
    /// </summary>
    public const string PolicyPrefix = "Permission:";

    /// <remarks>
    /// The policy name is set through the base constructor because
    /// <see cref="AuthorizeAttribute.Policy"/> is not virtual and so cannot be
    /// computed by an override.
    /// </remarks>
    public HasPermissionAttribute(string permission)
        : base(PolicyPrefix + permission)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(permission);

        Permission = permission;
    }

    public string Permission { get; }
}
