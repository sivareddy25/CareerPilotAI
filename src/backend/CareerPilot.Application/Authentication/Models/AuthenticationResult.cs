namespace CareerPilot.Application.Authentication.Models;

/// <summary>
/// What a successful sign-in, registration or refresh returns to the client.
/// </summary>
/// <param name="AccessToken">Short-lived bearer token.</param>
/// <param name="RefreshToken">
/// Long-lived rotation credential. Returned exactly once per issuance; the server
/// keeps only its hash and cannot reissue this value.
/// </param>
public sealed record AuthenticationResult(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    DateTime RefreshTokenExpiresAt,
    UserProfile User)
{
    public string TokenType => "Bearer";
}
