using CareerPilot.Domain.Jobs;
using CareerPilot.Domain.Jobs.Entities;

namespace CareerPilot.Application.Abstractions.Jobs;

public sealed record JobSyncResult(
    JobProviderKind Provider,
    int ProcessedCount,
    int InsertedCount,
    int UpdatedCount,
    int DeactivatedCount,
    bool Success,
    string? FailureReason = null);

public interface IJobNormalizationService
{
    Task<Job> NormalizeAsync(RawJobPayload payload, CancellationToken cancellationToken = default);
}

public interface IJobSynchronizationService
{
    Task<JobSyncResult> SynchronizeProviderAsync(JobProviderKind providerKind, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<JobSyncResult>> SynchronizeAllProvidersAsync(CancellationToken cancellationToken = default);
}
