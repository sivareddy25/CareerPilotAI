using System.Text.RegularExpressions;
using CareerPilot.Application.Abstractions.Resumes;
using CareerPilot.Domain.Resumes;

namespace CareerPilot.Infrastructure.Resumes.Parsing;

/// <summary>
/// Turns flat extracted text into a structured <see cref="ResumeDocument"/>.
/// </summary>
/// <remarks>
/// <para>
/// Shared by the PDF and DOCX parsers, which differ only in how they get text out of a
/// container. Keeping the structural guesswork in one place means an improvement to
/// section detection benefits both, and the two cannot drift into disagreeing about
/// what a "Skills" heading looks like.
/// </para>
/// <para>
/// <b>This is heuristic and it is lossy.</b> It matches headings, contact patterns and
/// bullet markers — nothing more. It does not understand language, and by instruction
/// there is no AI and no OCR here. It will mis-split unconventional layouts, which is
/// exactly why anything it cannot place is preserved in
/// <see cref="ResumeDocument.UnparsedSections"/> rather than discarded: a user can move
/// a stray paragraph, but cannot recover one that was silently dropped.
/// </para>
/// </remarks>
internal static partial class ResumeTextHeuristics
{
    [GeneratedRegex(@"[\w.+-]+@[\w-]+\.[\w.-]+", RegexOptions.CultureInvariant)]
    private static partial Regex EmailPattern();

    /// <summary>Loose by design — international formats vary far more than any strict pattern allows.</summary>
    [GeneratedRegex(@"(\+?\d[\d\s().-]{7,}\d)", RegexOptions.CultureInvariant)]
    private static partial Regex PhonePattern();

