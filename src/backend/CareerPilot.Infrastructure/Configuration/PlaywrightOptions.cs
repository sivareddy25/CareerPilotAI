namespace CareerPilot.Infrastructure.Configuration;

/// <summary>Browser automation settings. Not consumed until the Playwright adapter lands.</summary>
public sealed class PlaywrightOptions
{
    public const string SectionName = "Playwright";

    public bool Headless { get; set; } = true;

    public string Browser { get; set; } = "chromium";

    public int NavigationTimeoutSeconds { get; set; } = 30;

    /// <summary>Artificial delay between actions; useful only when debugging locally.</summary>
    public int SlowMoMilliseconds { get; set; }

    public string DownloadsPath { get; set; } = string.Empty;
}
