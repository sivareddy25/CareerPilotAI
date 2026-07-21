namespace CareerPilot.Domain.Abstractions;

/// <summary>
/// Soft-delete contract. Entities are never physically removed; they are
/// flagged and filtered out by a global query filter.
/// </summary>
public interface ISoftDelete
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
    string? DeletedBy { get; set; }
}
