using CareerPilot.Domain.Resumes;

namespace CareerPilot.Application.Abstractions.Resumes;

/// <summary>Outcome of parsing one file.</summary>
/// <param name="Document">What was extracted. Never null — a failed parse yields an empty document.</param>
/// <param name="Warnings">
/// Non-fatal problems: sections that could not be identified, dates that could not be
/// read. Surfaced to the user rather than logged and forgotten, because heuristic
/// parsing is lossy and the user is the only one who can tell what is missing.
/// </param>
public sealed record ResumeParseResult(ResumeDocument Document, IReadOnlyList<string> Warnings)
{
    public static ResumeParseResult From(ResumeDocument document, params string[] warnings) =>
        new(document, warnings);
}

/// <summary>
/// Turns a file into a <see cref="ResumeDocument"/>.
/// </summary>
/// <remarks>
/// <para>
/// One implementation per source format, selected at runtime by
/// <see cref="IResumeParserRegistry"/>. Adding a format means adding a parser and
/// nothing else: no caller, command, or controller changes, because nothing above this
/// interface knows which formats exist.
/// </para>
/// <para>
/// Parsing is deliberately heuristic and deliberately not clever. No AI, no OCR — a
/// PDF with no embedded text yields an empty document and a warning saying so, rather
/// than silently producing nothing and appearing to succeed.
/// </para>
/// </remarks>
public interface IResumeParser
{
    /// <summary>The single format this parser handles.</summary>
    ResumeFormat Format { get; }

    /// <summary>
    /// Reads <paramref name="content"/> into a document.
    /// </summary>
    /// <remarks>
    /// Must not throw for malformed input — a corrupt file is an expected outcome of
    /// letting users upload files, and it belongs in <see cref="ResumeParseResult.Warnings"/>,
    /// not in an exception that becomes a 500.
    /// </remarks>
    Task<ResumeParseResult> ParseAsync(Stream content, CancellationToken cancellationToken = default);
}

/// <summary>
/// Resolves the parser for a format.
/// </summary>
public interface IResumeParserRegistry
{
    /// <summary>Returns the parser for <paramref name="format"/>, or null if unsupported.</summary>
    IResumeParser? For(ResumeFormat format);

    /// <summary>Formats that can currently be imported. Drives both validation and the UI.</summary>
    IReadOnlyCollection<ResumeFormat> SupportedFormats { get; }
}
