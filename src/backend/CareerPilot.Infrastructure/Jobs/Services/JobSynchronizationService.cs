using CareerPilot.Application.Abstractions.Jobs;
using CareerPilot.Application.Abstractions.Persistence;
using CareerPilot.Domain.Jobs;
using CareerPilot.Domain.Jobs.Entities;
using Microsoft.Extensions.Logging;

namespace CareerPilot.Infrastructure.Jobs.Services;

public sealed class JobSynchronizationService(
    IJobProviderRegistry providerRegistry,
    IJobNormalizationService normalizationService,
    IJobRepository jobRepository,
    IUnitOfWork unitOfWork,
    ILogger<JobSynchronizationService> logger) : IJobSynchronizationService
{
    public async Task<JobSyncResult> SynchronizeProviderAsync(JobProviderKind providerKind, CancellationToken cancellationToken = default)
    {
        var startedAt = DateTimeOffset.UtcNow;
        var provider = providerRegistry.GetProvider(providerKind);

        if (provider is null)
        {
            logger.LogWarning("Provider connector {ProviderKind} not registered.", providerKind);
            return new JobSyncResult(providerKind, 0, 0, 0, 0, false, $"Provider {providerKind} not available.");
        }

        var processed = 0;
        var inserted = 0;
        var updated = 0;

        try
        {
            logger.LogInformation("Starting synchronization for provider {ProviderKind}...", providerKind);
            var rawJobs = await provider.FetchJobsAsync(cancellationToken);
            processed = rawJobs.Count;

            foreach (var raw in rawJobs)
            {
                var normalizedJob = await normalizationService.NormalizeAsync(raw, cancellationToken);
                var existingJob = await jobRepository.GetByExternalIdAsync(normalizedJob.ExternalJobId, normalizedJob.Source, cancellationToken);

                if (existingJob is null)
                {
                    await jobRepository.AddAsync(normalizedJob, cancellationToken);
                    inserted++;
                }
                else
                {
                    if (existingJob.ContentHash != normalizedJob.ContentHash)
                    {
                        existingJob.UpdatePosting(
                            normalizedJob.Title,
                            normalizedJob.Description,
                            normalizedJob.Requirements,
                            normalizedJob.Responsibilities,
                            normalizedJob.Benefits,
                            normalizedJob.Location,
                            normalizedJob.Salary,
                            normalizedJob.EmploymentType,
                            normalizedJob.ExperienceLevel,
                            normalizedJob.ExpiresAt,
                            normalizedJob.ApplyUrl,
                            normalizedJob.SourceMetadataJson,
                            normalizedJob.ContentHash);

                        existingJob.SetSkills(raw.Skills);
                        existingJob.SetTags(raw.Tags);
                        jobRepository.Update(existingJob);
                        updated++;
                    }
                    else
                    {
                        existingJob.SyncTouch();
                        jobRepository.Update(existingJob);
                    }
                }
            }

            var deactivated = await jobRepository.DeactivateExpiredJobsAsync(cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var completedAt = DateTimeOffset.UtcNow;
            logger.LogInformation(
                "Completed synchronization for {ProviderKind}. Processed: {Processed}, Inserted: {Inserted}, Updated: {Updated}, Deactivated: {Deactivated}",
                providerKind, processed, inserted, updated, deactivated);

            return new JobSyncResult(providerKind, processed, inserted, updated, deactivated, true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Synchronization failed for provider {ProviderKind}", providerKind);
            return new JobSyncResult(providerKind, processed, inserted, updated, 0, false, ex.Message);
        }
    }

    public async Task<IReadOnlyList<JobSyncResult>> SynchronizeAllProvidersAsync(CancellationToken cancellationToken = default)
    {
        var results = new List<JobSyncResult>();
        foreach (var provider in providerRegistry.GetAllProviders())
        {
            var res = await SynchronizeProviderAsync(provider.ProviderKind, cancellationToken);
            results.Add(res);
        }
        return results;
    }
}
