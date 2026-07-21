namespace CareerPilot.Application.Abstractions.Messaging;

/// <summary>
/// Marks a request that reads state without modifying it and returns
/// <typeparamref name="TResponse"/>.
/// </summary>
public interface IQuery<TResponse>
{
}
