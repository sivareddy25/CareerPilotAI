using System.Text.Json;
using System.Text.Json.Serialization;
using CareerPilot.Application.Abstractions.Jobs;
using CareerPilot.Domain.Jobs;
using CareerPilot.Infrastructure.Configuration;
using CareerPilot.Infrastructure.Jobs.Normalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CareerPilot.Infrastructure.Jobs.Providers;

public sealed class LeverJobProvider(
    IHttpClientFactory httpClientFactory,
    ILogger<LeverJobProvider> logger) : IJobProvider
{
    private const string ProviderName = nameof(JobProviderKind.Lever);

    private static readonly List<string> DefaultLeverCompanies =
    [
        "atlassian", "palantir", "netflix", "twitch", "rover", "figma", "canva", "spotify", "datadog", "cloudflare"
    ];

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public JobProviderKind ProviderKind => JobProviderKind.Lever;

    public async Task<IReadOnlyList<RawJobPayload>> FetchJobsAsync(CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient(ProviderName);
        client.BaseAddress ??= new Uri("https://api.lever.co/");

        var payloads = new List<RawJobPayload>();

        foreach (var company in DefaultLeverCompanies)
        {
            try
            {
                payloads.AddRange(await FetchCompanyJobsAsync(client, company, cancellationToken));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Lever company {Company} could not be fetched.", company);
            }
        }

        logger.LogInformation("Lever provider ingested {Count} active postings.", payloads.Count);
        return payloads;
    }

    private async Task<IReadOnlyList<RawJobPayload>> FetchCompanyJobsAsync(
        HttpClient client,
        string company,
        CancellationToken cancellationToken)
    {
        var requestUri = $"v0/postings/{Uri.EscapeDataString(company)}?mode=json";
        using var response = await client.GetAsync(requestUri, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return [];
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var jobs = await JsonSerializer.DeserializeAsync<List<LeverJob>>(stream, SerializerOptions, cancellationToken);

        if (jobs is not { Count: > 0 })
        {
            return [];
        }

        return jobs
            .Where(j => !string.IsNullOrWhiteSpace(j.Text))
            .Take(100)
            .Select(j => MapToPayload(j, company))
            .ToList();
    }

    private static RawJobPayload MapToPayload(LeverJob job, string company)
    {
        var description = JobContentSanitizer.ToPlainText(job.Description ?? job.Text ?? "");
        var (responsibilities, requirements, benefits) = JobContentSanitizer.ExtractSections(description);
        var location = JobLocationParser.Parse(job.Categories?.Location);

        return new RawJobPayload(
            ExternalJobId: job.Id ?? Guid.NewGuid().ToString(),
            Source: JobProviderKind.Lever,
            Title: job.Text?.Trim() ?? "Software Engineer",
            CompanyName: company.ToUpperFirst(),
            CompanyWebsite: null,
            Description: description,
            Requirements: requirements,
            Responsibilities: responsibilities,
            Benefits: benefits,
            Country: location.Country,
            State: location.State,
            City: location.City,
            RemoteType: location.RemoteType,
            EmploymentType: JobContentSanitizer.DetectEmploymentType(job.Text ?? ""),
            ExperienceLevel: JobSeniorityParser.Parse(job.Text ?? ""),
            MinSalary: null,
            MaxSalary: null,
            Currency: "USD",
            PayPeriod: "Yearly",
            PostedAt: job.CreatedAt.HasValue
                ? DateTimeOffset.FromUnixTimeMilliseconds(job.CreatedAt.Value)
                : DateTimeOffset.UtcNow,
            ExpiresAt: null,
            ApplyUrl: job.HostedUrl ?? $"https://jobs.lever.co/{company}/{job.Id}",
            Language: "en",
            Skills: [".NET", "C#", "Angular", "TypeScript", "SQL"],
            Tags: [job.Categories?.Team ?? "Engineering"],
            RawJsonMetadata: JsonSerializer.Serialize(new { provider = "lever", company, job_id = job.Id }));
    }

    private sealed record LeverJob
    {
        public string? Id { get; init; }
        public string? Text { get; init; }
        public long? CreatedAt { get; init; }
        public string? Description { get; init; }
        public string? HostedUrl { get; init; }
        public LeverCategories? Categories { get; init; }
    }

    private sealed record LeverCategories
    {
        public string? Location { get; init; }
        public string? Team { get; init; }
        public string? Commitment { get; init; }
    }
}

internal static class StringExtensions
{
    public static string ToUpperFirst(this string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return input;
        return char.ToUpperInvariant(input[0]) + input[1..];
    }
}
