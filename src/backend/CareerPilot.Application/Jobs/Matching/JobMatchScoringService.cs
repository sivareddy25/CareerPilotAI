using System.Text.RegularExpressions;
using CareerPilot.Domain.Entities.Profiles;
using CareerPilot.Domain.Jobs;
using CareerPilot.Domain.Jobs.Entities;
using CareerPilot.Domain.Jobs.ValueObjects;

namespace CareerPilot.Application.Jobs.Matching;

/// <summary>
/// Deterministic, explainable job-to-profile scoring.
/// </summary>
/// <remarks>
/// <para>
/// Weighted mean of independent dimensions. Weights reflect what actually predicts fit and what
/// data is actually available: skills dominate, then role and seniority, then location and
/// employment type. Salary is a bonus dimension that only participates when the posting states a
/// range — most board postings (Greenhouse included) do not, and inventing a salary signal from
/// absent data would be noise.
/// </para>
/// <para>
/// A dimension that cannot be evaluated (no target titles set, no salary on the posting) is
/// dropped from both the numerator and the denominator rather than scored zero, so a missing
/// input lowers confidence, not the score. The overall number is therefore "of the things we can
/// compare, how well does this fit", which is the honest reading.
/// </para>
/// <para>
/// Skills are matched against the posting's <em>text</em>, not a structured skills list, because
/// board APIs rarely publish one — Greenhouse never does. That means the score measures the
/// user's skills the posting mentions; the inverse (skills the posting wants that the user lacks)
/// needs the job's requirements parsed, which is the LLM explainer's job, not this calculator's.
/// </para>
/// </remarks>
internal sealed partial class JobMatchScoringService : IJobMatchScoringService
{
    private const double SkillsWeight = 0.45;
    private const double TitleWeight = 0.25;
    private const double SeniorityWeight = 0.15;
    private const double LocationWeight = 0.10;
    private const double EmploymentWeight = 0.05;
    private const double SalaryWeight = 0.15;

