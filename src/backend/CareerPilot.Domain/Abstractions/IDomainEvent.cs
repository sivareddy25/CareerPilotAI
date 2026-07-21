namespace CareerPilot.Domain.Abstractions;

/// <summary>
/// Contract for domain events emitted by entities when state changes occur.
/// </summary>
public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
