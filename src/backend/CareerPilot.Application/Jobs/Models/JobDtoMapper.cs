using CareerPilot.Application.Jobs.Matching;
using CareerPilot.Domain.Jobs.Entities;

namespace CareerPilot.Application.Jobs.Models;

/// <summary>
/// Maps a <see cref="Job"/> aggregate to its <see cref="JobDto"/>.
/// </summary>
/// <remarks>
/// Extracted because three query handlers — job list, job detail, and the dashboard — need the
/// identical projection. The dashboard previously carried its own copy that hard-coded the
/// company as "techcorp"/"Software"/"Tech company" regardless of the real employer, so every
/// recommended job on the dashboard showed the wrong company. One mapper removes the drift.
///
/// Callers must load <see cref="Job.Company"/>, <see cref="Job.Skills"/> and <see cref="Job.Tags"/>
/// (the repository's Include calls do this): the null-coalescing here guards against a missing
/// company row, not against an unloaded navigation, which would silently produce empty data.
/// </remarks>
public static class JobDtoMapper
{
    public static JobDto ToDto(Job job, MatchScore? matchScore = null)
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
            job.LastSynchronizedAt,
            matchScore?.OverallScore,
            matchScore?.MatchedSkills,
            matchScore?.Components
                .Select(c => new MatchComponentDto(c.Name, c.Score, c.Detail))
                .ToList(),
            matchScore?.Summary);
    }
}
