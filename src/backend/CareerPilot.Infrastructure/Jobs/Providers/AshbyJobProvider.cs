using CareerPilot.Application.Abstractions.Jobs;
using CareerPilot.Domain.Jobs;

namespace CareerPilot.Infrastructure.Jobs.Providers;

public sealed class AshbyJobProvider : IJobProvider
{
    public JobProviderKind ProviderKind => JobProviderKind.Ashby;

    public Task<IReadOnlyList<RawJobPayload>> FetchJobsAsync(CancellationToken cancellationToken = default)
    {
        var sampleData = new List<RawJobPayload>
        {
            new(
                ExternalJobId: "ash-301",
                Source: JobProviderKind.Ashby,
                Title: "Senior Frontend Engineer (Angular)",
                CompanyName: "Nexus Cloud",
                CompanyWebsite: "https://nexuscloud.example.com",
                Description: "Build modern Fluent 2 design systems using Angular Signals and Standalone Components.",
                Requirements: "Strong mastery of SCSS, CSS Custom Variables, Angular Signals, Accessibility (WCAG).",
                Responsibilities: "Maintain component libraries, optimize bundle load speeds.",
                Benefits: "Flexible remote work, monthly wellness stipend, annual retreat.",
                Country: "United States",
                State: "TX",
                City: "Austin",
                RemoteType: RemoteType.Remote,
                EmploymentType: EmploymentType.FullTime,
                ExperienceLevel: ExperienceLevel.SeniorLevel,
                MinSalary: 150000,
                MaxSalary: 180000,
                Currency: "USD",
                PayPeriod: "Yearly",
                PostedAt: DateTimeOffset.UtcNow.AddDays(-2),
                ExpiresAt: DateTimeOffset.UtcNow.AddDays(28),
                ApplyUrl: "https://jobs.ashbyhq.com/nexuscloud/301",
                Language: "en",
                Skills: ["Angular", "TypeScript", "SCSS", "Fluent 2", "Accessibility"],
                Tags: ["frontend", "angular", "remote"],
                RawJsonMetadata: "{\"provider\":\"ashby\",\"job_id\":\"301\"}"
            )
        };

        return Task.FromResult<IReadOnlyList<RawJobPayload>>(sampleData);
    }
}
