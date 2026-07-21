using CareerPilot.Application.Abstractions.Jobs;
using CareerPilot.Domain.Jobs;

namespace CareerPilot.Infrastructure.Jobs.Providers;

public sealed class SmartRecruitersJobProvider : IJobProvider
{
    public JobProviderKind ProviderKind => JobProviderKind.SmartRecruiters;

    public Task<IReadOnlyList<RawJobPayload>> FetchJobsAsync(CancellationToken cancellationToken = default)
    {
        var sampleData = new List<RawJobPayload>
        {
            new(
                ExternalJobId: "sr-501",
                Source: JobProviderKind.SmartRecruiters,
                Title: "DevOps & Infrastructure Engineer",
                CompanyName: "CloudScale Systems",
                CompanyWebsite: "https://cloudscale.example.com",
                Description: "Manage Kubernetes clusters, PostgreSQL database migrations, and CI/CD pipelines.",
                Requirements: "Docker, Kubernetes, GitHub Actions, Terraform, Npgsql.",
                Responsibilities: "Automate build deployments and infrastructure monitoring.",
                Benefits: "Flexible hours, internet stipend, hardware allowance.",
                Country: "United States",
                State: "CO",
                City: "Denver",
                RemoteType: RemoteType.Remote,
                EmploymentType: EmploymentType.FullTime,
                ExperienceLevel: ExperienceLevel.MidLevel,
                MinSalary: 140000,
                MaxSalary: 170000,
                Currency: "USD",
                PayPeriod: "Yearly",
                PostedAt: DateTimeOffset.UtcNow.AddDays(-6),
                ExpiresAt: DateTimeOffset.UtcNow.AddDays(24),
                ApplyUrl: "https://jobs.smartrecruiters.com/CloudScale/501",
                Language: "en",
                Skills: ["DevOps", "Kubernetes", "PostgreSQL", "Docker"],
                Tags: ["devops", "remote", "cloud"],
                RawJsonMetadata: "{\"provider\":\"smartrecruiters\",\"id\":\"501\"}"
            )
        };

        return Task.FromResult<IReadOnlyList<RawJobPayload>>(sampleData);
    }
}

public sealed class CompanyCareerPageJobProvider : IJobProvider
{
    public JobProviderKind ProviderKind => JobProviderKind.CareerPage;

    public Task<IReadOnlyList<RawJobPayload>> FetchJobsAsync(CancellationToken cancellationToken = default)
    {
        var sampleData = new List<RawJobPayload>
        {
            new(
                ExternalJobId: "cp-601",
                Source: JobProviderKind.CareerPage,
                Title: "Principal Product Designer",
                CompanyName: "CareerPilot AI",
                CompanyWebsite: "https://careerpilot.example.com",
                Description: "Lead modern SaaS design systems and Fluent 2 inspired accessibility specifications.",
                Requirements: "7+ years design system architecture, UX micro-animations, Figma mastery.",
                Responsibilities: "Guide component specs, conduct accessibility audits.",
                Benefits: "Top-tier health, equity, annual conference budget.",
                Country: "United States",
                State: "CA",
                City: "San Francisco",
                RemoteType: RemoteType.Hybrid,
                EmploymentType: EmploymentType.FullTime,
                ExperienceLevel: ExperienceLevel.Director,
                MinSalary: 190000,
                MaxSalary: 230000,
                Currency: "USD",
                PayPeriod: "Yearly",
                PostedAt: DateTimeOffset.UtcNow.AddDays(-1),
                ExpiresAt: DateTimeOffset.UtcNow.AddDays(29),
                ApplyUrl: "https://careerpilot.example.com/careers/601",
                Language: "en",
                Skills: ["UX Design", "Design Systems", "Fluent 2", "Accessibility"],
                Tags: ["design", "product", "san-francisco"],
                RawJsonMetadata: "{\"provider\":\"careerpage\",\"id\":\"601\"}"
            )
        };

        return Task.FromResult<IReadOnlyList<RawJobPayload>>(sampleData);
    }
}
