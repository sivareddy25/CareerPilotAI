using CareerPilot.Application.Abstractions.Resumes;
using CareerPilot.Application.Abstractions.Storage;
using CareerPilot.Application.Exceptions;
using CareerPilot.Domain.Resumes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CareerPilot.Application.Resumes.Services;

/// <summary>Result of importing one file within a batch.</summary>
public sealed record ResumeImportItemResult(
    string FileName,
    bool Succeeded,
    Guid? ResumeId,
    string? Title,
    IReadOnlyList<string> Warnings,
    string? Error);

/// <summary>
/// Validates and parses uploaded files.
/// </summary>
/// <remarks>
/// <para>
/// Owns everything that must hold before a file becomes a resume: size, declared
/// format, magic bytes, and text extraction. It deliberately does not persist anything
/// — the command handler does that, so this stays testable without a database and the
/// transaction boundary lives with the operation that owns it.
/// </para>
/// <para>
/// A batch import is partially successful by design. One corrupt file among ten must
/// not discard the other nine, so each file is reported independently and the caller
/// decides what to do about failures.
/// </para>
/// </remarks>
public sealed class ResumeImportService(
    IResumeParserRegistry parsers,
    IOptions<ResumeOptions> options,
    ILogger<ResumeImportService> logger)
{
    private readonly ResumeOptions _options = options.Value;

    /// <summary>
    /// Leading bytes for the formats accepted, checked against the file's real content.
    /// </summary>
    /// <remarks>
    /// The declared content type and the file extension both come from the client and
    /// prove nothing. PDFs begin "%PDF"; DOCX is a ZIP container so begins "PK". JSON
    /// has no signature and is validated by parsing it instead.
    /// </remarks>
    private static readonly byte[] PdfSignature = "%PDF"u8.ToArray();
    private static readonly byte[] ZipSignature = [0x50, 0x4B];

    /// <summary>
    /// Parses one file, returning a document or an explanation.
    /// </summary>
    /// <remarks>
    /// Never throws for bad input. Import is a bulk operation over user-supplied files;
    /// an exception per malformed file would abort the batch and lose the good ones.
    /// </remarks>
    public async Task<(ResumeDocument? Document, ResumeFormat Format, IReadOnlyList<string> Warnings, string? Error)>
        ParseAsync(FileUploadRequest file, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(file);

        if (file.Length <= 0)
        {
            return (null, default, [], "The file is empty.");
        }

        if (file.Length > _options.MaxFileSizeBytes)
        {
            var megabytes = _options.MaxFileSizeBytes / (1024 * 1024);
            return (null, default, [], $"The file is larger than {megabytes} MB.");
        }

        var format = await DetectFormatAsync(file, cancellationToken);

        if (format is null)
        {
            return (null, default, [], "Only PDF, DOCX and JSON resumes can be imported.");
        }

        var parser = parsers.For(format.Value);

        if (parser is null)
        {
            return (null, format.Value, [], $"{format} import is not available.");
        }

        try
        {
            file.Content.Seek(0, SeekOrigin.Begin);

            var result = await parser.ParseAsync(file.Content, cancellationToken);

            if (result.Document.IsEmpty)
            {
                // A PDF of scanned images parses to nothing. Saying so is better than
                // storing a blank resume that looks like a bug — and OCR, which would
                // be the fix, is explicitly out of scope.
                return (
                    null,
                    format.Value,
                    result.Warnings,
                    "No text could be read from this file. If it is a scan or an image, export a text-based copy and try again.");
            }

            return (result.Document, format.Value, result.Warnings, null);
        }
        catch (Exception exception)
        {
            // The file name is user-supplied and goes in the log only as a structured
            // value; the file's contents never do.
            logger.LogWarning(
                exception,
                "Resume parsing failed. Format: {Format}, FileName: {FileName}",
                format,
                file.FileName);

            return (null, format.Value, [], "The file could not be read. It may be corrupt or password protected.");
        }
    }

    public void EnsureBatchIsAcceptable(int fileCount)
    {
        if (fileCount <= 0)
        {
            throw new UnsupportedResumeFormatException("Attach at least one file to import.");
        }

        if (fileCount > _options.MaxFilesPerImport)
        {
            throw new UnsupportedResumeFormatException(
                $"You can import up to {_options.MaxFilesPerImport} files at once.");
        }
    }

    public void EnsureQuotaAllows(int existingCount, int incomingCount)
    {
        if (existingCount + incomingCount > _options.MaxResumesPerUser)
        {
            throw new ResumeQuotaExceededException(_options.MaxResumesPerUser);
        }
    }

    /// <summary>
    /// Determines the real format from the file's leading bytes.
    /// </summary>
    /// <remarks>
    /// Content sniffing rather than trusting the extension. A caller can name anything
    /// ".pdf"; only the bytes decide which parser is safe to hand it to.
    /// </remarks>
    private static async Task<ResumeFormat?> DetectFormatAsync(
        FileUploadRequest file,
        CancellationToken cancellationToken)
    {
        if (!file.Content.CanSeek)
        {
            return null;
        }

        var header = new byte[8];
        file.Content.Seek(0, SeekOrigin.Begin);
        var read = await file.Content.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false, cancellationToken);
        file.Content.Seek(0, SeekOrigin.Begin);

        if (read >= PdfSignature.Length && header.AsSpan(0, PdfSignature.Length).SequenceEqual(PdfSignature))
        {
            return ResumeFormat.Pdf;
        }

        // Any ZIP could be here; the DOCX parser verifies the OOXML parts and reports a
        // clear error if the container is a ZIP of something else.
        if (read >= ZipSignature.Length && header.AsSpan(0, ZipSignature.Length).SequenceEqual(ZipSignature))
        {
            return ResumeFormat.Docx;
        }

        // JSON has no signature. Leading whitespace is skipped, then an object or array
        // opener is the only remaining possibility worth trying.
        for (var i = 0; i < read; i++)
        {
            var b = header[i];
            if (b is (byte)' ' or (byte)'\r' or (byte)'\n' or (byte)'\t' or 0xEF or 0xBB or 0xBF)
            {
                continue;
            }

            return b is (byte)'{' or (byte)'[' ? ResumeFormat.Json : null;
        }

        return null;
    }
}
