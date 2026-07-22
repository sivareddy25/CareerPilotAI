using CareerPilot.Domain.Jobs;

namespace CareerPilot.Infrastructure.Jobs.Providers;

/// <summary>
/// SmartRecruiters provider. Not implemented yet — see <see cref="NotImplementedJobProvider"/>.
/// </summary>
/// <remarks>
/// SmartRecruiters has a public postings API (<c>api.smartrecruiters.com/v1/companies/{id}/postings</c>).
/// A later increment. Contributes no jobs until then.
/// </remarks>
public sealed class SmartRecruitersJobProvider : NotImplementedJobProvider
{
    public override JobProviderKind ProviderKind => JobProviderKind.SmartRecruiters;
}

/// <summary>
/// Generic company career-page provider. Not implemented yet — see
/// <see cref="NotImplementedJobProvider"/>.
/// </summary>
/// <remarks>
/// A career page has no standard schema, so this is the hardest source and depends on the
/// browser-automation work later in the slice. Contributes no jobs until then; previously it
/// emitted the "Principal Product Designer at CareerPilot AI" sample whose apply URL 404ed.
/// </remarks>
public sealed class CompanyCareerPageJobProvider : NotImplementedJobProvider
{
    public override JobProviderKind ProviderKind => JobProviderKind.CareerPage;
}
