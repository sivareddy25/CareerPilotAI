namespace CareerPilot.Application.Resumes;

/// <summary>
/// Import and export limits.
/// </summary>
public sealed class ResumeOptions
{
    public const string SectionName = "Resumes";

    /// <summary>
    /// 5 MB per file. A text-bearing resume is far smaller; the headroom is for
    /// embedded fonts and images in DOCX. The point of the ceiling is that parsing
    /// allocates proportionally to input, so an unbounded upload is an unbounded
    /// allocation.
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024;

    /// <summary>
    /// Files accepted in a single import request.
    /// </summary>
    /// <remarks>
    /// Multi-file import is a requirement, so the bound is on the batch rather than on
    /// the request being single-file. Ten files at 5 MB is a 50 MB request, which is
    /// why the controller sets its own size limit to match.
    /// </remarks>
    public int MaxFilesPerImport { get; set; } = 10;

    /// <summary>
    /// Resumes one account may hold. Prevents an automated client from using import as
    /// unbounded storage.
    /// </summary>
    public int MaxResumesPerUser { get; set; } = 50;

    /// <summary>
    /// Characters of extracted text a parser will consider.
    /// </summary>
    /// <remarks>
    /// A defence against decompression bombs: a small DOCX can expand into an enormous
    /// text stream, and the heuristics are line-oriented, so cost grows with length.
    /// Truncation is reported as a warning rather than being silent.
    /// </remarks>
    public int MaxExtractedCharacters { get; set; } = 200_000;
}
