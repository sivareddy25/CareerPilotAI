using CareerPilot.Application.Abstractions.Persistence;

namespace CareerPilot.Infrastructure.Authentication;

public sealed class LocalUserContext : IUserContext
{
    public Guid UserId => LocalUserProvider.DefaultLocalUserId;
    public string Email => "local.user@careerpilot.internal";
    public bool IsAuthenticated => true;
}
