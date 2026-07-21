using System.Collections.Concurrent;
using CareerPilot.Application.Abstractions.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace CareerPilot.Application.Messaging;

/// <summary>
/// Default <see cref="IQueryDispatcher"/>. Mirrors <see cref="CommandDispatcher"/>;
/// kept as a separate type so read and write paths can diverge later
/// (read replicas, caching behaviors) without touching the write path.
/// </summary>
internal sealed class QueryDispatcher(IServiceProvider serviceProvider) : IQueryDispatcher
{
    private static readonly ConcurrentDictionary<Type, object> Wrappers = new();

    public Task<TResponse> Query<TResponse>(
        IQuery<TResponse> query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var wrapper = (QueryHandlerWrapper<TResponse>)Wrappers.GetOrAdd(
            query.GetType(),
            static queryType => Activator.CreateInstance(
                typeof(QueryHandlerWrapperImpl<,>).MakeGenericType(queryType, typeof(TResponse)))!);

        return wrapper.Handle(query, serviceProvider, cancellationToken);
    }
}

internal abstract class QueryHandlerWrapper<TResponse>
{
    public abstract Task<TResponse> Handle(
        object query,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}

internal sealed class QueryHandlerWrapperImpl<TQuery, TResponse> : QueryHandlerWrapper<TResponse>
    where TQuery : IQuery<TResponse>
{
    public override Task<TResponse> Handle(
        object query,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var typedQuery = (TQuery)query;
        var handler = serviceProvider.GetRequiredService<IQueryHandler<TQuery, TResponse>>();

        RequestHandlerDelegate<TResponse> pipeline = () => handler.Handle(typedQuery, cancellationToken);

        foreach (var behavior in serviceProvider
                     .GetServices<IPipelineBehavior<TQuery, TResponse>>()
                     .Reverse())
        {
            var next = pipeline;
            var current = behavior;
            pipeline = () => current.Handle(typedQuery, next, cancellationToken);
        }

        return pipeline();
    }
}
