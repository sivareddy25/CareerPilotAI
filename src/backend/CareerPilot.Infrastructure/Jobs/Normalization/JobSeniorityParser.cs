using System.Text.RegularExpressions;
using CareerPilot.Domain.Jobs;

namespace CareerPilot.Infrastructure.Jobs.Normalization;

/// <summary>
/// Infers <see cref="ExperienceLevel"/> from a job title.
/// </summary>
/// <remarks>
/// Title-only by design. Descriptions mention seniority constantly and misleadingly — a
/// junior posting saying "you will work with senior engineers" would classify as senior on a
/// body-text match. The title is the one place the employer states the level deliberately.
/// </remarks>
internal static partial class JobSeniorityParser
{
    [GeneratedRegex(@"\b(chief|cto|ceo|cfo|vp|vice president|head of)\b", RegexOptions.IgnoreCase)]
    private static partial Regex ExecutiveTitle();

    [GeneratedRegex(@"\bdirector\b", RegexOptions.IgnoreCase)]
    private static partial Regex DirectorTitle();

    [GeneratedRegex(@"\b(lead|principal|staff|architect|manager)\b", RegexOptions.IgnoreCase)]
    private static partial Regex LeadTitle();

    [GeneratedRegex(@"\b(senior|sr\.?|snr)\b", RegexOptions.IgnoreCase)]
    private static partial Regex SeniorTitle();

    [GeneratedRegex(@"\b(junior|jr\.?|intern|graduate|entry[\s-]?level|associate|apprentice|new grad)\b", RegexOptions.IgnoreCase)]
    private static partial Regex EntryTitle();

    public static ExperienceLevel Parse(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return ExperienceLevel.MidLevel;
        }

        // Ordered most senior first: "Senior Director" and "Lead Architect" both match more
        // than one pattern, and the highest is the accurate one.
        if (ExecutiveTitle().IsMatch(title)) return ExperienceLevel.Executive;
        if (DirectorTitle().IsMatch(title)) return ExperienceLevel.Director;
        if (LeadTitle().IsMatch(title)) return ExperienceLevel.Lead;
        if (SeniorTitle().IsMatch(title)) return ExperienceLevel.SeniorLevel;
        if (EntryTitle().IsMatch(title)) return ExperienceLevel.EntryLevel;

        // Unmarked titles ("Software Engineer") are mid-level far more often than anything
        // else, and mid sits closest to the extremes if the guess is wrong.
        return ExperienceLevel.MidLevel;
    }
}
