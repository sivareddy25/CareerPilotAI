using CareerPilot.Application.Abstractions.Jobs;
using CareerPilot.Domain.Jobs;

namespace CareerPilot.Infrastructure.Jobs.Providers;

/// <summary>
/// Base for providers that are registered but whose real integration is not built yet.
/// </summary>
/// <remarks>
/// These providers previously returned a hand-written sample job each — "Staff Engineer at Acme
/// Innovations", apply URL <c>jobs.lever.co/acme/201</c> and the like. Those fabricated postings
/// were ingested alongside the real Greenhouse jobs and, on Apply, sent the user to a URL that
/// does not exist, showing "no job found". A not-yet-implemented provider must contribute
/// nothing rather than fiction, so it returns an empty set until its adapter lands.
///
/// Kept as concrete registrations (rather than deleted) so the registry, the provider enum, and
/// the configuration surface stay complete — implementing one becomes replacing its
/// <see cref="FetchJobsAsync"/>, with no wiring change elsewhere.
/// </remarks>
public abstract class NotImplementedJobProvider : IJobProvider
{
    public abstract JobProviderKind ProviderKind { get; }

    public Task<IReadOnlyList<RawJobPayload>> FetchJobsAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<RawJobPayload>>([]);
}
