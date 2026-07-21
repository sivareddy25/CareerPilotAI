using CareerPilot.Domain.Jobs;

namespace CareerPilot.Application.Abstractions.Jobs;

public interface IJobProvider
{
    JobProviderKind ProviderKind { get; }
    Task<IReadOnlyList<RawJobPayload>> FetchJobsAsync(CancellationToken cancellationToken = default);
}

public interface IJobProviderRegistry
{
    IJobProvider? GetProvider(JobProviderKind providerKind);
    IReadOnlyCollection<IJobProvider> GetAllProviders();
}