    [GeneratedRegex(@"(?:https?://)?(?:www\.)?linkedin\.com/[\w\-/]+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex LinkedInPattern();

    [GeneratedRegex(@"(?:https?://)?(?:www\.)?github\.com/[\w\-/]+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex GitHubPattern();

    [GeneratedRegex(@"https?://[^\s]+", RegexOptions.CultureInvariant)]
    private static partial Regex UrlPattern();

    /// <summary>
    /// A date range such as "Jan 2019 - Present" or "2019 — 2021".
    /// </summary>
    /// <remarks>
    /// Captures the two sides as text rather than parsing them into dates. Resumes write
    /// dates in a dozen ways, and forcing them into a date type would reject valid input
    /// or invent precision the source never had.
    /// </remarks>
    [GeneratedRegex(
        @"^(?<start>(?:[A-Za-z]{3,9}\.?\s+)?\d{4})\s*[-–—to]+\s*(?<end>(?:[A-Za-z]{3,9}\.?\s+)?\d{4}|present|current|now)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex DateRangePattern();

    /// <summary>
    /// Heading synonyms per section.
    /// </summary>
    /// <remarks>
    /// Matched case-insensitively against short standalone lines. Kept as data so adding
    /// a synonym is a one-line change rather than new branching.
    /// </remarks>
    private static readonly (ResumeSectionKind Kind, string[] Headings)[] SectionHeadings =
    [
        (ResumeSectionKind.Summary,
            ["summary", "professional summary", "profile", "about", "objective", "career objective"]),
        (ResumeSectionKind.Experience,
            ["experience", "work experience", "professional experience", "employment", "employment history", "career history", "work history"]),
        (ResumeSectionKind.Education,
            ["education", "academic background", "academics", "qualifications", "education and training"]),
        (ResumeSectionKind.Skills,
            ["skills", "technical skills", "core competencies", "competencies", "technologies", "expertise"]),
        (ResumeSectionKind.Projects,
            ["projects", "personal projects", "selected projects", "portfolio"]),
        (ResumeSectionKind.Certifications,
            ["certifications", "certificates", "licenses", "accreditations", "awards"]),
    ];

    private static readonly char[] BulletMarkers = ['•', '·', '‣', '▪', '-', '–', '—', '*'];

    public static ResumeDocument Parse(string text, List<string> warnings)
    {
        var lines = text
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n')
            .Select(line => line.Trim())
            .ToList();

        var contact = ExtractContact(lines);
        var (sections, preamble) = SplitIntoSections(lines);

        if (sections.Count == 0)
        {
            warnings.Add("No standard section headings were found, so the text was kept as a single block.");
        }

        var document = new ResumeDocument
        {
            Contact = contact,
            Summary = JoinLines(Section(sections, ResumeSectionKind.Summary)),
            Experience = ParseExperience(Section(sections, ResumeSectionKind.Experience)),
            Education = ParseEducation(Section(sections, ResumeSectionKind.Education)),
            Skills = ParseSkills(Section(sections, ResumeSectionKind.Skills)),
            Projects = ParseProjects(Section(sections, ResumeSectionKind.Projects)),
            Certifications = ParseCertifications(Section(sections, ResumeSectionKind.Certifications)),
            // The preamble is the block before the first heading — normally the contact
            // details, which ExtractContact has already read. It is only surfaced when
            // it holds more than that, so a correctly parsed resume shows no warning
            // block while a mis-parsed one shows exactly what was not placed.
            UnparsedSections = preamble.Count > SkipLeadingContactLines
                ? [JoinLines(preamble.Skip(SkipLeadingContactLines).ToList()) ?? string.Empty]
                : [],
        };

        if (document.Experience.Count == 0 && document.Education.Count == 0)
        {
            warnings.Add("No experience or education entries were recognised. Review the imported resume before using it.");
        }

        return document;
    }

    /// <summary>
    /// Lines of the contact block that ExtractContact has already consumed, and which
    /// therefore should not be reported as unplaced content.
    /// </summary>
    private const int SkipLeadingContactLines = 3;

    private static List<string> Section(Dictionary<ResumeSectionKind, List<string>> sections, ResumeSectionKind kind) =>
        sections.TryGetValue(kind, out var lines) ? lines : [];

    /// <summary>
    /// Reads contact details from the top of the document.
    /// </summary>
    /// <remarks>
    /// Scans only the first 15 lines. Email and URL patterns match anywhere in a resume
    /// — a project link, a former employer's site — and bounding the search to the
    /// header is what keeps a project URL out of the contact block.
    /// </remarks>
    private static ResumeContact ExtractContact(List<string> lines)
    {
        var header = lines.Take(15).ToList();
        var headerText = string.Join('\n', header);

        var email = EmailPattern().Match(headerText).Value;
        var phone = PhonePattern().Match(headerText).Value.Trim();
        var linkedIn = LinkedInPattern().Match(headerText).Value;
        var gitHub = GitHubPattern().Match(headerText).Value;

        var website = UrlPattern()
            .Matches(headerText)
            .Select(match => match.Value)
            .FirstOrDefault(url =>
                !url.Contains("linkedin.com", StringComparison.OrdinalIgnoreCase)
                && !url.Contains("github.com", StringComparison.OrdinalIgnoreCase));

        // The name is taken as the first substantive line that is not itself contact
        // data — the near-universal convention in resume layouts.
        var name = header.FirstOrDefault(line =>
            line.Length is > 1 and < 60
            && !EmailPattern().IsMatch(line)
            && !PhonePattern().IsMatch(line)
            && !UrlPattern().IsMatch(line)
            && !IsSectionHeading(line, out _));

        // The line after the name is the headline when it is short and not contact data.
        var nameIndex = name is null ? -1 : header.IndexOf(name);
        var headline = nameIndex >= 0 && nameIndex + 1 < header.Count
            ? header[nameIndex + 1]
            : null;

        if (headline is not null &&
            (headline.Length > 90 || EmailPattern().IsMatch(headline) || PhonePattern().IsMatch(headline)))
        {
            headline = null;
        }

        return new ResumeContact
        {
            FullName = Blank(name),
            Headline = Blank(headline),
            Email = Blank(email),
            Phone = Blank(phone),
            LinkedIn = Blank(linkedIn),
            GitHub = Blank(gitHub),
            Website = Blank(website),
        };
    }

    /// <summary>
    /// Groups lines under the most recent recognised heading.
    /// </summary>
    /// <remarks>
    /// Lines before the first heading — the contact block, usually — land under a null
    /// key and become <see cref="ResumeDocument.UnparsedSections"/>.
    /// </remarks>
    private static (Dictionary<ResumeSectionKind, List<string>> Sections, List<string> Preamble)
        SplitIntoSections(List<string> lines)
    {
        // The pre-heading block is held in its own list rather than under a null key.
        // Dictionary rejects a null key outright, and an earlier version of this method
        // used ResumeSectionKind? for exactly that purpose — which threw on every PDF
        // and DOCX import, because every resume has content before its first heading.
        var sections = new Dictionary<ResumeSectionKind, List<string>>();
        var preamble = new List<string>();

        ResumeSectionKind? current = null;

        foreach (var line in lines)
        {
            if (IsSectionHeading(line, out var kind))
            {
                current = kind;
                sections.TryAdd(kind, []);
                continue;
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (current is { } active)
            {
                sections[active].Add(line);
            }
            else
            {
                preamble.Add(line);
            }
        }

        return (sections, preamble);
    }

    private static bool IsSectionHeading(string line, out ResumeSectionKind kind)
    {
        kind = default;

        // Headings are short and standalone. The length bound is what stops a sentence
        // that happens to contain "experience" from being read as a heading.
        if (line.Length is 0 or > 40)
        {
            return false;
        }

        var normalized = line
            .Trim()
            .TrimEnd(':')
            .Replace("_", string.Empty, StringComparison.Ordinal)
            .Trim()
            .ToLowerInvariant();

        foreach (var (sectionKind, headings) in SectionHeadings)
        {
            if (headings.Contains(normalized, StringComparer.Ordinal))
            {
                kind = sectionKind;
                return true;
            }
        }

        return false;
    }

    private static List<ResumeExperience> ParseExperience(List<string> lines)
    {
        var entries = new List<ResumeExperience>();
        ResumeExperience? current = null;
        var highlights = new List<string>();

        void Flush()
        {
            if (current is not null)
            {
                entries.Add(current with { Highlights = highlights.ToList() });
            }

            highlights = [];
        }

        foreach (var line in lines)
        {
            if (IsBullet(line))
            {
                highlights.Add(StripBullet(line));
                continue;
            }

            var dateMatch = DateRangePattern().Match(line);

            // A non-bullet line carrying a date range starts a new role. This is the
            // single most reliable structural signal in a resume body.
            if (dateMatch.Success)
            {
                Flush();

                var remainder = line[..dateMatch.Index].Trim(' ', ',', '|', '-', '–', '—');
                var (role, company) = SplitRoleAndCompany(remainder);
                var end = dateMatch.Groups["end"].Value;

                current = new ResumeExperience
                {
                    Role = Blank(role),
                    Company = Blank(company),
                    StartDate = Blank(dateMatch.Groups["start"].Value),
                    EndDate = IsOngoing(end) ? null : Blank(end),
                    IsCurrent = IsOngoing(end),
                };

                continue;
            }

            if (current is null)
            {
                var (role, company) = SplitRoleAndCompany(line);
                current = new ResumeExperience { Role = Blank(role), Company = Blank(company) };
                continue;
            }

            // Continuation prose under a role: kept as a highlight rather than dropped.
            highlights.Add(line);
        }

        Flush();

        return entries;
    }

    private static List<ResumeEducation> ParseEducation(List<string> lines)
    {
        var entries = new List<ResumeEducation>();

        foreach (var line in lines.Where(line => !IsBullet(line)))
        {
            var dateMatch = DateRangePattern().Match(line);
            var withoutDates = dateMatch.Success ? line[..dateMatch.Index].Trim(' ', ',', '|', '-') : line;
            var parts = SplitOnSeparators(withoutDates);

            entries.Add(new ResumeEducation
            {
                Degree = Blank(parts.ElementAtOrDefault(0)),
                Institution = Blank(parts.ElementAtOrDefault(1)),
                FieldOfStudy = Blank(parts.ElementAtOrDefault(2)),
                StartDate = dateMatch.Success ? Blank(dateMatch.Groups["start"].Value) : null,
                EndDate = dateMatch.Success ? Blank(dateMatch.Groups["end"].Value) : null,
            });
        }

        return entries;
    }

    /// <summary>
    /// Reads skills, honouring both list and "Category: a, b, c" layouts.
    /// </summary>
    private static List<ResumeSkill> ParseSkills(List<string> lines)
    {
        var skills = new List<ResumeSkill>();

        foreach (var line in lines)
        {
            var text = StripBullet(line);
            string? category = null;

            var colon = text.IndexOf(':', StringComparison.Ordinal);
            if (colon > 0 && colon < 40)
            {
                category = text[..colon].Trim();
                text = text[(colon + 1)..];
            }

            foreach (var name in text.Split([',', '|', ';', '•'], StringSplitOptions.RemoveEmptyEntries))
            {
                var trimmed = name.Trim();

                // Bounded to exclude a prose sentence that landed in this section.
                if (trimmed.Length is > 0 and <= 60)
                {
                    skills.Add(new ResumeSkill { Name = trimmed, Category = category });
                }
            }
        }

        return skills;
    }

    private static List<ResumeProject> ParseProjects(List<string> lines)
    {
        var entries = new List<ResumeProject>();
        ResumeProject? current = null;
        var highlights = new List<string>();

        void Flush()
        {
            if (current is not null)
            {
                entries.Add(current with { Highlights = highlights.ToList() });
            }

            highlights = [];
        }

        foreach (var line in lines)
        {
            if (IsBullet(line))
            {
                highlights.Add(StripBullet(line));
                continue;
            }

            Flush();

            var url = UrlPattern().Match(line).Value;
            var name = string.IsNullOrEmpty(url) ? line : line.Replace(url, string.Empty, StringComparison.Ordinal);
            var parts = SplitOnSeparators(name);

            current = new ResumeProject
            {
                Name = Blank(parts.ElementAtOrDefault(0)),
                Description = Blank(parts.ElementAtOrDefault(1)),
                Url = Blank(url),
            };
        }

        Flush();

        return entries;
    }

    private static List<ResumeCertification> ParseCertifications(List<string> lines)
    {
        var entries = new List<ResumeCertification>();

        foreach (var line in lines)
        {
            var text = StripBullet(line);
            var url = UrlPattern().Match(text).Value;

            if (!string.IsNullOrEmpty(url))
            {
                text = text.Replace(url, string.Empty, StringComparison.Ordinal);
            }

            var year = Regex.Match(text, @"\b(19|20)\d{2}\b", RegexOptions.CultureInvariant).Value;
            var parts = SplitOnSeparators(text);

            entries.Add(new ResumeCertification
            {
                Name = Blank(parts.ElementAtOrDefault(0)),
                Issuer = Blank(parts.ElementAtOrDefault(1)),
                IssuedDate = Blank(year),
                CredentialUrl = Blank(url),
            });
        }

        return entries;
    }

    /// <summary>
    /// Splits "Senior Engineer at Acme" or "Senior Engineer, Acme" into role and company.
    /// </summary>
    private static (string? Role, string? Company) SplitRoleAndCompany(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return (null, null);
        }

        var atIndex = line.IndexOf(" at ", StringComparison.OrdinalIgnoreCase);
        if (atIndex > 0)
        {
            return (line[..atIndex].Trim(), line[(atIndex + 4)..].Trim());
        }

        var parts = SplitOnSeparators(line);

        return (parts.ElementAtOrDefault(0), parts.ElementAtOrDefault(1));
    }

    private static string[] SplitOnSeparators(string line) =>
        line.Split(['|', ',', '·', '—', '–'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static bool IsBullet(string line) =>
        line.Length > 1 && BulletMarkers.Contains(line[0]);

    private static string StripBullet(string line) =>
        IsBullet(line) ? line[1..].Trim() : line.Trim();

    private static bool IsOngoing(string value) =>
        value.Equals("present", StringComparison.OrdinalIgnoreCase)
        || value.Equals("current", StringComparison.OrdinalIgnoreCase)
        || value.Equals("now", StringComparison.OrdinalIgnoreCase);

    private static string? Blank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? JoinLines(List<string> lines) =>
        lines.Count == 0 ? null : string.Join(' ', lines).Trim();
}
