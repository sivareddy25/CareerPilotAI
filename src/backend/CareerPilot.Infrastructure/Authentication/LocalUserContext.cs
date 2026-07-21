namespace CareerPilot.Infrastructure.Authentication;

/// <summary>Local-mode identity data used by desktop and development workflows.</summary>
public sealed class LocalUserContext
{
    public Guid UserId => LocalUserProvider.DefaultLocalUserId;
    public string Email => "local.user@careerpilot.internal";
    public bool IsAuthenticated => true;
}
