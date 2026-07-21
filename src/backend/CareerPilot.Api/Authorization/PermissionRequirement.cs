using Microsoft.AspNetCore.Authorization;

namespace CareerPilot.Api.Authorization;

/// <summary>
/// Requires the caller to hold a named permission.
/// </summary>
public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
