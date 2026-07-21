using CareerPilot.Application.Jobs.Models;

namespace CareerPilot.Application.Dashboard.Models;

public sealed record DashboardMetricsDto(
    int TotalApplied,
    int ActiveInterviews,
    int TotalOffers,
    double ResponseRatePercentage,
    int SavedJobsCount,
    double AverageResumeScore);

public sealed record UpcomingInterviewWidgetDto(
    Guid ApplicationId,
    string JobTitle,
    string CompanyName,
    string RoundName,
    DateTimeOffset ScheduledAt,
    string InterviewerName);

public sealed record ActivityFeedItemDto(
    Guid Id,
    string Title,
    string Description,
    string Category,
    DateTimeOffset Timestamp);

public sealed record DashboardOverviewDto(
    DashboardMetricsDto Metrics,
    IReadOnlyList<UpcomingInterviewWidgetDto> UpcomingInterviews,
    IReadOnlyList<JobDto> RecommendedJobs,
    IReadOnlyList<JobDto> RecentlyViewedJobs,
    IReadOnlyList<ActivityFeedItemDto> ActivityFeed,
    IReadOnlyList<NotificationDto> RecentNotifications);
