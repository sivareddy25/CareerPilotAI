using CareerPilot.Application.Abstractions.Resumes;
using CareerPilot.Application.Exceptions;
using CareerPilot.Domain.Resumes;
using Microsoft.Extensions.Logging;

namespace CareerPilot.Application.Resumes.Services;

/// <summary>
/// Renders a stored resume into a downloadable file.
/// </summary>
/// <remarks>
/// A thin coordinator over <see cref="IResumeExporterRegistry"/>. The value it adds is
/// the file-name safety work below — every exporter would otherwise have to repeat it,
/// and one forgetting would be a header-injection bug.
/// </remarks>
public sealed class ResumeExportService(
    IResumeExporterRegistry exporters,
    ILogger<ResumeExportService> logger)
{
    public async Task<ResumeExportResult> ExportAsync(
        Resume resume,
        ResumeFormat format,
        ResumeTemplateKey? templateOverride,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(resume);

        // Print is produced by the browser from the live preview; there is deliberately
        // no server-side exporter for it, so asking for one is a client error.
        if (format == ResumeFormat.Print)
        {
            throw new UnsupportedResumeFormatException(
                "Print output is produced in the browser, not by the server.");
        }

        var exporter = exporters.For(format)
            ?? throw new UnsupportedResumeFormatException($"{format} export is not available.");

        // The override lets the export dialog preview a template without committing it
        // to the stored resume — choosing a look to export in is not the same as
        // changing the resume's template.
        var template = templateOverride ?? resume.Template;

        var result = await exporter.ExportAsync(
            resume.Document,
            template,
            SafeFileName(resume.Title),
            cancellationToken);

        logger.LogInformation(
            "Resume exported. ResumeId: {ResumeId}, Format: {Format}, Template: {Template}",
            resume.Id,
            format,
            template);

        return result;
    }

    /// <summary>
    /// Reduces a user-supplied title to something safe for a Content-Disposition header
    /// and a file system.
    /// </summary>
    /// <remarks>
    /// The title is free text the user chose. Placed unescaped into a download header it
    /// could inject CR/LF and forge response headers, or carry path separators that
    /// escape a directory when the browser saves it. Restricting to a known-good set is
    /// safer than trying to enumerate what to strip.
    /// </remarks>
    private static string SafeFileName(string title)
    {
        var cleaned = new string(title
            .Select(c => char.IsLetterOrDigit(c) || c is '-' or '_' or ' ' ? c : '-')
            .ToArray())
            .Trim()
            .Replace(' ', '-');

        while (cleaned.Contains("--", StringComparison.Ordinal))
        {
            cleaned = cleaned.Replace("--", "-", StringComparison.Ordinal);
        }

        cleaned = cleaned.Trim('-');

        // Bounded so the header stays well inside limits, and never empty — a resume
        // titled entirely in symbols would otherwise produce a nameless download.
        return string.IsNullOrWhiteSpace(cleaned)
            ? "resume"
            : cleaned[..Math.Min(cleaned.Length, 80)];
    }
}
