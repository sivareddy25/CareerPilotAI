using System.Collections.Concurrent;
using CareerPilot.Application.Abstractions.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace CareerPilot.Application.Messaging;

/// <summary>
/// Default <see cref="ICommandDispatcher"/>. Resolves the closed handler type for the
/// command's runtime type and composes registered behaviors around it.
/// </summary>
internal sealed class CommandDispatcher(IServiceProvider serviceProvider) : ICommandDispatcher
{
    // A command type maps to exactly one response type, so the runtime type is a
    // sufficient cache key. Reflection therefore happens once per command type,
    // not once per dispatch.
    private static readonly ConcurrentDictionary<Type, object> Wrappers = new();

    public Task<TResponse> Send<TResponse>(
        ICommand<TResponse> command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var wrapper = (CommandHandlerWrapper<TResponse>)Wrappers.GetOrAdd(
            command.GetType(),
            static commandType => Activator.CreateInstance(
                typeof(CommandHandlerWrapperImpl<,>).MakeGenericType(commandType, typeof(TResponse)))!);

        return wrapper.Handle(command, serviceProvider, cancellationToken);
    }
}

/// <summary>
/// Non-generic-over-command bridge, letting the dispatcher hold a reference to a
/// wrapper whose command type is only known at runtime.
/// </summary>
internal abstract class CommandHandlerWrapper<TResponse>
{
    public abstract Task<TResponse> Handle(
        object command,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}

internal sealed class CommandHandlerWrapperImpl<TCommand, TResponse> : CommandHandlerWrapper<TResponse>
    where TCommand : ICommand<TResponse>
{
    public override Task<TResponse> Handle(
        object command,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var typedCommand = (TCommand)command;
        var handler = serviceProvider.GetRequiredService<ICommandHandler<TCommand, TResponse>>();

        RequestHandlerDelegate<TResponse> pipeline = () => handler.Handle(typedCommand, cancellationToken);

        // Reversed so the first-registered behavior ends up outermost.
        foreach (var behavior in serviceProvider
                     .GetServices<IPipelineBehavior<TCommand, TResponse>>()
                     .Reverse())
        {
            var next = pipeline;
            var current = behavior;
            pipeline = () => current.Handle(typedCommand, next, cancellationToken);
        }

        return pipeline();
    }
}
