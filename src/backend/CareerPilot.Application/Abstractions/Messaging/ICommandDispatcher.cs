namespace CareerPilot.Application.Abstractions.Messaging;

/// <summary>
/// Routes a command to its handler, running any registered pipeline behaviors first.
/// </summary>
public interface ICommandDispatcher
{
    Task<TResponse> Send<TResponse>(
        ICommand<TResponse> command,
        CancellationToken cancellationToken = default);
}