    // Words too generic to identify a role, stripped before comparing target titles to a posting
    // title so "Senior Engineer" vs "Staff Engineer" doesn't match purely on "engineer"… while
    // still matching on the meaningful token when one is shared.
    private static readonly HashSet<string> TitleStopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "senior", "junior", "lead", "staff", "principal", "the", "a", "an", "of", "and", "ii", "iii",
    };

    // Skill names that are also ordinary English words. Matched case-sensitively against the
    // posting so the language "Go" is found but the verb "go" is not — otherwise these single
    // short tokens match almost every description and inflate the skills score wholesale.
    private static readonly HashSet<string> AmbiguousSkills = new(StringComparer.OrdinalIgnoreCase)
    {
        "Go", "R", "C", "D", "Rust", "Swift", "Scala",
    };

    [GeneratedRegex(@"[a-z0-9#+.]+", RegexOptions.IgnoreCase)]
    private static partial Regex WordToken();

    public MatchScore? Score(Job job, UserProfile profile)
    {
        ArgumentNullException.ThrowIfNull(job);
        ArgumentNullException.ThrowIfNull(profile);

        var profileSkills = profile.Skills.Select(s => s.Name).Where(n => !string.IsNullOrWhiteSpace(n)).ToList();
        var hasTargetTitles = !string.IsNullOrWhiteSpace(profile.TargetJobTitles);

        if (profileSkills.Count == 0 && !hasTargetTitles)
        {
            return null;
        }

        var components = new List<MatchComponent>();

        var haystack = $"{job.Title}\n{job.Description}\n{job.Requirements}\n{job.Responsibilities}";
        var matchedSkills = MatchSkills(profileSkills, haystack);

        if (profileSkills.Count > 0)
        {
            var skillPct = (int)Math.Round(100.0 * matchedSkills.Count / profileSkills.Count);
            components.Add(new MatchComponent(
                "Skills",
                skillPct,
                SkillsWeight,
                matchedSkills.Count == 0
                    ? "None of your skills are mentioned in this posting."
                    : $"{matchedSkills.Count} of your {profileSkills.Count} skills matched: {string.Join(", ", matchedSkills)}."));
        }

        if (hasTargetTitles)
        {
            var (titleScore, titleDetail) = ScoreTitle(profile.TargetJobTitles!, job.Title);
            components.Add(new MatchComponent("Role", titleScore, TitleWeight, titleDetail));
        }

        components.Add(ScoreSeniority(profile.YearsOfExperience, job.ExperienceLevel));
        components.Add(ScoreLocation(profile, job.Location));

        if (profile.PreferredEmploymentType is { } preferredType)
        {
            var match = preferredType == job.EmploymentType;
            components.Add(new MatchComponent(
                "Employment type",
                match ? 100 : 0,
                EmploymentWeight,
                match ? $"Matches your preferred {preferredType}." : $"Posting is {job.EmploymentType}, you prefer {preferredType}."));
        }

        if (profile.DesiredSalaryAmount is { } desired && job.Salary.MaxSalary is { } jobMax && jobMax > 0)
        {
            components.Add(ScoreSalary(desired, profile.DesiredSalaryCurrency, job.Salary));
        }

        var overall = WeightedMean(components);
        var summary = BuildSummary(overall, matchedSkills, profileSkills.Count);

        return new MatchScore(overall, matchedSkills, components, summary);
    }

    private static int WeightedMean(IReadOnlyList<MatchComponent> components)
    {
        var totalWeight = components.Sum(c => c.Weight);

        if (totalWeight <= 0)
        {
            return 0;
        }

        var weighted = components.Sum(c => c.Score * c.Weight);
        return (int)Math.Round(Math.Clamp(weighted / totalWeight, 0, 100));
    }

    /// <summary>Profile skills that appear as whole tokens in the posting text.</summary>
    private static List<string> MatchSkills(IReadOnlyList<string> skills, string haystack)
    {
        var rawTokens = WordToken().Matches(haystack).Select(m => m.Value).ToList();

        // Two token sets: case-insensitive for normal skills, case-sensitive for the ambiguous
        // ones. Whole-token comparison rather than substring — substring matching reports "Java"
        // for a "JavaScript" posting. The tokenizer keeps #, + and . so "C#", "C++" and ".NET"
        // survive as single tokens on both sides.
        var tokens = rawTokens.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var casedTokens = rawTokens.ToHashSet(StringComparer.Ordinal);

        return skills
            .Where(skill => SkillMatches(skill, tokens, casedTokens))
            .ToList();
    }

    private static bool SkillMatches(string skill, HashSet<string> tokens, HashSet<string> casedTokens)
    {
        var skillTokens = SkillTokens(skill).ToList();

        if (skillTokens.Count == 0)
        {
            return false;
        }

        // A single-token skill that collides with an English word must appear in the posting with
        // its skill capitalisation ("Go", "GO", "Rust"), not as the lowercase common word — no
        // matter how the user typed it in their profile.
        if (skillTokens.Count == 1 && AmbiguousSkills.Contains(skillTokens[0]))
        {
            var word = skillTokens[0];
            var titleCase = char.ToUpperInvariant(word[0]) + word[1..].ToLowerInvariant();
            return casedTokens.Contains(titleCase) || casedTokens.Contains(word.ToUpperInvariant());
        }

        return skillTokens.All(tokens.Contains);
    }

    private static IEnumerable<string> SkillTokens(string skill) =>
        WordToken().Matches(skill).Select(m => m.Value);

    private static (int Score, string Detail) ScoreTitle(string targetTitles, string jobTitle)
    {
        var jobWords = MeaningfulWords(jobTitle);

        // Any configured target title shares a meaningful word with the posting title → role hit.
        // Titles are comma- or newline-separated free text from the user.
        var targets = targetTitles.Split([',', '\n', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var target in targets)
        {
            var targetWords = MeaningfulWords(target);
            var shared = targetWords.Intersect(jobWords, StringComparer.OrdinalIgnoreCase).ToList();

            if (shared.Count > 0)
            {
                var strength = (int)Math.Round(100.0 * shared.Count / Math.Max(1, targetWords.Count));
                return (Math.Clamp(strength, 40, 100), $"Role overlaps your target \"{target}\" on: {string.Join(", ", shared)}.");
            }
        }

        return (0, $"Posting title \"{jobTitle}\" does not overlap your target roles.");
    }

    private static List<string> MeaningfulWords(string text) =>
        WordToken().Matches(text)
            .Select(m => m.Value)
            .Where(w => w.Length > 1 && !TitleStopWords.Contains(w))
            .ToList();

    private static MatchComponent ScoreSeniority(int? years, ExperienceLevel jobLevel)
    {
        if (years is not { } y)
        {
            // No stated experience — treat as neutral rather than penalising, but at reduced
            // confidence so it cannot dominate. Half marks is the honest "unknown".
            return new MatchComponent("Seniority", 50, SeniorityWeight, "Add your years of experience for a seniority match.");
        }

        var expected = ExpectedLevel(y);
        var distance = Math.Abs((int)expected - (int)jobLevel);

        // Exact band = 100, one off = 65, two off = 30, further = 0. A junior applying to a
        // director role should score low here without zeroing the whole match.
        var score = distance switch
        {
            0 => 100,
            1 => 65,
            2 => 30,
            _ => 0,
        };

        return new MatchComponent(
            "Seniority",
            score,
            SeniorityWeight,
            $"{y} yrs maps to {expected}; posting is {jobLevel}.");
    }

    private static ExperienceLevel ExpectedLevel(int years) => years switch
    {
        <= 1 => ExperienceLevel.EntryLevel,
        <= 4 => ExperienceLevel.MidLevel,
        <= 8 => ExperienceLevel.SeniorLevel,
        <= 12 => ExperienceLevel.Lead,
        _ => ExperienceLevel.Executive,
    };

    private static MatchComponent ScoreLocation(UserProfile profile, Location location)
    {
        var preferredRemote = profile.PreferredRemoteType;
        var remoteMatch = preferredRemote is null || preferredRemote == location.RemoteType || location.RemoteType == RemoteType.Remote;

        var countryMatch = string.IsNullOrWhiteSpace(profile.Country)
            || string.Equals(profile.Country, location.Country, StringComparison.OrdinalIgnoreCase)
            || location.RemoteType == RemoteType.Remote;

        var score = (remoteMatch, countryMatch) switch
        {
            (true, true) => 100,
            (true, false) => 55,
            (false, true) => 55,
            _ => 15,
        };

        return new MatchComponent(
            "Location",
            score,
            LocationWeight,
            $"{location.DisplayLocation}" +
            (preferredRemote is { } pr ? $" vs your {pr} preference." : "."));
    }

    private static MatchComponent ScoreSalary(decimal desired, string? desiredCurrency, SalaryRange salary)
    {
        // Only comparable in the same currency; a cross-currency comparison without FX would be
        // wrong more often than right, so it is treated as neutral.
        if (!string.IsNullOrWhiteSpace(desiredCurrency)
            && !string.Equals(desiredCurrency, salary.Currency, StringComparison.OrdinalIgnoreCase))
        {
            return new MatchComponent("Salary", 50, SalaryWeight, $"Posting pays in {salary.Currency}; your target is in {desiredCurrency}.");
        }

        var jobMax = salary.MaxSalary!.Value;

        // Meets or beats target = 100. Below target scales linearly down to 0 at half the target,
        // so a posting paying far below expectations is clearly penalised.
        var ratio = (double)(jobMax / desired);
        var score = ratio >= 1 ? 100 : (int)Math.Round(Math.Clamp((ratio - 0.5) / 0.5, 0, 1) * 100);

        return new MatchComponent(
            "Salary",
            score,
            SalaryWeight,
            $"Posting tops out at {jobMax:N0} {salary.Currency} against your {desired:N0} target.");
    }

    private static string BuildSummary(int overall, IReadOnlyList<string> matchedSkills, int totalSkills)
    {
        var band = overall switch
        {
            >= 85 => "Strong match",
            >= 70 => "Good match",
            >= 50 => "Partial match",
            _ => "Weak match",
        };

        return totalSkills == 0
            ? band
            : $"{band} — {matchedSkills.Count}/{totalSkills} skills.";
    }
}
