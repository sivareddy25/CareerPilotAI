using System.Security.Cryptography;
using System.Text;
using CareerPilot.Application.Abstractions.Jobs;
using CareerPilot.Application.Abstractions.Persistence;
using CareerPilot.Domain.Jobs.Entities;
using CareerPilot.Domain.Jobs.ValueObjects;

namespace CareerPilot.Infrastructure.Jobs.Services;

public sealed class JobNormalizationService(ICompanyRepository companyRepository) : IJobNormalizationService
{
    public async Task<Job> NormalizeAsync(RawJobPayload payload, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(payload);

        // 1. Resolve or Create Company
        var companyName = string.IsNullOrWhiteSpace(payload.CompanyName) ? "Unknown Company" : payload.CompanyName.Trim();
        var company = await companyRepository.GetByNameAsync(companyName, cancellationToken);
        if (company is null)
        {
            company = Company.Create(companyName, payload.CompanyWebsite, null, null, null);
            await companyRepository.AddAsync(company, cancellationToken);
        }

        // 2. Value Objects
        var location = Location.Create(
            payload.Country ?? "United States",
            payload.State,
            payload.City,
            payload.RemoteType);

        var salary = SalaryRange.Create(
            payload.MinSalary,
            payload.MaxSalary,
            payload.Currency,
            payload.PayPeriod);

        // 3. Content Hash calculation for change detection
        var hashInput = $"{payload.Title.Trim()}|{companyName}|{payload.Description}|{payload.Requirements}|{location.DisplayLocation}";
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(hashInput)));

        // 4. Create Job aggregate
        var job = Job.Create(
            payload.ExternalJobId,
            payload.Source,
            payload.Title,
            company.Id,
            payload.Description,
            payload.Requirements,
            payload.Responsibilities,
            payload.Benefits,
            location,
            salary,
            payload.EmploymentType,
            payload.ExperienceLevel,
            payload.PostedAt,
            payload.ExpiresAt,
            payload.ApplyUrl,
            payload.Language,
            payload.RawJsonMetadata,
            hash);

        job.SetSkills(payload.Skills);
        job.SetTags(payload.Tags);

        return job;
    }
}
