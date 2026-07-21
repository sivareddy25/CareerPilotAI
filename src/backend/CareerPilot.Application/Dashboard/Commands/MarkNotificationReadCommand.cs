using CareerPilot.Application.Abstractions.Messaging;

namespace CareerPilot.Application.Dashboard.Commands;

public sealed record MarkNotificationReadCommand(Guid UserId, Guid NotificationId) : ICommand<bool>;

internal sealed class MarkNotificationReadCommandHandler : ICommandHandler<MarkNotificationReadCommand, bool>
{
    public Task<bool> Handle(MarkNotificationReadCommand command, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }
}
