using CareerPilot.Domain.Jobs;

namespace CareerPilot.Infrastructure.Jobs.Providers;

/// <summary>
/// Lever board provider. Not implemented yet — see <see cref="NotImplementedJobProvider"/>.
/// </summary>
/// <remarks>
/// Lever publishes a public postings API (<c>api.lever.co/v0/postings/{company}?mode=json</c>),
/// so this is a real next candidate. Until its adapter is written it contributes no jobs.
/// </remarks>
public sealed class LeverJobProvider : NotImplementedJobProvider
{
    public override JobProviderKind ProviderKind => JobProviderKind.Lever;
}
