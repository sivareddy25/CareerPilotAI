using CareerPilot.Application.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace CareerPilot.Api.Authorization;

/// <summary>
/// Named policies registered at startup, alongside the dynamic
/// <c>Permission:{name}</c> policies.
/// </summary>
/// <remarks>
/// Named policies are for rules that combine several conditions or that express an
/// intent broader than one permission. A rule that is simply "holds permission X"
/// should use <see cref="HasPermissionAttribute"/> instead — wrapping it in a named
/// policy adds a layer of indirection and nothing else.
/// </remarks>
public static class AuthorizationPolicies
{
    public const string RequireAdministrator = nameof(RequireAdministrator);

    /// <summary>
    /// Every account confirmed enough to act, as opposed to merely authenticated.
    /// </summary>
    public const string RequireVerifiedAccount = nameof(RequireVerifiedAccount);

    public static void AddCareerPilotPolicies(this AuthorizationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        // Fallback policy, applied to every endpoint that carries no authorization
        // metadata of its own. Endpoints are therefore protected by default and must
        // opt out with [AllowAnonymous] — the reverse of the framework default, where
        // forgetting [Authorize] silently publishes an endpoint.
        options.FallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();

        options.AddPolicy(RequireAdministrator, policy => policy
            .RequireAuthenticatedUser()
            .RequireRole(SystemRoles.Administrator));

        options.AddPolicy(RequireVerifiedAccount, policy => policy
            .RequireAuthenticatedUser()
            .RequireClaim(AuthenticationClaimTypes.SecurityStamp));
    }
}
