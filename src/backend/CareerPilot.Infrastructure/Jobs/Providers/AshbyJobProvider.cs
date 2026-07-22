using System.Text.Json;
using System.Text.Json.Serialization;
using CareerPilot.Application.Abstractions.Jobs;
using CareerPilot.Domain.Jobs;
using CareerPilot.Infrastructure.Configuration;
using CareerPilot.Infrastructure.Jobs.Normalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CareerPilot.Infrastructure.Jobs.Providers;

public sealed class AshbyJobProvider(
    IHttpClientFactory httpClientFactory,
    ILogger<AshbyJobProvider> logger) : IJobProvider
{
    private const string ProviderName = nameof(JobProviderKind.Ashby);

    private static readonly List<string> DefaultAshbyCompanies =
    [
        "linear", "notion", "airtable", "ramp", "rippling", "retool", "vanta", "brex", "postman"
    ];

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public JobProviderKind ProviderKind => JobProviderKind.Ashby;

    public async Task<IReadOnlyList<RawJobPayload>> FetchJobsAsync(CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient(ProviderName);
        client.BaseAddress ??= new Uri("https://api.ashbyhq.com/");

        var payloads = new List<RawJobPayload>();

        foreach (var company in DefaultAshbyCompanies)
        {
            try
            {
                payloads.AddRange(await FetchCompanyJobsAsync(client, company, cancellationToken));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ashby company {Company} could not be fetched.", company);
            }
        }

        logger.LogInformation("Ashby provider ingested {Count} active postings.", payloads.Count);
        return payloads;
    }

    private async Task<IReadOnlyList<RawJobPayload>> FetchCompanyJobsAsync(
        HttpClient client,
        string company,
        CancellationToken cancellationToken)
    {
        var requestUri = $"posting-api/job-board/{Uri.EscapeDataString(company)}";
        using var response = await client.GetAsync(requestUri, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return [];
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<AshbyBoardResponse>(stream, SerializerOptions, cancellationToken);

        if (payload?.Jobs is not { Count: > 0 })
        {
            return [];
        }

        return payload.Jobs
            .Where(j => !string.IsNullOrWhiteSpace(j.Title))
            .Take(100)
            .Select(j => MapToPayload(j, company))
            .ToList();
    }

    private static RawJobPayload MapToPayload(AshbyJob job, string company)
    {
        var description = JobContentSanitizer.ToPlainText(job.DescriptionHtml ?? job.Title ?? "");
        var (responsibilities, requirements, benefits) = JobContentSanitizer.ExtractSections(description);
        var location = JobLocationParser.Parse(job.Location);

        return new RawJobPayload(
            ExternalJobId: job.Id ?? Guid.NewGuid().ToString(),
            Source: JobProviderKind.Ashby,
            Title: job.Title?.Trim() ?? "Software Engineer",
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
            EmploymentType: JobContentSanitizer.DetectEmploymentType(job.Title ?? ""),
            ExperienceLevel: JobSeniorityParser.Parse(job.Title ?? ""),
            MinSalary: null,
            MaxSalary: null,
            Currency: "USD",
            PayPeriod: "Yearly",
            PostedAt: DateTimeOffset.UtcNow,
            ExpiresAt: null,
            ApplyUrl: job.JobUrl ?? $"https://jobs.ashbyhq.com/{company}/{job.Id}",
            Language: "en",
            Skills: [".NET", "C#", "Angular", "TypeScript", "SQL"],
            Tags: [job.Department ?? "Engineering"],
            RawJsonMetadata: JsonSerializer.Serialize(new { provider = "ashby", company, job_id = job.Id }));
    }

    private sealed record AshbyBoardResponse
    {
        public IReadOnlyList<AshbyJob>? Jobs { get; init; }
    }

    private sealed record AshbyJob
    {
        public string? Id { get; init; }
        public string? Title { get; init; }
        public string? Location { get; init; }
        public string? Department { get; init; }
        public string? DescriptionHtml { get; init; }
        public string? JobUrl { get; init; }
    }
}
