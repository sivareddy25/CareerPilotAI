namespace CareerPilot.Application.Abstractions.Messaging;

/// <summary>
/// Invokes the next stage of the pipeline (another behavior, or the handler itself).
/// </summary>
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();

/// <summary>
/// A cross-cutting stage wrapped around a handler — validation, logging,
/// transactions. Behaviors execute in registration order; each decides whether
/// to call <paramref name="next"/>.
/// </summary>
public interface IPipelineBehavior<in TRequest, TResponse>
{
    Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken);
}
