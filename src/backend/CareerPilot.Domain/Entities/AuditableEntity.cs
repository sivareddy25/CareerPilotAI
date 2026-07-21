using CareerPilot.Domain.Abstractions;

namespace CareerPilot.Domain.Entities;

/// <summary>
/// Base class for entities requiring a full audit trail and optimistic concurrency.
/// The persistence layer populates audit fields and concurrency token automatically.
/// </summary>
public abstract class AuditableEntity : EntityBase, IAuditable, ITimestamped
{
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Optimistic concurrency token (mapped to xmin / concurrency token in EF Core).
    /// </summary>
    public uint Version { get; set; }
}
