using CareerPilot.Domain.Entities.Identity;

namespace CareerPilot.Application.Abstractions.Authentication;

/// <summary>An issued access token and the metadata the caller needs to manage it.</summary>
/// <param name="Value">The compact-serialised JWT.</param>
/// <param name="TokenId">The <c>jti</c> claim, used to blacklist this exact token on sign-out.</param>
/// <param name="ExpiresAt">Absolute UTC expiry.</param>
public sealed record AccessToken(string Value, string TokenId, DateTime ExpiresAt);

/// <summary>
/// Mints signed access tokens.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Issues an access token carrying the user's identity, roles and permissions as
    /// claims, so routine authorization needs no database round-trip.
    /// </summary>
    AccessToken GenerateAccessToken(
        User user,
        IReadOnlyCollection<string> roles,
        IReadOnlyCollection<string> permissions);
}
