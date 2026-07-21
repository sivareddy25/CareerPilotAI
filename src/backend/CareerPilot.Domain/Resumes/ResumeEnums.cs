namespace CareerPilot.Domain.Resumes;

/// <summary>
/// The presentation applied to a resume.
/// </summary>
/// <remarks>
/// A template selects a layout; it never changes the underlying
/// <see cref="ResumeDocument"/>. Switching template is therefore always lossless and
/// always reversible — no content can be gained or lost by choosing a different look.
/// </remarks>
public enum ResumeTemplateKey
{
    /// <summary>
    /// Single column, no graphics, conventional headings. The safe default: applicant
    /// tracking systems parse text linearly, and multi-column or graphical layouts are
    /// where they silently lose content.
    /// </summary>
    AtsFriendly = 0,

    Professional = 1,
    Executive = 2,
    Minimal = 3,
    Modern = 4,
    Corporate = 5,
}

/// <summary>
/// A file format the module reads or writes.
/// </summary>
public enum ResumeFormat
{
    /// <summary>Lossless. The only format that round-trips a document unchanged.</summary>
    Json = 0,

    Pdf = 1,
    Docx = 2,

    /// <summary>Browser print output format.</summary>
    Print = 3,
}

/// <summary>
/// Page dimensions for document export.
/// </summary>
public enum ResumePageSize
{
    A4 = 0,
    Letter = 1,
}

/// <summary>
/// Page margin options for document export.
/// </summary>
public enum ResumeMarginSize
{
    Normal = 0,
    Narrow = 1,
    Wide = 2,
}
