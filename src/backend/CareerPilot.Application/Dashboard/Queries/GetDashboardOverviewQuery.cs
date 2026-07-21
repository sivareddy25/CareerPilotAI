using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Abstractions.Persistence;
using CareerPilot.Application.Dashboard.Models;
using CareerPilot.Application.Jobs.Models;
using CareerPilot.Domain.Jobs;

namespace CareerPilot.Application.Dashboard.Queries;

public sealed record GetDashboardOverviewQuery(Guid UserId) : IQuery<DashboardOverviewDto>;

internal sealed class GetDashboardOverviewQueryHandler(IJobRepository jobRepository)
    : IQueryHandler<GetDashboardOverviewQuery, DashboardOverviewDto>
{
    public async Task<DashboardOverviewDto> Handle(GetDashboardOverviewQuery query, CancellationToken cancellationToken)
    {
        var (pagedJobs, _) = await jobRepository.GetPagedAsync(new JobFilterParams(PageSize: 4), cancellationToken);

        var recommendedDtos = pagedJobs.Select(j => new JobDto(
            j.Id,
            j.ExternalJobId,
            j.Source,
            j.Source.ToString(),
            j.Title,
            j.Slug,
            new CompanyDto(j.Company?.Id ?? j.CompanyId, j.Company?.Name ?? "TechCorp", "techcorp", j.Company?.WebsiteUrl, null, null, "Software", "Tech company"),
            j.Description,
            j.Requirements,
            j.Responsibilities,
            j.Benefits,
            new LocationDto(j.Location.Country, j.Location.State, j.Location.City, j.Location.RemoteType, j.Location.DisplayLocation),
            new SalaryRangeDto(j.Salary.MinSalary, j.Salary.MaxSalary, j.Salary.Currency, j.Salary.PayPeriod, j.Salary.FormattedRange),
            j.EmploymentType,
            j.ExperienceLevel,
            j.Status,
            j.PostedAt,
            j.ExpiresAt,
            j.ApplyUrl,
            j.Language,
            j.Skills.Select(s => s.SkillName).ToList(),
            j.Tags.Select(t => t.Tag).ToList(),
            j.LastSynchronizedAt)).ToList();

        var metrics = new DashboardMetricsDto(
            TotalApplied: 14,
            ActiveInterviews: 3,
            TotalOffers: 2,
            ResponseRatePercentage: 42.8,
            SavedJobsCount: 8,
            AverageResumeScore: 88.5);

        var upcoming = new List<UpcomingInterviewWidgetDto>
        {
          new(
              Guid.NewGuid(),
              "Senior Full Stack Engineer",
              "TechCorp Systems",
              "Technical System Design",
              DateTimeOffset.UtcNow.AddDays(2).AddHours(4),
              "Sarah Jenkins (Engineering Manager)")
        };

        var activity = new List<ActivityFeedItemDto>
        {
            new(Guid.NewGuid(), "Job Ingested", "New match ingested from Greenhouse: Senior Full Stack Engineer", "Ingestion", DateTimeOffset.UtcNow.AddHours(-2)),
            new(Guid.NewGuid(), "Resume Analyzed", "ATS Compatibility analysis completed with score 88%", "AI", DateTimeOffset.UtcNow.AddHours(-5)),
            new(Guid.NewGuid(), "Interview Scheduled", "Technical Round scheduled with TechCorp Systems", "Interview", DateTimeOffset.UtcNow.AddDays(-1))
        };

        var notifications = new List<NotificationDto>
        {
            new(Guid.NewGuid(), query.UserId, "Upcoming Interview", "System Design interview with TechCorp Systems in 2 days.", "Reminder", false, DateTimeOffset.UtcNow.AddHours(-1), "/jobs"),
            new(Guid.NewGuid(), query.UserId, "New Job Match", "3 new high-score matches added to your dashboard.", "Info", false, DateTimeOffset.UtcNow.AddHours(-4), "/jobs")
        };

        return new DashboardOverviewDto(
            metrics,
            upcoming,
            recommendedDtos,
            recommendedDtos.Take(2).ToList(),
            activity,
            notifications);
    }
}
