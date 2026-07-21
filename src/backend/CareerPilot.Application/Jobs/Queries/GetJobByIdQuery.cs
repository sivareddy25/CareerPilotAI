using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Abstractions.Persistence;
using CareerPilot.Application.Jobs.Models;
using CareerPilot.Domain.Jobs.Entities;

namespace CareerPilot.Application.Jobs.Queries;

public sealed record GetJobByIdQuery(Guid JobId) : IQuery<JobDto?>;

internal sealed class GetJobByIdQueryHandler(IJobRepository jobRepository)
    : IQueryHandler<GetJobByIdQuery, JobDto?>
{
    public async Task<JobDto?> Handle(GetJobByIdQuery query, CancellationToken cancellationToken)
    {
        var job = await jobRepository.GetByIdAsync(query.JobId, cancellationToken);
        if (job is null) return null;

        var company = new CompanyDto(
            job.Company?.Id ?? job.CompanyId,
            job.Company?.Name ?? "Unknown Company",
            job.Company?.Slug ?? "unknown",
            job.Company?.WebsiteUrl,
            job.Company?.CareerPageUrl,
            job.Company?.LogoUrl,
            job.Company?.Industry,
            job.Company?.Description);

        var location = new LocationDto(
            job.Location.Country,
            job.Location.State,
            job.Location.City,
            job.Location.RemoteType,
            job.Location.DisplayLocation);

        var salary = new SalaryRangeDto(
            job.Salary.MinSalary,
            job.Salary.MaxSalary,
            job.Salary.Currency,
            job.Salary.PayPeriod,
            job.Salary.FormattedRange);

        return new JobDto(
            job.Id,
            job.ExternalJobId,
            job.Source,
            job.Source.ToString(),
            job.Title,
            job.Slug,
            company,
            job.Description,
            job.Requirements,
            job.Responsibilities,
            job.Benefits,
            location,
            salary,
            job.EmploymentType,
            job.ExperienceLevel,
            job.Status,
            job.PostedAt,
            job.ExpiresAt,
            job.ApplyUrl,
            job.Language,
            job.Skills.Select(s => s.SkillName).ToList(),
            job.Tags.Select(t => t.Tag).ToList(),
            job.LastSynchronizedAt);
    }
}
