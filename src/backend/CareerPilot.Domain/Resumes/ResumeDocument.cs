using System.Text.Json.Serialization;

namespace CareerPilot.Domain.Resumes;

/// <summary>
/// The canonical resume. Every import produces one of these, every export consumes
/// one, and every template renders one.
/// </summary>
/// <remarks>
/// <para>
/// This type is the reason the module holds together. Import, export and rendering are
/// each a transformation to or from this shape, so a new source format needs only a
/// parser and a new output format only an exporter — neither has to know the other
/// exists, and neither multiplies against the five templates.
/// </para>
/// <para>
/// Immutable records throughout. A document handed to a renderer must come back
/// unchanged; if a template could mutate what it renders, switching templates would
/// silently alter the resume rather than just its presentation.
/// </para>
/// <para>
/// Deliberately a value object, not an entity graph. Resume sections are document
/// shaped — ordered, nested, sparsely populated, and subject to schema drift — and
/// modelling them as seven related tables would commit Phase 8 to a normalisation it
/// has not chosen yet. Persisted as a single JSONB column; see
/// <c>ResumeConfiguration</c>.
/// </para>
/// </remarks>
public sealed record ResumeDocument
{
    /// <summary>
    /// Schema version of this document.
    /// </summary>
    /// <remarks>
    /// Stamped on every document so that a future shape change can be detected and
    /// migrated on read. Without it, a stored JSON blob is unversioned and the only way
    /// to evolve the schema is to guess at what an old row contains.
    /// </remarks>
    public int SchemaVersion { get; init; } = CurrentSchemaVersion;

    public const int CurrentSchemaVersion = 1;

    public ResumeContact Contact { get; init; } = new();

    /// <summary>Free-text professional summary. Rendered as a paragraph by every template.</summary>
    public string? Summary { get; init; }

    public IReadOnlyList<ResumeExperience> Experience { get; init; } = [];

    public IReadOnlyList<ResumeEducation> Education { get; init; } = [];

    public IReadOnlyList<ResumeSkill> Skills { get; init; } = [];

    public IReadOnlyList<ResumeProject> Projects { get; init; } = [];

    public IReadOnlyList<ResumeCertification> Certifications { get; init; } = [];

    /// <summary>
    /// Text a parser recognised as belonging to the resume but could not place in a
    /// known section.
    /// </summary>
    /// <remarks>
    /// Kept rather than discarded. Heuristic parsing is lossy by nature, and silently
    /// dropping content means a user imports a resume and finds paragraphs missing with
    /// no indication why. Surfacing it lets them paste it where it belongs.
    /// </remarks>
    public IReadOnlyList<string> UnparsedSections { get; init; } = [];

    public static ResumeDocument Empty() => new();

    /// <summary>True when nothing beyond an empty contact block was extracted.</summary>
    /// <remarks>
    /// Ignored for serialisation. It is derived state, and letting it into the exported
    /// JSON would put a field in the portable format that re-import has to ignore —
    /// exactly the kind of drift the lossless round-trip is meant to avoid.
    /// </remarks>
    [JsonIgnore]
    public bool IsEmpty =>
        Contact.IsEmpty
        && string.IsNullOrWhiteSpace(Summary)
        && Experience.Count == 0
        && Education.Count == 0
        && Skills.Count == 0
        && Projects.Count == 0
        && Certifications.Count == 0;
}

public sealed record ResumeContact
{
    public string? FullName { get; init; }

    /// <summary>Role or tagline shown under the name, for example "Senior Engineer".</summary>
    public string? Headline { get; init; }

    public string? Email { get; init; }

    public string? Phone { get; init; }

    public string? Location { get; init; }

    public string? Website { get; init; }

    public string? LinkedIn { get; init; }

    public string? GitHub { get; init; }

    [JsonIgnore]
    public bool IsEmpty =>
        string.IsNullOrWhiteSpace(FullName)
        && string.IsNullOrWhiteSpace(Email)
        && string.IsNullOrWhiteSpace(Phone);
}

/// <summary>
/// A period of employment.
/// </summary>
/// <remarks>
/// Dates are strings, not <see cref="DateOnly"/>. Real resumes say "2019", "Jan 2019",
/// "Summer 2019" and "Present", and forcing those into a date type means either
/// rejecting valid input at import or inventing precision the source never had. The
/// value is displayed, never computed with; a phase that needs real date arithmetic can
/// add a parsed field alongside this one.
/// </remarks>
public sealed record ResumeExperience
{
    public string? Company { get; init; }
    public string? Role { get; init; }
    public string? Location { get; init; }
    public string? StartDate { get; init; }
    public string? EndDate { get; init; }
    public bool IsCurrent { get; init; }

    /// <summary>Achievement bullets. Order is meaningful and preserved.</summary>
    public IReadOnlyList<string> Highlights { get; init; } = [];
}

public sealed record ResumeEducation
{
    public string? Institution { get; init; }
    public string? Degree { get; init; }
    public string? FieldOfStudy { get; init; }
    public string? Location { get; init; }
    public string? StartDate { get; init; }
    public string? EndDate { get; init; }
    public string? Grade { get; init; }
}

/// <summary>
/// A skill, optionally grouped.
/// </summary>
/// <remarks>
/// <see cref="Category"/> lets templates group skills without the document storing the
/// grouping twice. Templates that do not group simply ignore it — presentation differs,
/// the data does not.
/// </remarks>
public sealed record ResumeSkill
{
    public string Name { get; init; } = string.Empty;
    public string? Category { get; init; }
}

public sealed record ResumeProject
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? Url { get; init; }
    public IReadOnlyList<string> Highlights { get; init; } = [];
}

public sealed record ResumeCertification
{
    public string? Name { get; init; }
    public string? Issuer { get; init; }
    public string? IssuedDate { get; init; }
    public string? ExpiryDate { get; init; }
    public string? CredentialUrl { get; init; }
}
