namespace CareerPilot.Domain.Abstractions;

/// <summary>
/// Entities implementing this contract carry a full audit trail:
/// who created, who last modified, and when.
/// </summary>
public interface IAuditable
{
    DateTime CreatedAt { get; set; }
    DateTime UpdatedAt { get; set; }
    string? CreatedBy { get; set; }
    string? UpdatedBy { get; set; }
}
