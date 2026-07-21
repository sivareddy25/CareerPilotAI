using System.Text;
using CareerPilot.Application.Abstractions.Resumes;
using CareerPilot.Application.Resumes;
using CareerPilot.Domain.Resumes;
using Microsoft.Extensions.Options;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace CareerPilot.Infrastructure.Resumes.Parsing;

/// <summary>
/// Extracts embedded text from a PDF.
/// </summary>
/// <remarks>
/// <para>
/// Uses PdfPig (Apache 2.0) purely as a text extractor. It reads text the PDF already
/// contains; it does not and cannot read text baked into images. A scanned resume
/// therefore yields nothing, and the import service reports that plainly rather than
/// storing an empty resume — OCR is explicitly out of scope for this phase.
/// </para>
/// <para>
/// <see cref="ContentOrderTextExtractor"/> is used rather than the default word
/// ordering because it follows the document's own content stream order, which for
/// single-column resumes is reading order. Multi-column PDFs still interleave, which is
/// the main reason the ATS-friendly template is a single column.
/// </para>
/// </remarks>
internal sealed class PdfResumeParser(IOptions<ResumeOptions> options) : IResumeParser
{
    private readonly ResumeOptions _options = options.Value;

    public ResumeFormat Format => ResumeFormat.Pdf;

    public Task<ResumeParseResult> ParseAsync(Stream content, CancellationToken cancellationToken = default)
    {
        var warnings = new List<string>();

        // PdfPig needs a seekable buffer and is synchronous, so the stream is copied
        // once up front rather than fought with.
        using var buffer = new MemoryStream();
        content.CopyTo(buffer);
        buffer.Seek(0, SeekOrigin.Begin);

        using var pdf = PdfDocument.Open(buffer);

        var builder = new StringBuilder();
        var truncated = false;

        foreach (var page in pdf.GetPages())
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Bounded against a document that expands into an enormous text stream; the
            // heuristics are line-oriented, so cost grows with length.
            if (builder.Length >= _options.MaxExtractedCharacters)
            {
                truncated = true;
                break;
            }

            builder.AppendLine(ContentOrderTextExtractor.GetText(page));
        }

        if (truncated)
        {
            warnings.Add("The document was unusually long and only the first part was imported.");
        }

        var text = builder.ToString();

        if (string.IsNullOrWhiteSpace(text))
        {
            // Distinguished from a parse failure: the file is a valid PDF, it simply
            // carries no selectable text.
            warnings.Add("This PDF contains no selectable text. It may be a scan or an image.");
            return Task.FromResult(new ResumeParseResult(ResumeDocument.Empty(), warnings));
        }

        var document = ResumeTextHeuristics.Parse(text, warnings);

        warnings.Add("PDF import is approximate. Check the imported sections before using this resume.");

        return Task.FromResult(new ResumeParseResult(document, warnings));
    }
}
