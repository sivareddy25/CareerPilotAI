using CareerPilot.Application.Abstractions.Jobs;
using CareerPilot.Domain.Jobs;

namespace CareerPilot.Infrastructure.Jobs.Providers;

public sealed class GreenhouseJobProvider : IJobProvider
{
    public JobProviderKind ProviderKind => JobProviderKind.Greenhouse;

    public Task<IReadOnlyList<RawJobPayload>> FetchJobsAsync(CancellationToken cancellationToken = default)
    {
        var sampleData = new List<RawJobPayload>
        {
            new(
                ExternalJobId: "gh-101",
                Source: JobProviderKind.Greenhouse,
                Title: "Senior Full Stack Engineer",
                CompanyName: "TechCorp Systems",
                CompanyWebsite: "https://techcorp.example.com",
                Description: "Join TechCorp Systems building cloud-native infrastructure with C# .NET and Angular.",
                Requirements: "5+ years C# .NET, Angular, PostgreSQL experience.",
                Responsibilities: "Architect clean domain modules, write unit tests, design Signal state management.",
                Benefits: "Health insurance, 401(k) matching, flexible PTO, $2,000 learning budget.",
                Country: "United States",
                State: "CA",
                City: "San Francisco",
                RemoteType: RemoteType.Hybrid,
                EmploymentType: EmploymentType.FullTime,
                ExperienceLevel: ExperienceLevel.SeniorLevel,
                MinSalary: 165000,
                MaxSalary: 195000,
                Currency: "USD",
                PayPeriod: "Yearly",
                PostedAt: DateTimeOffset.UtcNow.AddDays(-3),
                ExpiresAt: DateTimeOffset.UtcNow.AddDays(27),
                ApplyUrl: "https://boards.greenhouse.io/techcorp/jobs/101",
                Language: "en",
                Skills: ["C#", ".NET 9", "Angular", "TypeScript", "PostgreSQL"],
                Tags: ["hybrid", "san-francisco", "senior"],
                RawJsonMetadata: "{\"provider\":\"greenhouse\",\"board_id\":\"techcorp\",\"job_id\":101}"
            ),
            new(
                ExternalJobId: "gh-102",
                Source: JobProviderKind.Greenhouse,
                Title: "Lead Systems Architect",
                CompanyName: "TechCorp Systems",
                CompanyWebsite: "https://techcorp.example.com",
                Description: "Drive microservices migration and CQRS data architecture across multi-region deployments.",
                Requirements: "8+ years in distributed systems, event-driven architectures.",
                Responsibilities: "Lead architecture reviews, establish design systems, optimize database queries.",
                Benefits: "Competitive equity package, remote workspace stipend, wellness benefits.",
                Country: "United States",
                State: "NY",
                City: "New York",
                RemoteType: RemoteType.Remote,
                EmploymentType: EmploymentType.FullTime,
                ExperienceLevel: ExperienceLevel.Lead,
                MinSalary: 210000,
                MaxSalary: 250000,
                Currency: "USD",
                PayPeriod: "Yearly",
                PostedAt: DateTimeOffset.UtcNow.AddDays(-1),
                ExpiresAt: DateTimeOffset.UtcNow.AddDays(30),
                ApplyUrl: "https://boards.greenhouse.io/techcorp/jobs/102",
                Language: "en",
                Skills: ["Clean Architecture", "CQRS", "Distributed Systems", "C#"],
                Tags: ["remote", "architecture", "lead"],
                RawJsonMetadata: "{\"provider\":\"greenhouse\",\"board_id\":\"techcorp\",\"job_id\":102}"
            )
        };

        return Task.FromResult<IReadOnlyList<RawJobPayload>>(sampleData);
    }
}
