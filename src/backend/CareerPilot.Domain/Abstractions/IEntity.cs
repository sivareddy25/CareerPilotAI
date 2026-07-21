namespace CareerPilot.Domain.Abstractions;

/// <summary>
/// Marker interface for all persistable domain entities.
/// Every entity must expose a stable, globally unique identifier.
/// </summary>
public interface IEntity
{
    Guid Id { get; }
}
