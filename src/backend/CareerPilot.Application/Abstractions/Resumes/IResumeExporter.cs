using CareerPilot.Domain.Resumes;

namespace CareerPilot.Application.Abstractions.Resumes;

/// <summary>A rendered resume, ready to stream to the client.</summary>
/// <param name="Content">
/// The bytes. Materialised rather than streamed because every exporter here builds its
/// output in memory anyway, and a resume is measured in kilobytes.
/// </param>
public sealed record ResumeExportResult(byte[] Content, string ContentType, string FileName);

/// <summary>
/// Renders a <see cref="ResumeDocument"/> into a file format.
/// </summary>
/// <remarks>
/// <para>
/// The counterpart to <see cref="IResumeParser"/>, and the second half of what keeps
/// this module from becoming a matrix. Formats and templates are orthogonal: an
/// exporter is handed the template to apply, so five templates and three formats need
/// eight implementations, not fifteen.
/// </para>
/// <para>
/// An exporter renders. It never reads or writes the database, and never decides
/// whether the caller is allowed the resume — that is settled before it is invoked.
/// </para>
/// </remarks>
public interface IResumeExporter
{
    ResumeFormat Format { get; }

    Task<ResumeExportResult> ExportAsync(
        ResumeDocument document,
        ResumeTemplateKey template,
        string fileNameWithoutExtension,
        CancellationToken cancellationToken = default);
}

public interface IResumeExporterRegistry
{
    IResumeExporter? For(ResumeFormat format);

    /// <summary>
    /// Formats that can be exported. Excludes <see cref="ResumeFormat.Print"/>, which is
    /// produced by the browser from the live preview rather than by the server.
    /// </summary>
    IReadOnlyCollection<ResumeFormat> SupportedFormats { get; }
}
