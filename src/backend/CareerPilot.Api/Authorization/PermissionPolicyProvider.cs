using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace CareerPilot.Api.Authorization;

/// <summary>
/// Materialises <c>Permission:{name}</c> policies on demand.
/// </summary>
/// <remarks>
/// Falls through to <see cref="DefaultAuthorizationPolicyProvider"/> for every other
/// policy name, so explicitly registered policies keep working unchanged. Returning
/// <c>null</c> for an unrecognised name is important: the authorization middleware
/// treats a missing policy as an error rather than as permission granted, so a typo
/// fails closed.
/// </remarks>
internal sealed class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallback;

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallback = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallback.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallback.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (!policyName.StartsWith(HasPermissionAttribute.PolicyPrefix, StringComparison.Ordinal))
        {
            return _fallback.GetPolicyAsync(policyName);
        }

        var permission = policyName[HasPermissionAttribute.PolicyPrefix.Length..];

        var policy = new AuthorizationPolicyBuilder()
            // Authentication is required as well as the permission. Without this, an
            // anonymous request reaches the handler with an empty principal and is
            // denied for the wrong reason — a 403 where a 401 belongs, which stops
            // clients from knowing to refresh their token.
            .RequireAuthenticatedUser()
            .AddRequirements(new PermissionRequirement(permission))
            .Build();

        return Task.FromResult<AuthorizationPolicy?>(policy);
    }
}
