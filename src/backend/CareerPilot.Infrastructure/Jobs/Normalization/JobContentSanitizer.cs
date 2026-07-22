using System.Net;
using System.Text.RegularExpressions;
using CareerPilot.Domain.Jobs;

namespace CareerPilot.Infrastructure.Jobs.Normalization;

/// <summary>
/// Turns the HTML blob boards return as a job description into plain text, and pulls
/// structured hints out of it.
/// </summary>
/// <remarks>
/// Board APIs return the description as authored in their rich-text editor — HTML, usually
/// entity-encoded on top. Stored raw it breaks every consumer downstream: the AI matcher would
/// embed markup as if it were prose, and the UI would either render foreign HTML or show tags.
/// Sanitising once at ingestion means the rest of the system only ever handles text.
/// </remarks>
internal static partial class JobContentSanitizer
{
    [GeneratedRegex(@"<(script|style)\b[^>]*>.*?</\1>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex ScriptOrStyleBlock();

    [GeneratedRegex(@"<br\s*/?>|</(p|div|li|h[1-6]|tr)>", RegexOptions.IgnoreCase)]
    private static partial Regex BlockBoundary();

    [GeneratedRegex(@"<li\b[^>]*>", RegexOptions.IgnoreCase)]
    private static partial Regex ListItemStart();

    [GeneratedRegex("<[^>]+>")]
    private static partial Regex AnyTag();

    [GeneratedRegex(@"[ \t]+")]
    private static partial Regex HorizontalWhitespace();

    [GeneratedRegex(@"\n{3,}")]
    private static partial Regex ExcessBlankLines();

    /// <summary>
    /// Section headings boards conventionally use. Matched at line start so a passing
    /// mention inside a sentence does not split the description.
    /// </summary>
    [GeneratedRegex(@"^\s*(what you'?ll do|responsibilities|the role|your role|in this role|day to day)\b.*$",
        RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex ResponsibilitiesHeading();

    [GeneratedRegex(@"^\s*(requirements|qualifications|what we'?re looking for|who you are|about you|you have|minimum qualifications)\b.*$",
        RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex RequirementsHeading();

    [GeneratedRegex(@"^\s*(benefits|perks|what we offer|compensation|why join)\b.*$",
        RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex BenefitsHeading();

    [GeneratedRegex(@"\b(intern|internship)\b", RegexOptions.IgnoreCase)]
    private static partial Regex InternshipMarker();

    [GeneratedRegex(@"\b(part[\s-]?time)\b", RegexOptions.IgnoreCase)]
    private static partial Regex PartTimeMarker();

    [GeneratedRegex(@"\b(contract|contractor|freelance|consultant)\b", RegexOptions.IgnoreCase)]
    private static partial Regex ContractMarker();

    public static string ToPlainText(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        // Decoded twice deliberately: Greenhouse returns entity-encoded HTML, so one pass
        // yields markup and the second yields the characters that markup represents.
        var decoded = WebUtility.HtmlDecode(WebUtility.HtmlDecode(html));

        // Script and style content is dropped rather than flattened — otherwise its body
        // would survive tag-stripping and land in the description as gibberish.
        var text = ScriptOrStyleBlock().Replace(decoded, string.Empty);

        text = ListItemStart().Replace(text, "\n• ");
        text = BlockBoundary().Replace(text, "\n");
        text = AnyTag().Replace(text, string.Empty);

        text = WebUtility.HtmlDecode(text);
        text = HorizontalWhitespace().Replace(text, " ");
        text = ExcessBlankLines().Replace(text, "\n\n");

        return string.Join('\n', text.Split('\n').Select(line => line.Trim())).Trim();
    }

    /// <summary>
    /// Extracts the responsibilities / requirements / benefits sections when the posting uses
    /// recognisable headings. Returns nulls when it does not — a wrong split is worse than an
    /// absent one, because these feed résumé matching.
    /// </summary>
    public static (string? Responsibilities, string? Requirements, string? Benefits) ExtractSections(string plainText)
    {
        if (string.IsNullOrWhiteSpace(plainText))
        {
            return (null, null, null);
        }

        return (
            SectionAfter(plainText, ResponsibilitiesHeading()),
            SectionAfter(plainText, RequirementsHeading()),
            SectionAfter(plainText, BenefitsHeading()));
    }

    /// <summary>
    /// Text from the first match of <paramref name="heading"/> to whichever other known
    /// heading comes next, so sections do not swallow each other.
    /// </summary>
    private static string? SectionAfter(string text, Regex heading)
    {
        var match = heading.Match(text);

        if (!match.Success)
        {
            return null;
        }

        var start = match.Index + match.Length;

        if (start >= text.Length)
        {
            return null;
        }

        var remainder = text[start..];

        var nextHeading = new[] { ResponsibilitiesHeading(), RequirementsHeading(), BenefitsHeading() }
            .Select(pattern => pattern.Match(remainder))
            .Where(m => m.Success)
            .Select(m => m.Index)
            .DefaultIfEmpty(remainder.Length)
            .Min();

        var section = remainder[..nextHeading].Trim();

        return string.IsNullOrWhiteSpace(section) ? null : section;
    }

    /// <summary>
    /// Infers employment type from the job title. Boards in scope do not expose it as a field,
    /// and defaulting everything to full-time would make the filter meaningless.
    /// </summary>
    /// <remarks>
    /// Title only, deliberately. Descriptions were tried and are far too noisy: a full-time
    /// sales posting says "contract" while describing the work ("contract negotiation",
    /// "enterprise contracts") and was classified as contract employment. The title is where
    /// an employer states the arrangement on purpose, so a miss there means it is full-time —
    /// which is the correct default anyway.
    /// </remarks>
    public static EmploymentType DetectEmploymentType(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return EmploymentType.FullTime;
        }

        if (InternshipMarker().IsMatch(title)) return EmploymentType.Internship;
        if (PartTimeMarker().IsMatch(title)) return EmploymentType.PartTime;
        if (ContractMarker().IsMatch(title)) return EmploymentType.Contract;

        return EmploymentType.FullTime;
    }
}
