using CareerPilot.Domain.Resumes;

namespace CareerPilot.Application.Resumes.Models;

/// <summary>
/// A resume with its full document. What the preview renders from.
/// </summary>
/// <remarks>
/// The document is returned as the domain record rather than remapped into a parallel
/// DTO tree. It is already an immutable value object with no secrets and no navigation
/// properties, so a second identical shape would add a maintenance burden and a place
/// for the two to disagree — the very duplication this module is meant to avoid.
/// </remarks>
public sealed record ResumeDto(
    Guid Id,
    string Title,
    ResumeTemplateKey Template,
    ResumeFormat? ImportedFrom,
    string? ImportedFileName,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    ResumeDocument Document)
{
    public static ResumeDto From(Resume resume) =>
        new(
            resume.Id,
            resume.Title,
            resume.Template,
            resume.ImportedFrom,
            resume.ImportedFileName,
            resume.CreatedAt,
            resume.UpdatedAt,
            resume.Document);
}

/// <summary>
/// Resume metadata without the document.
/// </summary>
/// <remarks>
/// The list view shows titles and dates, never content. Omitting the document keeps a
/// fifty-resume listing small instead of shipping fifty full documents to render six
/// lines of text each.
/// </remarks>
public sealed record ResumeSummaryDto(
    Guid Id,
    string Title,
    ResumeTemplateKey Template,
    ResumeFormat? ImportedFrom,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static ResumeSummaryDto From(Resume resume) =>
        new(
            resume.Id,
            resume.Title,
            resume.Template,
            resume.ImportedFrom,
            resume.CreatedAt,
            resume.UpdatedAt);
}

/// <summary>
/// Outcome of a multi-file import: one entry per file, in the order submitted.
/// </summary>
/// <remarks>
/// Reports success and failure together rather than failing the batch, so a single bad
/// file does not discard the rest. The wizard shows this per row.
/// </remarks>
public sealed record ResumeImportResultDto(
    int TotalFiles,
    int ImportedCount,
    IReadOnlyList<ResumeImportEntryDto> Results)
{
    public bool AllSucceeded => ImportedCount == TotalFiles;
}

public sealed record ResumeImportEntryDto(
    string FileName,
    bool Succeeded,
    Guid? ResumeId,
    string? Title,
    IReadOnlyList<string> Warnings,
    string? Error);
