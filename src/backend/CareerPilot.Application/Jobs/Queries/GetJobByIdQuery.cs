using CareerPilot.Application.Abstractions.Authentication;
using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Abstractions.Persistence;
using CareerPilot.Application.Jobs.Matching;
using CareerPilot.Application.Jobs.Models;

namespace CareerPilot.Application.Jobs.Queries;

public sealed record GetJobByIdQuery(Guid JobId) : IQuery<JobDto?>;

internal sealed class GetJobByIdQueryHandler(
    IJobRepository jobRepository,
    IUserProfileRepository profileRepository,
    ICurrentUserService currentUser,
    IJobMatchScoringService scoringService)
    : IQueryHandler<GetJobByIdQuery, JobDto?>
{
    public async Task<JobDto?> Handle(GetJobByIdQuery query, CancellationToken cancellationToken)
    {
        var job = await jobRepository.GetByIdAsync(query.JobId, cancellationToken);

        if (job is null)
        {
            return null;
        }

        var profile = currentUser.UserId is { } userId
            ? await profileRepository.GetByUserIdAsync(userId, cancellationToken)
            : null;

        var matchScore = profile is null ? null : scoringService.Score(job, profile);

        return JobDtoMapper.ToDto(job, matchScore);
    }
}
