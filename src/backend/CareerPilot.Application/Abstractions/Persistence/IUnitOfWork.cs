namespace CareerPilot.Application.Abstractions.Persistence;

/// <summary>
/// Abstracts the transactional boundary for EF Core SaveChanges operations.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
