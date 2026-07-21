using CareerPilot.Domain.Resumes;

namespace CareerPilot.Application.Abstractions.Resumes;

/// <summary>
/// Presentation rules for one template.
/// </summary>
/// <remarks>
/// <para>
/// This is what makes "templates must not duplicate resume data" enforceable rather
/// than merely intended. A template is <i>data about how to draw</i> — fonts, spacing,
/// column count, section order — and contains no resume content at all. The renderer
/// walks the single <see cref="ResumeDocument"/> and consults this for how to lay it
/// out.
/// </para>
/// <para>
/// The same descriptor is served to the Angular client, so the five templates are
/// defined once and the web preview cannot drift from the PDF and DOCX output.
/// </para>
/// </remarks>
public sealed record ResumeTemplateDescriptor
{
    public required ResumeTemplateKey Key { get; init; }

    public required string Name { get; init; }

    public required string Description { get; init; }

    /// <summary>
    /// True when the layout is known to survive automated resume parsers. Surfaced in
    /// the gallery because it is the single most consequential thing about a template
    /// choice, and users cannot infer it from a thumbnail.
    /// </summary>
    public bool IsAtsSafe { get; init; }

    /// <summary>1 or 2. Two-column layouts look better and parse worse.</summary>
    public int Columns { get; init; } = 1;

    public required string AccentColor { get; init; }

    public required string HeadingFont { get; init; }

    public required string BodyFont { get; init; }

    /// <summary>Body size in points; headings are derived from it by the renderer.</summary>
    public double BaseFontSize { get; init; } = 10.5;

    /// <summary>
    /// Section order for this template. Ordering is presentation, so it lives here —
    /// an executive template leading with experience and a graduate one leading with
    /// education render the very same document.
    /// </summary>
    public required IReadOnlyList<ResumeSectionKind> SectionOrder { get; init; }
}

/// <summary>Sections a template may order. Mirrors the document's own sections.</summary>
public enum ResumeSectionKind
{
    Summary = 0,
    Experience = 1,
    Education = 2,
    Skills = 3,
    Projects = 4,
    Certifications = 5,
}

/// <summary>
/// The five templates, defined once and served to every renderer and to the client.
/// </summary>
public interface IResumeTemplateCatalog
{
    IReadOnlyList<ResumeTemplateDescriptor> All { get; }

    /// <summary>
    /// Returns the descriptor for <paramref name="key"/>, falling back to the ATS-safe
    /// default rather than throwing. A resume whose stored template was withdrawn must
    /// still render.
    /// </summary>
    ResumeTemplateDescriptor Get(ResumeTemplateKey key);
}
