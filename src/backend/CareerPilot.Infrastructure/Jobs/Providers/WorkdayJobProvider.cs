using CareerPilot.Domain.Jobs;

namespace CareerPilot.Infrastructure.Jobs.Providers;

/// <summary>
/// Workday provider. Not implemented yet — see <see cref="NotImplementedJobProvider"/>.
/// </summary>
/// <remarks>
/// Workday tenants expose a JSON endpoint per site (<c>{tenant}.myworkdayjobs.com/wday/cxs/…</c>)
/// but each employer's host and site name differ, so this needs more configuration than the
/// board APIs. Deferred until after the simpler providers. Contributes no jobs meanwhile.
/// </remarks>
public sealed class WorkdayJobProvider : NotImplementedJobProvider
{
    public override JobProviderKind ProviderKind => JobProviderKind.Workday;
}
