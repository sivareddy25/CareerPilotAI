namespace CareerPilot.Application.Abstractions.Messaging;

/// <summary>
/// Handles a single <typeparamref name="TCommand"/>. Exactly one handler
/// per command type; registered automatically by assembly scanning.
/// </summary>
public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    Task<TResponse> Handle(TCommand command, CancellationToken cancellationToken);
}
