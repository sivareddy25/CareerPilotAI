using CareerPilot.Domain.Entities;

namespace CareerPilot.Domain.Jobs.Entities;

public sealed class JobTag : EntityBase
{
    private JobTag()
    {
        Tag = string.Empty;
    }

    public JobTag(Guid jobId, string tag)
    {
        JobId = jobId;
        Tag = tag.Trim().ToLowerInvariant();
    }

    public Guid JobId { get; private set; }
    public string Tag { get; private set; }
}
