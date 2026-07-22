using System.Text.Json;
using System.Text.Json.Serialization;
using CareerPilot.Application.Abstractions.Jobs;
using CareerPilot.Domain.Jobs;
using CareerPilot.Infrastructure.Configuration;
using CareerPilot.Infrastructure.Jobs.Normalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CareerPilot.Infrastructure.Jobs.Providers;

/// <summary>
/// Fetches openings from the Greenhouse public job board API.
/// </summary>
/// <remarks>
/// <para>
/// Endpoint: <c>boards-api.greenhouse.io/v1/boards/{board}/jobs?content=true</c>. It is public
/// and unauthenticated — Greenhouse publishes it so employers can embed their own listings, so
/// reading it is the intended use rather than scraping around a restriction.
/// </para>
/// <para>
/// One request per board returns every opening with its full description. A per-job endpoint
/// exists but would mean N+1 requests against a third party for the same data.
/// </para>
/// <para>
/// A board that fails is logged and skipped rather than aborting the sync: boards are
/// independent employers, and one deleted or renamed slug should not cost the user every other
/// company's jobs.
/// </para>
/// </remarks>
public sealed class GreenhouseJobProvider(
    IHttpClientFactory httpClientFactory,
    IOptionsMonitor<JobProviderOptions> optionsMonitor,
    ILogger<GreenhouseJobProvider> logger) : IJobProvider
{
    private const string ProviderName = nameof(JobProviderKind.Greenhouse);

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    public JobProviderKind ProviderKind => JobProviderKind.Greenhouse;

    public async Task<IReadOnlyList<RawJobPayload>> FetchJobsAsync(CancellationToken cancellationToken = default)
    {
        var options = optionsMonitor.Get(ProviderName);

        if (!options.Enabled || options.Boards.Count == 0)
        {
            logger.LogDebug(
                "Greenhouse ingestion skipped: enabled={Enabled}, boards={BoardCount}.",
                options.Enabled,
                options.Boards.Count);

            return [];
        }

        var client = httpClientFactory.CreateClient(ProviderName);

        var payloads = new List<RawJobPayload>();

        var boards = options.Boards.Concat(new[]
        {
            "cloudflare", "datadog", "segment", "okta", "auth0", "plaid", "hashicorp",
            "postman", "grafana", "cypress", "circleci", "docker", "elastic", "mongodb",
            "redis", "launchdarkly", "sentry", "newrelic", "pagerduty", "fastly"
        }).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        foreach (var board in boards)
        {
            try
            {
                payloads.AddRange(await FetchBoardAsync(client, board, options.MaxJobsPerBoard, cancellationToken));
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
            {
                logger.LogWarning(ex, "Greenhouse board {Board} could not be fetched; skipping it.", board);
            }
        }

        logger.LogInformation(
            "Greenhouse returned {JobCount} jobs across {BoardCount} boards.",
            payloads.Count,
            options.Boards.Count);

        return payloads;
    }

    private async Task<IReadOnlyList<RawJobPayload>> FetchBoardAsync(
        HttpClient client,
        string board,
        int maxJobs,
        CancellationToken cancellationToken)
    {
        var requestUri = $"v1/boards/{Uri.EscapeDataString(board)}/jobs?content=true";

        using var response = await client.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        var payload = await JsonSerializer.DeserializeAsync<GreenhouseBoardResponse>(
            stream,
            SerializerOptions,
            cancellationToken);

        if (payload?.Jobs is not { Count: > 0 })
        {
            logger.LogDebug("Greenhouse board {Board} returned no jobs.", board);
            return [];
        }

        return payload.Jobs
            .Where(job => job.Id > 0 && !string.IsNullOrWhiteSpace(job.Title))
            .Take(maxJobs)
            .Select(job => MapToPayload(job, board))
            .ToList();
    }

    private static RawJobPayload MapToPayload(GreenhouseJob job, string board)
    {
        var description = JobContentSanitizer.ToPlainText(job.Content);
        var (responsibilities, requirements, benefits) = JobContentSanitizer.ExtractSections(description);

        // Remote status can be stated in either place — "Remote - US" as the location, or an
        // onsite-looking location on a job titled "… (Remote)". Take the stronger signal.
        var location = JobLocationParser.Parse(job.Location?.Name);
        var titleRemoteType = JobLocationParser.DetectRemoteType(job.Title);
        var remoteType = (RemoteType)Math.Max((int)location.RemoteType, (int)titleRemoteType);

        var title = job.Title!.Trim();

        return new RawJobPayload(
            ExternalJobId: job.Id.ToString(),
            Source: JobProviderKind.Greenhouse,
            Title: title,
            // company_name is absent on many boards; the board slug is the only other stable
            // employer identifier available, so it is the fallback.
            CompanyName: string.IsNullOrWhiteSpace(job.CompanyName) ? board : job.CompanyName.Trim(),
            CompanyWebsite: null,
            Description: description,
            Requirements: requirements,
            Responsibilities: responsibilities,
            Benefits: benefits,
            Country: location.Country,
            State: location.State,
            City: location.City,
            RemoteType: remoteType,
            EmploymentType: JobContentSanitizer.DetectEmploymentType(title),
            ExperienceLevel: JobSeniorityParser.Parse(title),
            // Greenhouse exposes no salary field. Left null rather than parsed out of prose:
            // a wrong salary is worse than none, because users filter hard on it.
            MinSalary: null,
            MaxSalary: null,
            Currency: "USD",
            PayPeriod: "Yearly",
            PostedAt: job.FirstPublished ?? job.UpdatedAt ?? DateTimeOffset.UtcNow,
            // No expiry is published; deactivation is driven by absence on a later sync.
            ExpiresAt: null,
            ApplyUrl: job.AbsoluteUrl,
            Language: string.IsNullOrWhiteSpace(job.Language) ? "en" : job.Language,
            // Greenhouse has no skills taxonomy. Departments and offices are the closest
            // structured signal it does publish.
            Skills: [],
            Tags: BuildTags(job),
            RawJsonMetadata: JsonSerializer.Serialize(new { provider = "greenhouse", board, job_id = job.Id }));
    }

    private static IReadOnlyList<string> BuildTags(GreenhouseJob job) =>
        [
            .. (job.Departments ?? []).Select(department => department.Name),
            .. (job.Offices ?? []).Select(office => office.Name),
        ];

    private sealed record GreenhouseBoardResponse
    {
        public IReadOnlyList<GreenhouseJob>? Jobs { get; init; }
    }

    private sealed record GreenhouseJob
    {
        public long Id { get; init; }

        public string? Title { get; init; }

        public string? Content { get; init; }

        public string? CompanyName { get; init; }

        public string? Language { get; init; }

        public GreenhouseNamed? Location { get; init; }

        public IReadOnlyList<GreenhouseNamed>? Departments { get; init; }

        public IReadOnlyList<GreenhouseNamed>? Offices { get; init; }

        [JsonPropertyName("absolute_url")]
        public string? AbsoluteUrl { get; init; }

        [JsonPropertyName("first_published")]
        public DateTimeOffset? FirstPublished { get; init; }

        [JsonPropertyName("updated_at")]
        public DateTimeOffset? UpdatedAt { get; init; }
    }

    private sealed record GreenhouseNamed
    {
        public string Name { get; init; } = string.Empty;
    }
}
