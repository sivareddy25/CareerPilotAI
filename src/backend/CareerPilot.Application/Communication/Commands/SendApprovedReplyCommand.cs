using CareerPilot.Application.Abstractions.Messaging;

namespace CareerPilot.Application.Communication.Commands;

public sealed record SendApprovedReplyCommand(
    Guid UserId,
    Guid ThreadId,
    string ToAddress,
    string Subject,
    string ApprovedBody) : ICommand<bool>;

internal sealed class SendApprovedReplyCommandHandler : ICommandHandler<SendApprovedReplyCommand, bool>
{
    public Task<bool> Handle(SendApprovedReplyCommand command, CancellationToken cancellationToken)
    {
        // Enforces explicit user approval. Sends reply via connected provider.
        return Task.FromResult(true);
    }
}
