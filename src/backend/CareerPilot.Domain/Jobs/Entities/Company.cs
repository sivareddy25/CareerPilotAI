using CareerPilot.Domain.Entities;

namespace CareerPilot.Domain.Jobs.Entities;

public sealed class Company : AuditableEntity
{
    private readonly List<Job> _jobs = [];

    private Company()
    {
        Name = string.Empty;
        Slug = string.Empty;
    }

    private Company(string name, string slug, string? websiteUrl, string? careerPageUrl, string? industry, string? description)
    {
        Name = name;
        Slug = slug;
        WebsiteUrl = websiteUrl;
        CareerPageUrl = careerPageUrl;
        Industry = industry;
        Description = description;
    }

    public string Name { get; private set; }
    public string Slug { get; private set; }
    public string? WebsiteUrl { get; private set; }
    public string? CareerPageUrl { get; private set; }
    public string? LogoUrl { get; private set; }
    public string? Industry { get; private set; }
    public string? Description { get; private set; }

    public IReadOnlyCollection<Job> Jobs => _jobs.AsReadOnly();

    public static Company Create(
        string name,
        string? websiteUrl = null,
        string? careerPageUrl = null,
        string? industry = null,
        string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var slug = GenerateSlug(name);
        return new Company(name.Trim(), slug, websiteUrl, careerPageUrl, industry, description);
    }

    public void UpdateProfile(string? websiteUrl, string? careerPageUrl, string? industry, string? description)
    {
        WebsiteUrl = websiteUrl ?? WebsiteUrl;
        CareerPageUrl = careerPageUrl ?? CareerPageUrl;
        Industry = industry ?? Industry;
        Description = description ?? Description;
    }

    private static string GenerateSlug(string input) =>
        input.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace(".", "")
            .Trim('-');
}
