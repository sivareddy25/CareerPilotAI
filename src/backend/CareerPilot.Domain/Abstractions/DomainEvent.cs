namespace CareerPilot.Domain.Abstractions;

/// <summary>
/// Abstract record base for domain events.
/// Implements <see cref="IDomainEvent"/> and sets <see cref="OccurredOn"/> to UTC DateTime.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}
