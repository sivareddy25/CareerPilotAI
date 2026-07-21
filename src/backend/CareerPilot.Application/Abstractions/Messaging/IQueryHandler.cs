namespace CareerPilot.Application.Abstractions.Messaging;

/// <summary>
/// Handles a single <typeparamref name="TQuery"/>. Exactly one handler
/// per query type; registered automatically by assembly scanning.
/// </summary>
public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    Task<TResponse> Handle(TQuery query, CancellationToken cancellationToken);
}
