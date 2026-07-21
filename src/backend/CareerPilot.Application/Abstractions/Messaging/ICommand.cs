namespace CareerPilot.Application.Abstractions.Messaging;

/// <summary>
/// Marks a request that changes state and returns <typeparamref name="TResponse"/>.
/// </summary>
/// <remarks>
/// Deliberately invariant. Covariance on <typeparamref name="TResponse"/> would let a
/// command be seen as <c>ICommand&lt;BaseType&gt;</c>, causing the dispatcher to resolve
/// the wrong closed handler type.
/// </remarks>
public interface ICommand<TResponse>
{
}
