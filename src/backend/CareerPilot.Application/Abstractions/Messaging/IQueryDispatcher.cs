namespace CareerPilot.Application.Abstractions.Messaging;

/// <summary>
/// Routes a query to its handler, running any registered pipeline behaviors first.
/// </summary>
public interface IQueryDispatcher
{
    Task<TResponse> Query<TResponse>(
        IQuery<TResponse> query,
        CancellationToken cancellationToken = default);
}
