using CareerPilot.Domain.Abstractions;

namespace CareerPilot.Domain.Entities;

/// <summary>
/// Base class for entities that support soft deletion. A global query filter
/// automatically excludes soft-deleted rows from all queries.
/// </summary>
public abstract class SoftDeleteEntity : AuditableEntity, ISoftDelete
{
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
