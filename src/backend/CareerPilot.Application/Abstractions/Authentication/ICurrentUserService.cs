namespace CareerPilot.Application.Abstractions.Authentication;

/// <summary>
/// The identity behind the request currently being handled, read from the validated
/// token. Lets handlers stay free of any dependency on HTTP.
/// </summary>
/// <remarks>
/// Every member reflects claims that the authentication middleware has already
/// verified. Handlers may trust <see cref="UserId"/>; they must never accept a user
/// id from the request body in its place, or any authenticated caller could act as
/// any other user.
/// </remarks>
public interface ICurrentUserService
{
    Guid? UserId { get; }

    string? Email { get; }

    bool IsAuthenticated { get; }

    /// <summary>The <c>jti</c> of the presented access token.</summary>
    string? TokenId { get; }

    /// <summary>
    /// Expiry of the presented access token. Sign-out needs this to know how long the
    /// token must stay blacklisted — past it, ordinary expiry validation suffices.
    /// </summary>
    DateTime? TokenExpiresAt { get; }

    /// <summary>The <c>SecurityStamp</c> the presented token was minted with.</summary>
    string? SecurityStamp { get; }

    IReadOnlyCollection<string> Roles { get; }

    IReadOnlyCollection<string> Permissions { get; }

    /// <summary>Caller address, for audit records only — never an authorization input.</summary>
    string? IpAddress { get; }

    string? UserAgent { get; }
}
