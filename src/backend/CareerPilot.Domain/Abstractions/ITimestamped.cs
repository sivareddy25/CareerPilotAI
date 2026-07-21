namespace CareerPilot.Domain.Abstractions;

/// <summary>
/// Lightweight timestamp contract for entities that only need creation
/// and last-modified times without full audit context.
/// </summary>
public interface ITimestamped
{
    DateTime CreatedAt { get; set; }
    DateTime UpdatedAt { get; set; }
}
