using CareerPilot.Application.Abstractions.Jobs;
using CareerPilot.Domain.Jobs;

namespace CareerPilot.Infrastructure.Jobs.Providers;

public sealed class LeverJobProvider : IJobProvider
{
    public JobProviderKind ProviderKind => JobProviderKind.Lever;

    public Task<IReadOnlyList<RawJobPayload>> FetchJobsAsync(CancellationToken cancellationToken = default)
    {
        var sampleData = new List<RawJobPayload>
        {
            new(
                ExternalJobId: "lev-201",
                Source: JobProviderKind.Lever,
                Title: "Staff Software Engineer - Backend",
                CompanyName: "Acme Innovations",
                CompanyWebsite: "https://acme.example.com",
                Description: "Build high-throughput REST APIs and data processing pipelines.",
                Requirements: "Deep knowledge of C#, EF Core, Npgsql, Redis caching.",
                Responsibilities: "Own core backend services, maintain 99.99% uptime SLAs.",
                Benefits: "Comprehensive medical/dental, unlimited PTO, stock options.",
                Country: "United States",
                State: "WA",
                City: "Seattle",
                RemoteType: RemoteType.Onsite,
                EmploymentType: EmploymentType.FullTime,
                ExperienceLevel: ExperienceLevel.SeniorLevel,
                MinSalary: 180000,
                MaxSalary: 215000,
                Currency: "USD",
                PayPeriod: "Yearly",
                PostedAt: DateTimeOffset.UtcNow.AddDays(-5),
                ExpiresAt: DateTimeOffset.UtcNow.AddDays(25),
                ApplyUrl: "https://jobs.lever.co/acme/201",
                Language: "en",
                Skills: ["C#", "PostgreSQL", "Redis", "REST API"],
                Tags: ["backend", "seattle", "onsite"],
                RawJsonMetadata: "{\"provider\":\"lever\",\"posting_id\":\"201\"}"
            )
        };

        return Task.FromResult<IReadOnlyList<RawJobPayload>>(sampleData);
    }
}
