using CareerPilot.Domain.Jobs;

namespace CareerPilot.Infrastructure.Jobs.Providers;

/// <summary>
/// Ashby board provider. Not implemented yet — see <see cref="NotImplementedJobProvider"/>.
/// </summary>
/// <remarks>
/// Ashby exposes a public job-board API (<c>api.ashbyhq.com/posting-api/job-board/{name}</c>),
/// a candidate for a later increment. Until then it contributes no jobs.
/// </remarks>
public sealed class AshbyJobProvider : NotImplementedJobProvider
{
    public override JobProviderKind ProviderKind => JobProviderKind.Ashby;
}
