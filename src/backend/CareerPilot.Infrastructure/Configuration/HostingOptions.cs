using CareerPilot.Domain.Configuration;

namespace CareerPilot.Infrastructure.Configuration;

public sealed class HostingOptions
{
    public const string SectionName = "Hosting";

    public HostingMode Mode { get; set; } = HostingMode.Local;

    public bool AutoCreateLocalUser { get; set; } = true;

    public string DefaultLocalUserEmail { get; set; } = "local.user@careerpilot.internal";

    public string DefaultLocalUserName { get; set; } = "Local User";

    public bool IsLocalMode => Mode == HostingMode.Local;

    public bool IsSaaSMode => Mode == HostingMode.SaaS;
}
