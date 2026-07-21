using CareerPilot.Application.Abstractions.Authentication;

namespace CareerPilot.Infrastructure.Authentication;

public sealed class LocalCurrentUserService : ICurrentUserService
{
    public Guid? UserId => LocalUserProvider.DefaultLocalUserId;
    public string? Email => "local.user@careerpilot.internal";
    public bool IsAuthenticated => true;
    public string? TokenId => "local-session-token-id";
    public DateTime? TokenExpiresAt => DateTime.MaxValue;
    public string? SecurityStamp => "local-security-stamp";
    public IReadOnlyCollection<string> Roles => new[] { "Admin", "User" };
    public IReadOnlyCollection<string> Permissions => Array.Empty<string>();
    public string? IpAddress => "127.0.0.1";
    public string? UserAgent => "LocalClient";
}
