namespace CareerPilot.Infrastructure.Configuration;

/// <summary>
/// Local Ollama endpoint used for on-demand AI explanations.
/// </summary>
/// <remarks>
/// Local-first: the model runs on the user's own machine, so there is no API key and nothing
/// leaves the host. Only qualitative, on-demand features (the "why does this job fit" narrative)
/// call it — the match score itself is deterministic and never needs the model, so an unavailable
/// or slow Ollama degrades one panel rather than breaking job discovery.
/// </remarks>
public sealed class OllamaOptions
{
    public const string SectionName = "Ollama";

    public string Endpoint { get; set; } = "http://localhost:11434";

    public string Model { get; set; } = "llama3.2:3b";

    /// <summary>
    /// A small local model generating a paragraph can take tens of seconds on a cold load, so
    /// this is generous. The explanation is opt-in, not on the hot path.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 120;
}
