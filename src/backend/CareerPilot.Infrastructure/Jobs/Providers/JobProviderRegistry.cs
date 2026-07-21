using CareerPilot.Application.Abstractions.Jobs;
using CareerPilot.Domain.Jobs;

namespace CareerPilot.Infrastructure.Jobs.Providers;

public sealed class JobProviderRegistry(IEnumerable<IJobProvider> providers) : IJobProviderRegistry
{
    private readonly Dictionary<JobProviderKind, IJobProvider> _providers =
        providers.ToDictionary(p => p.ProviderKind);

    public IJobProvider? GetProvider(JobProviderKind providerKind) =>
        _providers.GetValueOrDefault(providerKind);

    public IReadOnlyCollection<IJobProvider> GetAllProviders() =>
        _providers.Values.ToList().AsReadOnly();
}
