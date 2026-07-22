using CareerPilot.Application.Abstractions.Authentication;
using CareerPilot.Application.Abstractions.Persistence;
using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Jobs.Matching;
using CareerPilot.Application.Jobs.Models;
using CareerPilot.Domain.Entities.Profiles;
using CareerPilot.Domain.Jobs.Entities;

namespace CareerPilot.Application.Jobs.Queries;

public sealed record PagedJobsResultDto(
    IReadOnlyList<JobDto> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages);

/// <param name="SortByMatch">
/// Rank by descending match score instead of recency. Ignored when the caller has no scorable
/// career profile — there is nothing to rank by — in which case recency stands.
/// </param>
public sealed record GetJobsQuery(JobFilterParams Filter, bool SortByMatch = false) : IQuery<PagedJobsResultDto>;

internal sealed class GetJobsQueryHandler(
    IJobRepository jobRepository,
    IUserProfileRepository profileRepository,
    ICurrentUserService currentUser,
    IJobMatchScoringService scoringService)
    : IQueryHandler<GetJobsQuery, PagedJobsResultDto>
{
    // Ranking by score means ordering the whole result set, which the database cannot do because
    // the score is not stored. So a match-sorted request materialises the matching jobs and ranks
    // them in memory. Capped because "materialise everything" must not become unbounded as the
    // table grows; the local dataset is a few hundred, and this ceiling leaves generous headroom
    // while keeping a single ranking pass cheap.
    private const int RankablePoolCap = 1000;

    public async Task<PagedJobsResultDto> Handle(GetJobsQuery query, CancellationToken cancellationToken)
    {
        var profile = await LoadProfileAsync(cancellationToken);
        var canScore = profile is not null
            && (profile.Skills.Count > 0 || !string.IsNullOrWhiteSpace(profile.TargetJobTitles));

        return query.SortByMatch && canScore
            ? await RankedAsync(query, profile!, cancellationToken)
            : await PagedAsync(query, profile, cancellationToken);
    }

    /// <summary>Database pagination by recency, scoring only the page that is returned.</summary>
    private async Task<PagedJobsResultDto> PagedAsync(GetJobsQuery query, UserProfile? profile, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await jobRepository.GetPagedAsync(query.Filter, cancellationToken);

        var dtos = items.Select(job => ToDto(job, profile)).ToList();
        var totalPages = (int)Math.Ceiling(totalCount / (double)query.Filter.PageSize);

        return new PagedJobsResultDto(dtos, totalCount, query.Filter.PageNumber, query.Filter.PageSize, Math.Max(1, totalPages));
    }

    /// <summary>Rank the full filtered set by score, then page in memory.</summary>
    private async Task<PagedJobsResultDto> RankedAsync(GetJobsQuery query, UserProfile profile, CancellationToken cancellationToken)
    {
        var poolFilter = query.Filter with { PageNumber = 1, PageSize = RankablePoolCap };
        var (pool, totalCount) = await jobRepository.GetPagedAsync(poolFilter, cancellationToken);

        var scored = pool
            .Select(job => (Dto: ToDto(job, profile), job.PostedAt))
            .OrderByDescending(x => x.Dto.MatchScore ?? 0)
            .ThenByDescending(x => x.PostedAt)
            .Select(x => x.Dto)
            .Skip((query.Filter.PageNumber - 1) * query.Filter.PageSize)
            .Take(query.Filter.PageSize)
            .ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)query.Filter.PageSize);

        return new PagedJobsResultDto(scored, totalCount, query.Filter.PageNumber, query.Filter.PageSize, Math.Max(1, totalPages));
    }

    private JobDto ToDto(Job job, UserProfile? profile) =>
        JobDtoMapper.ToDto(job, profile is null ? null : scoringService.Score(job, profile));

    private async Task<UserProfile?> LoadProfileAsync(CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            return null;
        }

        return await profileRepository.GetByUserIdAsync(userId, cancellationToken);
    }
}
