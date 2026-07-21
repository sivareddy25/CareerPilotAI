namespace CareerPilot.Infrastructure.Configuration;

/// <summary>
/// LLM provider settings. Not consumed until the AI adapter lands.
/// <see cref="ApiKey"/> must come from a secret store, never from appsettings.
/// </summary>
public sealed class OpenAIOptions
{
    public const string SectionName = "OpenAI";

    public string ApiKey { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = string.Empty;

    public int TimeoutSeconds { get; set; } = 100;

    public int MaxRetries { get; set; } = 2;
}
