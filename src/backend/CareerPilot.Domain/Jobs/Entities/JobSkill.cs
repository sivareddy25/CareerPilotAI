using CareerPilot.Domain.Entities;

namespace CareerPilot.Domain.Jobs.Entities;

public sealed class JobSkill : EntityBase
{
    private JobSkill()
    {
        SkillName = string.Empty;
    }

    public JobSkill(Guid jobId, string skillName, bool isRequired = true)
    {
        JobId = jobId;
        SkillName = skillName.Trim();
        IsRequired = isRequired;
    }

    public Guid JobId { get; private set; }
    public string SkillName { get; private set; }
    public bool IsRequired { get; private set; }
}
