using CareerPilot.Domain.Jobs;
using CareerPilot.Domain.Jobs.Entities;

namespace CareerPilot.Application.Abstractions.Persistence;

public sealed record JobFilterParams(
    string? SearchTerm = null,
    string? Country = null,
    string? City = null,
    RemoteType? RemoteType = null,
    ExperienceLevel? ExperienceLevel = null,
    EmploymentType? EmploymentType = null,
    decimal? MinSalary = null,
    string? Skill = null,
    Guid? CompanyId = null,
    int PageNumber = 1,
    int PageSize = 20);

public interface IJobRepository
{
    Task<Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Job?> GetByExternalIdAsync(string externalJobId, JobProviderKind source, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Job> Items, int TotalCount)> GetPagedAsync(JobFilterParams filter, CancellationToken cancellationToken = default);
    Task AddAsync(Job job, CancellationToken cancellationToken = default);
    void Update(Job job);
    Task<int> DeactivateExpiredJobsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivates a provider's active jobs whose external IDs are absent from
    /// <paramref name="seenExternalIds"/> — the postings the provider no longer lists.
    /// </summary>
    /// <remarks>
    /// Call only after a successful fetch. A provider that returned an empty set because it
    /// failed would, through this method, deactivate everything it owns; that is tolerable
    /// because deactivation is reversible (a returning posting reactivates on the next sync)
    /// but must not be triggered by a fetch that never really happened.
    /// </remarks>
    Task<int> DeactivateMissingAsync(
        JobProviderKind source,
        IReadOnlyCollection<string> seenExternalIds,
        CancellationToken cancellationToken = default);
}

public interface ICompanyRepository
{
    Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Company?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<Company?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Company>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Company company, CancellationToken cancellationToken = default);
}
