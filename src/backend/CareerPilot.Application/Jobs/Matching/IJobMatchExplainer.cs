using CareerPilot.Domain.Entities.Profiles;
using CareerPilot.Domain.Jobs.Entities;

namespace CareerPilot.Application.Jobs.Matching;

/// <summary>
/// A natural-language explanation of why a job does or doesn't fit a profile.
/// </summary>
/// <param name="Strengths">Reasons to apply, e.g. "Your Spark and Azure experience align."</param>
/// <param name="Gaps">Requirements the user appears to lack, e.g. "Terraform".</param>
/// <param name="Recommendation">A one-line verdict.</param>
/// <param name="GeneratedByAi">
/// False when the model was unavailable and this was assembled from the deterministic score
/// instead, so the UI can be honest about the source rather than presenting a fallback as AI.
/// </param>
public sealed record MatchExplanation(
    IReadOnlyList<string> Strengths,
    IReadOnlyList<string> Gaps,
    string Recommendation,
    bool GeneratedByAi);

/// <summary>
/// Produces the qualitative "why this job fits" narrative, on demand.
/// </summary>
/// <remarks>
/// Distinct from <see cref="IJobMatchScoringService"/> by cost and determinism: scoring is a pure
/// calculation run over every job in a list, while explanation is one model call made only when a
/// user asks about a single job. Keeping them apart is what lets the list stay fast.
/// </remarks>
public interface IJobMatchExplainer
{
    Task<MatchExplanation> ExplainAsync(
        Job job,
        UserProfile profile,
        MatchScore score,
        CancellationToken cancellationToken = default);
}
