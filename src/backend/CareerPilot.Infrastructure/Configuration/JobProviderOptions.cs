namespace CareerPilot.Infrastructure.Configuration;

/// <summary>
/// Per-provider ingestion settings, bound as named options under <c>JobProviders:{Kind}</c>.
/// </summary>
/// <remarks>
/// Named rather than one class per provider: every board-style source needs the same three
/// things — whether it is on, which tenants to pull, and how long to wait — so a per-provider
/// type would be five copies of the same shape. A new provider is a configuration section and
/// an <c>IJobProvider</c>; nothing here changes.
/// </remarks>
public sealed class JobProviderOptions
{
    public const string SectionName = "JobProviders";

    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Tenant identifiers on the provider's board — Greenhouse and Lever both address
    /// employers by a slug in the URL path (<c>boards-api.greenhouse.io/v1/boards/{board}</c>).
    /// Empty means the provider has nothing to fetch and is skipped.
    /// </summary>
    public IList<string> Boards { get; set; } = [];

    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Cap on jobs taken per board. Large employers post several hundred openings, and an
    /// unbounded first sync would write all of them before the user has expressed any
    /// preferences.
    /// </summary>
    public int MaxJobsPerBoard { get; set; } = 100;
}
