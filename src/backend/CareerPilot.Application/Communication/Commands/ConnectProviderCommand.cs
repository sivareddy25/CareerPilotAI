using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Domain.Communication;

namespace CareerPilot.Application.Communication.Commands;

public sealed record ConnectProviderCommand(
    Guid UserId,
    CommunicationProviderKind ProviderKind,
    string OAuthAuthCode) : ICommand<bool>;

public sealed record DisconnectProviderCommand(
    Guid UserId,
    Guid AccountId) : ICommand<bool>;

internal sealed class ConnectProviderCommandHandler : ICommandHandler<ConnectProviderCommand, bool>
{
    public Task<bool> Handle(ConnectProviderCommand command, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }
}

internal sealed class DisconnectProviderCommandHandler : ICommandHandler<DisconnectProviderCommand, bool>
{
    public Task<bool> Handle(DisconnectProviderCommand command, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }
}
