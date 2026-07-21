using CareerPilot.Application.Abstractions.Authentication;
using CareerPilot.Application.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace CareerPilot.Api.Authorization;

/// <summary>
/// Decides a <see cref="PermissionRequirement"/>.
/// </summary>
/// <remarks>
/// <para>
/// Two-stage by design. The token's permission claims are checked first and settle the
/// overwhelming majority of requests with no I/O. Only when the claim is absent does it
/// fall back to the database.
/// </para>
/// <para>
/// The fallback exists so a permission granted *after* a token was issued takes effect
/// immediately, instead of leaving the user unable to use it until their token expires.
/// The reverse case — a permission revoked after issuance — is not covered here: the
/// claim is still present, so the fast path allows it, for up to the remaining lifetime
/// of the access token. Closing that would mean a database read on every authorized
/// request. The exposure is bounded by <c>Jwt:AccessTokenMinutes</c>, and refresh
/// re-reads roles and permissions from scratch. For a privilege where even that window
/// is unacceptable, check <see cref="IPermissionService"/> directly in the handler
/// rather than relying on the claim.
/// </para>
/// </remarks>
internal sealed class PermissionAuthorizationHandler(
    ICurrentUserService currentUser,
    IPermissionService permissionService)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var hasClaim = context.User
            .FindAll(AuthenticationClaimTypes.Permission)
            .Any(claim => string.Equals(claim.Value, requirement.Permission, StringComparison.Ordinal));

        if (hasClaim)
        {
            context.Succeed(requirement);
            return;
        }

        if (currentUser.UserId is not { } userId)
        {
            return;
        }

        if (await permissionService.HasPermissionAsync(userId, requirement.Permission))
        {
            context.Succeed(requirement);
        }

        // No explicit Fail(): leaving the requirement unmet denies it, whereas Fail()
        // would veto the whole policy even if another handler could satisfy it.
    }
}
