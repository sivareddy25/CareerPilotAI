using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Communication.Models;
using CareerPilot.Domain.Communication;

namespace CareerPilot.Application.Communication.Queries;

public sealed record GetConnectedAccountsQuery(Guid UserId) : IQuery<IReadOnlyList<ConnectedAccountDto>>;

internal sealed class GetConnectedAccountsQueryHandler : IQueryHandler<GetConnectedAccountsQuery, IReadOnlyList<ConnectedAccountDto>>
{
    public Task<IReadOnlyList<ConnectedAccountDto>> Handle(GetConnectedAccountsQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyList<ConnectedAccountDto> accounts = new List<ConnectedAccountDto>
        {
            new(Guid.NewGuid(), query.UserId, CommunicationProviderKind.Microsoft365, "Microsoft 365 Outlook", "user.career@outlook.com", true, DateTimeOffset.UtcNow.AddDays(-10), DateTimeOffset.UtcNow.AddHours(-1)),
            new(Guid.NewGuid(), query.UserId, CommunicationProviderKind.Google, "Google Gmail & Calendar", "user.dev@gmail.com", true, DateTimeOffset.UtcNow.AddDays(-5), DateTimeOffset.UtcNow.AddHours(-2))
        };

        return Task.FromResult(accounts);
    }
}
