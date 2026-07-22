namespace CareerPilot.Domain.Entities.Profiles;

/// <summary>
/// A single skill on a user's career profile, with optional years of experience.
/// </summary>
/// <remarks>
/// A child entity of <see cref="UserProfile"/> rather than a delimited string column, because
/// the match-scoring engine needs to iterate skills individually and weight them by experience.
/// Modelled after <see cref="Jobs.Entities.JobSkill"/> for consistency.
/// </remarks>
public sealed class ProfileSkill : EntityBase
{
    private ProfileSkill()
    {
        Name = string.Empty;
    }

    public ProfileSkill(Guid profileId, string name, int? yearsOfExperience = null)
    {
        ProfileId = profileId;
        Name = name.Trim();
        YearsOfExperience = yearsOfExperience is > 0 ? yearsOfExperience : null;
    }

    public Guid ProfileId { get; private set; }

    public string Name { get; private set; }

    /// <summary>Years using this skill. Null when the user did not specify.</summary>
    public int? YearsOfExperience { get; private set; }
}
