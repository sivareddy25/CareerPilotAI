using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CareerPilot.Application.Jobs.Matching;
using CareerPilot.Domain.Entities.Profiles;
using CareerPilot.Domain.Jobs.Entities;
using CareerPilot.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CareerPilot.Infrastructure.Ai;

/// <summary>
/// Explains a job match with a local Ollama model, falling back to the deterministic score.
/// </summary>
/// <remarks>
/// <para>
/// The model is asked for strict JSON so the result maps onto <see cref="MatchExplanation"/>
/// without prose-scraping. Small local models do not always comply, so every failure mode —
/// unreachable Ollama, a timeout, non-JSON output, malformed JSON — funnels into
/// <see cref="Fallback"/>, which reconstructs a serviceable explanation from the deterministic
/// score that was computed anyway. The feature therefore never hard-fails; at worst it is less
/// eloquent and <see cref="MatchExplanation.GeneratedByAi"/> is false.
/// </para>
/// <para>
/// Only the matched skills, component details and the posting's own text are sent — data the user
/// already holds about themselves and a public posting. Nothing is persisted by the model; this
/// is a stateless local call.
/// </para>
/// </remarks>
internal sealed class OllamaJobMatchExplainer(
    IHttpClientFactory httpClientFactory,
    IOptions<OllamaOptions> options,
    ILogger<OllamaJobMatchExplainer> logger) : IJobMatchExplainer
{
    internal const string HttpClientName = "Ollama";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public async Task<MatchExplanation> ExplainAsync(
        Job job,
        UserProfile profile,
        MatchScore score,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(HttpClientName);

            var request = new OllamaGenerateRequest(
                options.Value.Model,
                BuildPrompt(job, profile, score),
                Stream: false,
                Format: "json",
                Options: new OllamaModelParameters(Temperature: 0.2));

            using var response = await client.PostAsJsonAsync("api/generate", request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>(SerializerOptions, cancellationToken);
            var parsed = ParseModelJson(body?.Response);

            if (parsed is null)
            {
                logger.LogWarning("Ollama returned no usable explanation JSON; using deterministic fallback.");
                return Fallback(score);
            }

            return new MatchExplanation(
                Strengths: Clean(parsed.Strengths) is { Count: > 0 } s ? s : Fallback(score).Strengths,
                Gaps: Clean(parsed.Gaps),
                Recommendation: string.IsNullOrWhiteSpace(parsed.Recommendation) ? score.Summary : parsed.Recommendation.Trim(),
                GeneratedByAi: true);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            logger.LogWarning(ex, "Ollama explanation failed; using deterministic fallback.");
            return Fallback(score);
        }
    }

    /// <summary>Deterministic explanation from the score, used whenever the model is unavailable.</summary>
    private static MatchExplanation Fallback(MatchScore score)
    {
        var strengths = score.Components
            .Where(c => c.Score >= 60)
            .Select(c => c.Detail)
            .ToList();

        if (score.MatchedSkills.Count > 0)
        {
            strengths.Insert(0, $"Matches your skills: {string.Join(", ", score.MatchedSkills)}.");
        }

        var gaps = score.Components
            .Where(c => c.Score < 50)
            .Select(c => c.Detail)
            .ToList();

        return new MatchExplanation(
            strengths.Count > 0 ? strengths : ["Some of your profile aligns with this posting."],
            gaps,
            score.Summary,
            GeneratedByAi: false);
    }

    private static string BuildPrompt(Job job, UserProfile profile, MatchScore score)
    {
        var skills = profile.Skills.Count > 0
            ? string.Join(", ", profile.Skills.Select(s => s.Name))
            : "(none provided)";

        // The posting can be long; the model only needs enough to reason about fit, and trimming
        // keeps the request within a small model's context and latency budget.
        var jobText = Truncate(job.Description, 2000);
        var requirements = Truncate(job.Requirements ?? string.Empty, 800);

        var builder = new StringBuilder();
        builder.AppendLine("You are a career assistant. Assess how well a candidate fits a job.");
        builder.AppendLine("Respond ONLY with JSON of this exact shape:");
        builder.AppendLine("{\"strengths\": [\"...\"], \"gaps\": [\"...\"], \"recommendation\": \"...\"}");
        builder.AppendLine("strengths: why the candidate fits. gaps: skills/requirements they seem to lack. recommendation: one sentence.");
        builder.AppendLine();
        builder.AppendLine($"CANDIDATE SKILLS: {skills}");
        builder.AppendLine($"CANDIDATE EXPERIENCE: {profile.YearsOfExperience?.ToString() ?? "unspecified"} years");
        builder.AppendLine($"DETERMINISTIC SCORE: {score.OverallScore}/100 ({score.Summary})");
        builder.AppendLine($"ALREADY-MATCHED SKILLS: {(score.MatchedSkills.Count > 0 ? string.Join(", ", score.MatchedSkills) : "none")}");
        builder.AppendLine();
        builder.AppendLine($"JOB TITLE: {job.Title}");
        builder.AppendLine($"JOB DESCRIPTION: {jobText}");

        if (requirements.Length > 0)
        {
            builder.AppendLine($"JOB REQUIREMENTS: {requirements}");
        }

        return builder.ToString();
    }

    private static ModelExplanationJson? ParseModelJson(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        // Even with format=json a small model can wrap the object in stray text; salvage the
        // outermost JSON object rather than discarding an otherwise-good answer.
        var start = raw.IndexOf('{');
        var end = raw.LastIndexOf('}');

        if (start < 0 || end <= start)
        {
            return null;
        }

        var json = raw[start..(end + 1)];

        try
        {
            return JsonSerializer.Deserialize<ModelExplanationJson>(json, SerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static List<string> Clean(IEnumerable<string>? values) =>
        (values ?? [])
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => v.Trim())
            .Take(6)
            .ToList();

    private static string Truncate(string text, int max) =>
        string.IsNullOrEmpty(text) || text.Length <= max ? text : text[..max];

    private sealed record OllamaGenerateRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("prompt")] string Prompt,
        [property: JsonPropertyName("stream")] bool Stream,
        [property: JsonPropertyName("format")] string Format,
        [property: JsonPropertyName("options")] OllamaModelParameters Options);

    private sealed record OllamaModelParameters(
        [property: JsonPropertyName("temperature")] double Temperature);

    private sealed record OllamaGenerateResponse(
        [property: JsonPropertyName("response")] string? Response);

    private sealed record ModelExplanationJson(
        [property: JsonPropertyName("strengths")] List<string>? Strengths,
        [property: JsonPropertyName("gaps")] List<string>? Gaps,
        [property: JsonPropertyName("recommendation")] string? Recommendation);
}
