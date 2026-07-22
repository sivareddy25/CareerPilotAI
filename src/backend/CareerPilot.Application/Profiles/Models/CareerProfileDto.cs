using CareerPilot.Domain.Entities.Profiles;
using CareerPilot.Domain.Jobs;

namespace CareerPilot.Application.Profiles.Models;

/// <summary>One skill on the career profile.</summary>
public sealed record ProfileSkillDto(string Name, int? YearsOfExperience);

/// <summary>
/// The structured, machine-readable career profile the job-matching engine reads.
/// </summary>
/// <remarks>
/// Separate from the human-facing <see cref="ProfileDto"/> so the two evolve independently:
/// this one is shaped for the scorer (typed enums, numeric salary, an iterable skills list),
/// while <see cref="ProfileDto"/> stays the contact/identity view the account screen edits.
/// </remarks>
public sealed record CareerProfileDto(
    int? YearsOfExperience,
    decimal? DesiredSalaryAmount,
    string? DesiredSalaryCurrency,
    EmploymentType? PreferredEmploymentType,
    RemoteType? PreferredRemoteType,
    string? TargetJobTitles,
    IReadOnlyList<ProfileSkillDto> Skills)
{
    public static CareerProfileDto From(UserProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        return new CareerProfileDto(
            profile.YearsOfExperience,
            profile.DesiredSalaryAmount,
            profile.DesiredSalaryCurrency,
            profile.PreferredEmploymentType,
            profile.PreferredRemoteType,
            profile.TargetJobTitles,
            profile.Skills.Select(skill => new ProfileSkillDto(skill.Name, skill.YearsOfExperience)).ToList());
    }

    /// <summary>True when there is enough here to compute a meaningful match score.</summary>
    /// <remarks>
    /// Skills or target titles are the minimum: with neither, every job scores the same and the
    /// ranking is noise. The scorer checks this to decide whether to attach a score at all.
    /// </remarks>
    public bool IsScorable => Skills.Count > 0 || !string.IsNullOrWhiteSpace(TargetJobTitles);
}
