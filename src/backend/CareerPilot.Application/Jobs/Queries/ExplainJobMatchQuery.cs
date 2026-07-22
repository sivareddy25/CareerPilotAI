using CareerPilot.Application.Abstractions.Authentication;
using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Abstractions.Persistence;
using CareerPilot.Application.Jobs.Matching;

namespace CareerPilot.Application.Jobs.Queries;

/// <summary>Result of asking the AI to explain a single job's fit.</summary>
public sealed record JobMatchExplanationDto(
    Guid JobId,
    int? MatchScore,
    IReadOnlyList<string> Strengths,
    IReadOnlyList<string> Gaps,
    string Recommendation,
    bool GeneratedByAi);

public sealed record ExplainJobMatchQuery(Guid JobId) : IQuery<JobMatchExplanationDto?>;

internal sealed class ExplainJobMatchQueryHandler(
    IJobRepository jobRepository,
    IUserProfileRepository profileRepository,
    ICurrentUserService currentUser,
    IJobMatchScoringService scoringService,
    IJobMatchExplainer explainer)
    : IQueryHandler<ExplainJobMatchQuery, JobMatchExplanationDto?>
{
    public async Task<JobMatchExplanationDto?> Handle(ExplainJobMatchQuery query, CancellationToken cancellationToken)
    {
        var job = await jobRepository.GetByIdAsync(query.JobId, cancellationToken);

        if (job is null)
        {
            return null;
        }

        var profile = currentUser.UserId is { } userId
            ? await profileRepository.GetByUserIdAsync(userId, cancellationToken)
            : null;

        // Explanation is meaningful only relative to a profile. With none — or an empty one — the
        // scorer returns null, and there is nothing honest to explain, so the handler says so
        // rather than invoking the model on empty inputs.
        var score = profile is null ? null : scoringService.Score(job, profile);

        if (profile is null || score is null)
        {
            return new JobMatchExplanationDto(
                job.Id,
                null,
                [],
                [],
                "Add skills or target roles to your career profile to see why this job fits.",
                GeneratedByAi: false);
        }

        var explanation = await explainer.ExplainAsync(job, profile, score, cancellationToken);

        return new JobMatchExplanationDto(
            job.Id,
            score.OverallScore,
            explanation.Strengths,
            explanation.Gaps,
            explanation.Recommendation,
            explanation.GeneratedByAi);
    }
}
