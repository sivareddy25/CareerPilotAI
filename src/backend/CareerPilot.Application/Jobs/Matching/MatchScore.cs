namespace CareerPilot.Application.Jobs.Matching;

/// <summary>One weighted dimension of a match score, kept for the "why" breakdown.</summary>
/// <param name="Name">Human label, e.g. "Skills" or "Seniority".</param>
/// <param name="Score">0–100 for this dimension alone.</param>
/// <param name="Weight">Its share of the overall score, before normalisation.</param>
/// <param name="Detail">Short human explanation of how this dimension scored.</param>
public sealed record MatchComponent(string Name, int Score, double Weight, string Detail);

/// <summary>
/// The result of scoring one job against one career profile.
/// </summary>
/// <remarks>
/// Deterministic and explainable by construction: <see cref="OverallScore"/> is a weighted mean
/// of <see cref="Components"/>, and <see cref="MatchedSkills"/> lists exactly which profile
/// skills were found in the posting. The qualitative "missing skills / should you apply"
/// narrative is produced separately by the LLM explainer — this stays a pure calculation so it
/// can run over every job on every list request without a model call.
/// </remarks>
public sealed record MatchScore(
    int OverallScore,
    IReadOnlyList<string> MatchedSkills,
    IReadOnlyList<MatchComponent> Components,
    string Summary);
