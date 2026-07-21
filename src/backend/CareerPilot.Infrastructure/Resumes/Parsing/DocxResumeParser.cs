using System.Text;
using CareerPilot.Application.Abstractions.Resumes;
using CareerPilot.Application.Resumes;
using CareerPilot.Domain.Resumes;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Options;

namespace CareerPilot.Infrastructure.Resumes.Parsing;

/// <summary>
/// Extracts text from a Word document.
/// </summary>
/// <remarks>
/// <para>
/// Reads the OOXML body directly with the Open XML SDK (MIT). DOCX import is materially
/// more accurate than PDF because paragraph boundaries and list formatting survive in
/// the markup — a PDF has lost that structure by the time it is written.
/// </para>
/// <para>
/// Text is taken from paragraphs and tables. Tables matter: a great many resume
/// templates lay their contact block or skills grid out in an invisible table, and
/// ignoring them would silently drop exactly the fields users most expect to survive.
/// </para>
/// </remarks>
internal sealed class DocxResumeParser(IOptions<ResumeOptions> options) : IResumeParser
{
    private readonly ResumeOptions _options = options.Value;

    public ResumeFormat Format => ResumeFormat.Docx;

    public Task<ResumeParseResult> ParseAsync(Stream content, CancellationToken cancellationToken = default)
    {
        var warnings = new List<string>();

        using var buffer = new MemoryStream();
        content.CopyTo(buffer);
        buffer.Seek(0, SeekOrigin.Begin);

        using var word = WordprocessingDocument.Open(buffer, isEditable: false);

        var body = word.MainDocumentPart?.Document?.Body;

        if (body is null)
        {
            // A valid ZIP that is not a Word document reaches here, because format
            // detection can only see the ZIP signature.
            warnings.Add("The file is not a readable Word document.");
            return Task.FromResult(new ResumeParseResult(ResumeDocument.Empty(), warnings));
        }

        var builder = new StringBuilder();
        var truncated = false;

        foreach (var element in body.Elements())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (builder.Length >= _options.MaxExtractedCharacters)
            {
                truncated = true;
                break;
            }

            switch (element)
            {
                case Paragraph paragraph:
                    AppendParagraph(builder, paragraph);
                    break;

                case Table table:
                    // Cells become their own lines. Flattening a row into one line would
                    // merge a label and its value into text the heuristics cannot split.
                    foreach (var cell in table.Descendants<TableCell>())
                    {
                        foreach (var paragraph in cell.Elements<Paragraph>())
                        {
                            AppendParagraph(builder, paragraph);
                        }
                    }

                    break;
            }
        }

        if (truncated)
        {
            warnings.Add("The document was unusually long and only the first part was imported.");
        }

        var text = builder.ToString();

        if (string.IsNullOrWhiteSpace(text))
        {
            warnings.Add("No text could be read from this document.");
            return Task.FromResult(new ResumeParseResult(ResumeDocument.Empty(), warnings));
        }

        var document = ResumeTextHeuristics.Parse(text, warnings);

        return Task.FromResult(new ResumeParseResult(document, warnings));
    }

    /// <summary>
    /// Appends a paragraph, preserving its list marker.
    /// </summary>
    /// <remarks>
    /// A numbering reference means Word rendered this as a bullet. Re-adding an explicit
    /// marker lets the shared heuristics recognise it as a highlight — otherwise every
    /// achievement bullet would arrive as indistinguishable prose.
    /// </remarks>
    private static void AppendParagraph(StringBuilder builder, Paragraph paragraph)
    {
        var text = paragraph.InnerText?.Trim();

        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        var isListItem = paragraph.ParagraphProperties?.NumberingProperties is not null;

        builder.AppendLine(isListItem ? $"• {text}" : text);
    }
}
