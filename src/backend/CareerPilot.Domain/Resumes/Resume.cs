using CareerPilot.Domain.Entities;
using CareerPilot.Domain.Entities.Identity;

namespace CareerPilot.Domain.Resumes;

/// <summary>
/// A stored resume: one canonical document plus the metadata needed to own, name and
/// present it.
/// </summary>
/// <remarks>
/// <para>
/// The aggregate is intentionally thin. It carries identity, ownership and the selected
/// template; everything a resume actually <i>says</i> lives in
/// <see cref="Document"/>. Phase 8 will add editing behaviour on top of this same
/// document rather than replacing it.
/// </para>
/// <para>
/// <see cref="UserId"/> is the ownership anchor. Every query filters on it, and no
/// operation in this module accepts a resume id without also checking it belongs to the
/// caller — a resume is private, and an id is guessable enough that "knows the id"
/// cannot be the authorisation test.
/// </para>
/// </remarks>
public sealed class Resume : SoftDeleteEntity
{
    private Resume()
    {
        Title = string.Empty;
        Document = ResumeDocument.Empty();
    }

    private Resume(Guid userId, string title, ResumeDocument document, ResumeTemplateKey template)
    {
        UserId = userId;
        Title = title;
        Document = document;
        Template = template;
    }

    public Guid UserId { get; private set; }

    /// <summary>User-facing name, for example "Backend Engineer — 2026".</summary>
    public string Title { get; private set; }

    /// <summary>Presentation only. Changing it cannot change <see cref="Document"/>.</summary>
    public ResumeTemplateKey Template { get; private set; }

    /// <summary>The canonical content. Replaced wholesale, never mutated in place.</summary>
    public ResumeDocument Document { get; private set; }

    /// <summary>
    /// Format this resume was imported from, or <c>null</c> if it was created directly.
    /// </summary>
    /// <remarks>
    /// Retained because import is lossy for PDF and DOCX. Knowing a resume came from a
    /// PDF explains to both the user and a future maintainer why its sections may be
    /// imperfectly split.
    /// </remarks>
    public ResumeFormat? ImportedFrom { get; private set; }

    /// <summary>Original upload name, for display only. Never used as a storage path.</summary>
    public string? ImportedFileName { get; private set; }

    /// <summary>Timestamp of the last time this resume was exported.</summary>
    public DateTimeOffset? LastExportedAt { get; private set; }

    /// <summary>Total export count for telemetry and history.</summary>
    public int ExportCount { get; private set; }

    public User? User { get; private set; }

    public static Resume Create(
        Guid userId,
        string title,
        ResumeDocument document,
        ResumeTemplateKey template = ResumeTemplateKey.AtsFriendly)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(document);

        return new Resume(userId, title.Trim(), document, template);
    }

    public static Resume CreateFromImport(
        Guid userId,
        string title,
        ResumeDocument document,
        ResumeFormat sourceFormat,
        string? originalFileName)
    {
        var resume = Create(userId, title, document);

        resume.ImportedFrom = sourceFormat;
        resume.ImportedFileName = originalFileName;

        return resume;
    }

    /// <summary>
    /// Switches presentation. Explicitly does not touch <see cref="Document"/> — that
    /// invariant is what makes template switching safe to do freely in the preview.
    /// </summary>
    public void ApplyTemplate(ResumeTemplateKey template) => Template = template;

    public void Rename(string title)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        Title = title.Trim();
    }

    public void ReplaceDocument(ResumeDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        Document = document;
    }

    public void RecordExport()
    {
        LastExportedAt = DateTimeOffset.UtcNow;
        ExportCount++;
    }
}
