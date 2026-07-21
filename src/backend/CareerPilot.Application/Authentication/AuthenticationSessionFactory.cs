using CareerPilot.Application.Abstractions.Authentication;
using CareerPilot.Application.Authentication.Models;
using CareerPilot.Domain.Entities.Identity;

namespace CareerPilot.Application.Authentication;

/// <summary>
/// Assembles the token pair and profile that every successful authentication returns.
/// </summary>
/// <remarks>
/// Shared by login, registration and refresh so all three issue sessions identically.
/// Duplicating this across handlers is how a security fix ends up applied to two of
/// the three paths.
/// </remarks>
internal sealed class AuthenticationSessionFactory(
    IJwtTokenService jwtTokenService,
    IRefreshTokenService refreshTokenService,
    IPermissionService permissionService)
{
    public async Task<AuthenticationResult> CreateAsync(User user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);

        var roles = await permissionService.GetRolesAsync(user.Id, cancellationToken);
        var permissions = await permissionService.GetPermissionsAsync(user.Id, cancellationToken);

        var accessToken = jwtTokenService.GenerateAccessToken(user, roles, permissions);
        var refreshToken = await refreshTokenService.IssueAsync(user.Id, cancellationToken);

        return new AuthenticationResult(
            accessToken.Value,
            refreshToken.RawToken,
            accessToken.ExpiresAt,
            refreshToken.ExpiresAt,
            UserProfile.From(user, roles, permissions));
    }

    /// <summary>
    /// Variant for refresh, where the refresh token has already been rotated by the
    /// caller and must not be issued a second time.
    /// </summary>
    public async Task<AuthenticationResult> CreateFromRotatedAsync(
        User user,
        IssuedRefreshToken rotatedToken,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(rotatedToken);

        var roles = await permissionService.GetRolesAsync(user.Id, cancellationToken);
        var permissions = await permissionService.GetPermissionsAsync(user.Id, cancellationToken);

        var accessToken = jwtTokenService.GenerateAccessToken(user, roles, permissions);

        return new AuthenticationResult(
            accessToken.Value,
            rotatedToken.RawToken,
            accessToken.ExpiresAt,
            rotatedToken.ExpiresAt,
            UserProfile.From(user, roles, permissions));
    }
}
