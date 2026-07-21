using CareerPilot.Application.Abstractions.Authentication;
using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Abstractions.Persistence;
using CareerPilot.Application.Authentication.Models;
using CareerPilot.Application.Exceptions;

namespace CareerPilot.Application.Authentication.Queries.GetCurrentUser;

/// <summary>
/// Reads the caller's profile from the database rather than reflecting their token
/// back at them.
/// </summary>
/// <remarks>
/// Roles and permissions are re-resolved on every call. A token issued fifteen minutes
/// ago may name privileges that have since been withdrawn, and this endpoint is what
/// clients drive their UI from — showing an administrator's menu to someone whose role
/// was just revoked would be misleading, even though the API itself would still refuse
/// the underlying calls.
/// </remarks>
internal sealed class GetCurrentUserQueryHandler(
    ICurrentUserService currentUser,
    IUserRepository users,
    IPermissionService permissionService)
    : IQueryHandler<GetCurrentUserQuery, UserProfile>
{
    public async Task<UserProfile> Handle(GetCurrentUserQuery query, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            throw new InvalidCredentialsException();
        }

        var user = await users.GetByIdAsync(userId, cancellationToken)
            ?? throw new InvalidCredentialsException();

        var roles = await permissionService.GetRolesAsync(userId, cancellationToken);
        var permissions = await permissionService.GetPermissionsAsync(userId, cancellationToken);

        return UserProfile.From(user, roles, permissions);
    }
}
