using CareerPilot.Domain.Entities.Profiles;
using CareerPilot.Domain.Jobs.Entities;

namespace CareerPilot.Application.Jobs.Matching;

/// <summary>
/// Scores how well a job fits a user's career profile.
/// </summary>
public interface IJobMatchScoringService
{
    /// <summary>
    /// Scores <paramref name="job"/> against <paramref name="profile"/>, or returns null when the
    /// profile has too little to score against (no skills and no target titles) — in which case a
    /// score would be meaningless and the caller should show none rather than a misleading number.
    /// </summary>
    MatchScore? Score(Job job, UserProfile profile);
}
