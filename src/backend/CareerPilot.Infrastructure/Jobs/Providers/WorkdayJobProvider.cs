using CareerPilot.Application.Abstractions.Jobs;
using CareerPilot.Domain.Jobs;

namespace CareerPilot.Infrastructure.Jobs.Providers;

public sealed class WorkdayJobProvider : IJobProvider
{
    public JobProviderKind ProviderKind => JobProviderKind.Workday;

    public Task<IReadOnlyList<RawJobPayload>> FetchJobsAsync(CancellationToken cancellationToken = default)
    {
        var sampleData = new List<RawJobPayload>
        {
            new(
                ExternalJobId: "wd-401",
                Source: JobProviderKind.Workday,
                Title: "Enterprise Software Engineer",
                CompanyName: "Global Financial Corp",
                CompanyWebsite: "https://globalfin.example.com",
                Description: "Workday integration for enterprise financial systems.",
                Requirements: "C# .NET, SQL Server/PostgreSQL, CQRS.",
                Responsibilities: "Maintain transaction security and audit logging.",
                Benefits: "Pension plan, health benefits, tuition reimbursement.",
                Country: "United States",
                State: "IL",
                City: "Chicago",
                RemoteType: RemoteType.Hybrid,
                EmploymentType: EmploymentType.FullTime,
                ExperienceLevel: ExperienceLevel.MidLevel,
                MinSalary: 130000,
                MaxSalary: 160000,
                Currency: "USD",
                PayPeriod: "Yearly",
                PostedAt: DateTimeOffset.UtcNow.AddDays(-4),
                ExpiresAt: DateTimeOffset.UtcNow.AddDays(26),
                ApplyUrl: "https://globalfin.wd1.myworkdayjobs.com/Careers/job/401",
                Language: "en",
                Skills: ["C#", ".NET", "SQL", "CQRS"],
                Tags: ["finance", "chicago", "hybrid"],
                RawJsonMetadata: "{\"provider\":\"workday\",\"req_id\":\"401\"}"
            )
        };

        return Task.FromResult<IReadOnlyList<RawJobPayload>>(sampleData);
    }
}
