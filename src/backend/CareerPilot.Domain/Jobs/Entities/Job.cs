using CareerPilot.Domain.Entities;
using CareerPilot.Domain.Jobs.ValueObjects;

namespace CareerPilot.Domain.Jobs.Entities;

public sealed class Job : SoftDeleteEntity
{
    private readonly List<JobSkill> _skills = [];
    private readonly List<JobTag> _tags = [];

    private Job()
    {
        ExternalJobId = string.Empty;
        Title = string.Empty;
        Slug = string.Empty;
        Description = string.Empty;
        Language = string.Empty;
        ContentHash = string.Empty;
        Location = Location.Create("United States", null, null, RemoteType.Onsite);
        Salary = SalaryRange.Create(null, null);
    }

    private Job(
        string externalJobId,
        JobProviderKind source,
        string title,
        Guid companyId,
        string description,
        string? requirements,
        string? responsibilities,
        string? benefits,
        Location location,
        SalaryRange salary,
        EmploymentType employmentType,
        ExperienceLevel experienceLevel,
        DateTimeOffset postedAt,
        DateTimeOffset? expiresAt,
        string? applyUrl,
        string? language,
        string? sourceMetadataJson,
        string contentHash)
    {
        ExternalJobId = externalJobId;
        Source = source;
        Title = title.Trim();
        Slug = GenerateSlug(title);
        CompanyId = companyId;
        Description = description;
        Requirements = requirements;
        Responsibilities = responsibilities;
        Benefits = benefits;
        Location = location;
        Salary = salary;
        EmploymentType = employmentType;
        ExperienceLevel = experienceLevel;
        Status = JobStatus.Active;
        PostedAt = postedAt;
        ExpiresAt = expiresAt;
        ApplyUrl = applyUrl;
        Language = language ?? "en";
        SourceMetadataJson = sourceMetadataJson;
        ContentHash = contentHash;
        LastSynchronizedAt = DateTimeOffset.UtcNow;
    }

    public string ExternalJobId { get; private set; }
    public JobProviderKind Source { get; private set; }
    public string Title { get; private set; }
    public string Slug { get; private set; }
    public Guid CompanyId { get; private set; }
    public Company? Company { get; private set; }

    public string Description { get; private set; }
    public string? Requirements { get; private set; }
    public string? Responsibilities { get; private set; }
    public string? Benefits { get; private set; }

    public Location Location { get; private set; }
    public SalaryRange Salary { get; private set; }

    public EmploymentType EmploymentType { get; private set; }
    public ExperienceLevel ExperienceLevel { get; private set; }
    public JobStatus Status { get; private set; }

    public DateTimeOffset PostedAt { get; private set; }
    public DateTimeOffset? ExpiresAt { get; private set; }
    public string? ApplyUrl { get; private set; }
    public string Language { get; private set; }
    public string? SourceMetadataJson { get; private set; }

    public DateTimeOffset LastSynchronizedAt { get; private set; }
    public string ContentHash { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    public IReadOnlyCollection<JobSkill> Skills => _skills.AsReadOnly();
    public IReadOnlyCollection<JobTag> Tags => _tags.AsReadOnly();

    public static Job Create(
        string externalJobId,
        JobProviderKind source,
        string title,
        Guid companyId,
        string description,
        string? requirements,
        string? responsibilities,
        string? benefits,
        Location location,
        SalaryRange salary,
        EmploymentType employmentType,
        ExperienceLevel experienceLevel,
        DateTimeOffset postedAt,
        DateTimeOffset? expiresAt,
        string? applyUrl,
        string? language,
        string? sourceMetadataJson,
        string contentHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(externalJobId);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        return new Job(
            externalJobId.Trim(),
            source,
            title,
            companyId,
            description,
            requirements,
            responsibilities,
            benefits,
            location,
            salary,
            employmentType,
            experienceLevel,
            postedAt,
            expiresAt,
            applyUrl,
            language,
            sourceMetadataJson,
            contentHash);
    }

    public void UpdatePosting(
        string title,
        string description,
        string? requirements,
        string? responsibilities,
        string? benefits,
        Location location,
        SalaryRange salary,
        EmploymentType employmentType,
        ExperienceLevel experienceLevel,
        DateTimeOffset? expiresAt,
        string? applyUrl,
        string? sourceMetadataJson,
        string newContentHash)
    {
        Title = title.Trim();
        Description = description;
        Requirements = requirements;
        Responsibilities = responsibilities;
        Benefits = benefits;
        Location = location;
        Salary = salary;
        EmploymentType = employmentType;
        ExperienceLevel = experienceLevel;
        ExpiresAt = expiresAt;
        ApplyUrl = applyUrl;
        SourceMetadataJson = sourceMetadataJson;
        ContentHash = newContentHash;
        LastSynchronizedAt = DateTimeOffset.UtcNow;

        // Seen in the feed with new content — live again if it had been deactivated.
        if (Status == JobStatus.Expired)
        {
            Status = JobStatus.Active;
        }
    }

    public void Deactivate()
    {
        Status = JobStatus.Expired;
    }

    public void SyncTouch()
    {
        // A job seen in the feed is live again. Without this, a posting that was deactivated
        // for dropping out of the feed and later returned would stay Expired forever, because
        // nothing else resets the status on the reappearance path.
        if (Status == JobStatus.Expired)
        {
            Status = JobStatus.Active;
        }

        LastSynchronizedAt = DateTimeOffset.UtcNow;
    }

    public void SetSkills(IEnumerable<string> skills)
    {
        _skills.Clear();
        foreach (var s in skills.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            _skills.Add(new JobSkill(Id, s));
        }
    }

    public void SetTags(IEnumerable<string> tags)
    {
        _tags.Clear();
        foreach (var t in tags.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            _tags.Add(new JobTag(Id, t));
        }
    }

    private static string GenerateSlug(string input) =>
        input.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace(".", "")
            .Trim('-');
}
