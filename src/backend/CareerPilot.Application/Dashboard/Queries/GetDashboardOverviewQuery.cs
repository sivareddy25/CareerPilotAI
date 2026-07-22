using CareerPilot.Application.Abstractions.Authentication;
using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Abstractions.Persistence;
using CareerPilot.Application.Dashboard.Models;
using CareerPilot.Application.Jobs.Matching;
using CareerPilot.Application.Jobs.Models;

namespace CareerPilot.Application.Dashboard.Queries;

public sealed record GetDashboardOverviewQuery(Guid UserId) : IQuery<DashboardOverviewDto>;

internal sealed class GetDashboardOverviewQueryHandler(
    IJobRepository jobRepository,
    IUserProfileRepository profileRepository,
    ICurrentUserService currentUser,
    IJobMatchScoringService scoringService)
    : IQueryHandler<GetDashboardOverviewQuery, DashboardOverviewDto>
{
    private const int RecommendedJobCount = 6;

    // How much of the active set to score for the dashboard's "recommended" picks. Deterministic
    // scoring over a few hundred rows is cheap; the cap only guards against the table growing
    // without bound.
    private const int ScoringPoolCap = 1000;

    public async Task<DashboardOverviewDto> Handle(GetDashboardOverviewQuery query, CancellationToken cancellationToken)
    {
        var profile = currentUser.UserId is { } userId
            ? await profileRepository.GetByUserIdAsync(userId, cancellationToken)
            : null;

        var canScore = profile is not null
            && (profile.Skills.Count > 0 || !string.IsNullOrWhiteSpace(profile.TargetJobTitles));

        // When there is a scorable profile, recommend the highest-matching jobs; otherwise the
        // most recent. Either way the companies are real — via the shared mapper — unlike the old
        // handler that hard-coded every company to "TechCorp" and capped the list at two.
        var (pool, totalJobs) = await jobRepository.GetPagedAsync(
            new JobFilterParams(PageSize: canScore ? ScoringPoolCap : RecommendedJobCount),
            cancellationToken);

        var recommendedJobs = canScore
            ? pool
                .Select(job => JobDtoMapper.ToDto(job, scoringService.Score(job, profile!)))
                .OrderByDescending(dto => dto.MatchScore ?? 0)
                .Take(RecommendedJobCount)
                .ToList()
            : pool.Take(RecommendedJobCount).Select(job => JobDtoMapper.ToDto(job)).ToList();

        // The application-tracking, saved-jobs and résumé-scoring subsystems are not built yet
        // (later stages of this slice), so these are reported as zero rather than invented. The
        // one figure that is real — how many jobs have actually been ingested — is surfaced in
        // the activity feed below, since the metrics contract has no field for it.
        var metrics = new DashboardMetricsDto(
            TotalApplied: 0,
            ActiveInterviews: 0,
            TotalOffers: 0,
            ResponseRatePercentage: 0,
            SavedJobsCount: 0,
            AverageResumeScore: 0);

        var activity = BuildActivityFeed(totalJobs, recommendedJobs);

        return new DashboardOverviewDto(
            metrics,
            // No interviews or notifications exist without application tracking. Empty is the
            // honest state; the previous seeded "interview with TechCorp in 2 days" was fiction.
            UpcomingInterviews: [],
            RecommendedJobs: recommendedJobs,
            // Nothing tracks views yet, so "recently viewed" is genuinely empty rather than a
            // duplicated slice of the recommended list.
            RecentlyViewedJobs: [],
            ActivityFeed: activity,
            RecentNotifications: []);
    }

    private static IReadOnlyList<ActivityFeedItemDto> BuildActivityFeed(
        int totalJobs,
        IReadOnlyList<JobDto> recentJobs)
    {
        if (totalJobs == 0)
        {
            return
            [
                new ActivityFeedItemDto(
                    Guid.NewGuid(),
                    "No jobs yet",
                    "Run a job synchronization to pull openings from the configured providers.",
                    "Ingestion",
                    DateTimeOffset.UtcNow),
            ];
        }

        var items = new List<ActivityFeedItemDto>
        {
            new(
                Guid.NewGuid(),
                "Jobs available",
                $"{totalJobs} active job{(totalJobs == 1 ? "" : "s")} ingested and ready to browse.",
                "Ingestion",
                DateTimeOffset.UtcNow),
        };

        // Each real ingested job, newest first, as a truthful feed entry — no fabricated
        // interviews or résumé scores.
        items.AddRange(recentJobs.Take(3).Select(job => new ActivityFeedItemDto(
            Guid.NewGuid(),
            "Job ingested",
            $"{job.Title} at {job.Company.Name} — via {job.SourceName}.",
            "Ingestion",
            job.LastSynchronizedAt)));

        return items;
    }
}
