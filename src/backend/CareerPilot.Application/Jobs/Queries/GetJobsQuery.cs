using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Abstractions.Persistence;
using CareerPilot.Application.Jobs.Models;
using CareerPilot.Domain.Jobs.Entities;

namespace CareerPilot.Application.Jobs.Queries;

public sealed record PagedJobsResultDto(
    IReadOnlyList<JobDto> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages);

public sealed record GetJobsQuery(JobFilterParams Filter) : IQuery<PagedJobsResultDto>;

internal sealed class GetJobsQueryHandler(IJobRepository jobRepository)
    : IQueryHandler<GetJobsQuery, PagedJobsResultDto>
{
    public async Task<PagedJobsResultDto> Handle(GetJobsQuery query, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await jobRepository.GetPagedAsync(query.Filter, cancellationToken);

        var dtos = items.Select(MapToDto).ToList();
        var totalPages = (int)Math.Ceiling(totalCount / (double)query.Filter.PageSize);

        return new PagedJobsResultDto(dtos, totalCount, query.Filter.PageNumber, query.Filter.PageSize, Math.Max(1, totalPages));
    }

    private static JobDto MapToDto(Job job)
    {
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
