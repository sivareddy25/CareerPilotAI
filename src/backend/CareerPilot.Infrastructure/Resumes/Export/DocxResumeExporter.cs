using CareerPilot.Application.Abstractions.Resumes;
using CareerPilot.Domain.Resumes;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace CareerPilot.Infrastructure.Resumes.Export;

/// <summary>
/// Renders a resume to DOCX.
/// </summary>
/// <remarks>
/// <para>
/// Writes real Word styles rather than directly formatted runs. That distinction
/// matters beyond tidiness: styled headings give the document a navigable outline,
/// survive a recruiter re-saving or converting the file, and are what assistive
/// technology and automated parsers actually read. Bold 14pt text merely looks like a
/// heading and carries none of that.
/// </para>
/// <para>
/// Shares <see cref="ResumeLayout"/> with the PDF exporter, so both render the same
/// blocks in the same order and can only differ in styling.
/// </para>
/// </remarks>
internal sealed class DocxResumeExporter(IResumeTemplateCatalog templates) : IResumeExporter
{
    private const string HeadingStyleId = "CareerPilotSection";
    private const string NameStyleId = "CareerPilotName";
    private const string MetaStyleId = "CareerPilotMeta";

    public ResumeFormat Format => ResumeFormat.Docx;

    public Task<ResumeExportResult> ExportAsync(
        ResumeDocument document,
        ResumeTemplateKey template,
        string fileNameWithoutExtension,
        CancellationToken cancellationToken = default)
    {
        var descriptor = templates.Get(template);
        var blocks = ResumeLayout.Build(document, descriptor);

        using var buffer = new MemoryStream();

        using (var word = WordprocessingDocument.Create(buffer, WordprocessingDocumentType.Document, autoSave: true))
        {
            var mainPart = word.AddMainDocumentPart();
            mainPart.Document = new Document(new Body());

            AddStyles(mainPart, descriptor);

            var body = mainPart.Document.Body!;

            foreach (var block in blocks)
            {
                cancellationToken.ThrowIfCancellationRequested();
                body.Append(Render(block, descriptor));
            }

            // Section properties must come last in the body; Word treats a document
            // without them as malformed and may refuse to open it.
            body.Append(PageSetup());
        }

        return Task.FromResult(new ResumeExportResult(
            buffer.ToArray(),
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            $"{fileNameWithoutExtension}.docx"));
    }

    private static SectionProperties PageSetup() =>
        new(
            // A4 in twentieths of a point, the unit OOXML uses throughout.
            new PageSize { Width = 11906U, Height = 16838U },
            new PageMargin { Top = 1020, Bottom = 1020, Left = 1020, Right = 1020 });

    private static void AddStyles(MainDocumentPart mainPart, ResumeTemplateDescriptor descriptor)
    {
        var stylePart = mainPart.AddNewPart<StyleDefinitionsPart>();
        var styles = new Styles();

        var accent = descriptor.AccentColor.TrimStart('#');

        // Default paragraph style: everything else inherits the body font from here, so
        // a template's font choice applies once rather than per run.
        styles.Append(new DocDefaults(
            new RunPropertiesDefault(
                new RunPropertiesBaseStyle(
                    new RunFonts { Ascii = descriptor.BodyFont, HighAnsi = descriptor.BodyFont },
                    new FontSize { Val = HalfPoints(descriptor.BaseFontSize) }))));

        styles.Append(BuildStyle(
            NameStyleId,
            "CareerPilot Name",
            descriptor.HeadingFont,
            descriptor.BaseFontSize + 11,
            bold: true,
            color: accent,
            spaceAfter: 40));

        styles.Append(BuildStyle(
            HeadingStyleId,
            "CareerPilot Section",
            descriptor.HeadingFont,
            descriptor.BaseFontSize + 1.5,
            bold: true,
            color: accent,
            spaceBefore: 220,
            spaceAfter: 80,
            // Same reasoning as the PDF exporter: a heading must not be orphaned at a
            // page foot.
            keepNext: true));

        styles.Append(BuildStyle(
            MetaStyleId,
            "CareerPilot Meta",
            descriptor.BodyFont,
            descriptor.BaseFontSize - 1,
            bold: false,
            color: "808080",
            keepNext: true));

        stylePart.Styles = styles;
    }

    private static Style BuildStyle(
        string styleId,
        string name,
        string font,
        double sizePoints,
        bool bold,
        string color,
        int spaceBefore = 0,
        int spaceAfter = 0,
        bool keepNext = false)
    {
        var paragraphProperties = new StyleParagraphProperties(
            new SpacingBetweenLines
            {
                Before = spaceBefore.ToString(),
                After = spaceAfter.ToString(),
                Line = "260",
                LineRule = LineSpacingRuleValues.Auto,
            });

        if (keepNext)
        {
            paragraphProperties.Append(new KeepNext());
        }

        return new Style(
            new StyleName { Val = name },
            new BasedOn { Val = "Normal" },
            paragraphProperties,
            new StyleRunProperties(
                new RunFonts { Ascii = font, HighAnsi = font },
                new FontSize { Val = HalfPoints(sizePoints) },
                new Bold { Val = OnOffValue.FromBoolean(bold) },
                new DocumentFormat.OpenXml.Wordprocessing.Color { Val = color }))
        {
            Type = StyleValues.Paragraph,
            StyleId = styleId,
            CustomStyle = true,
        };
    }

    private static Paragraph Render(LayoutBlock block, ResumeTemplateDescriptor descriptor)
    {
        switch (block.Kind)
        {
            case LayoutBlockKind.Name:
                return StyledParagraph(block.Text, NameStyleId, centered: descriptor.Key == ResumeTemplateKey.Executive);

            case LayoutBlockKind.Headline:
            case LayoutBlockKind.Contact:
                return StyledParagraph(block.Text, MetaStyleId, centered: descriptor.Key == ResumeTemplateKey.Executive);

            case LayoutBlockKind.SectionHeading:
                var headingText = descriptor.Key is ResumeTemplateKey.AtsFriendly or ResumeTemplateKey.Modern
                    ? block.Text.ToUpperInvariant()
                    : block.Text;

                return StyledParagraph(headingText, HeadingStyleId);

            case LayoutBlockKind.EntryTitle:
                var title = new Paragraph(
                    new ParagraphProperties(new KeepNext(), new SpacingBetweenLines { Before = "120" }),
                    new Run(new RunProperties(new Bold()), new Text(block.Text) { Space = SpaceProcessingModeValues.Preserve }));

                return title;

            case LayoutBlockKind.EntryMeta:
                return StyledParagraph(block.Text, MetaStyleId);

            case LayoutBlockKind.Bullets:
                // Bullets are emitted as separate paragraphs by the caller loop below;
                // this branch renders them joined so a single block stays one element.
                var bulletParagraph = new Paragraph();

                for (var i = 0; i < block.Bullets.Count; i++)
                {
                    if (i > 0)
                    {
                        bulletParagraph.Append(new Run(new Break()));
                    }

                    bulletParagraph.Append(new Run(
                        new Text($"•  {block.Bullets[i]}") { Space = SpaceProcessingModeValues.Preserve }));
                }

                bulletParagraph.PrependChild(new ParagraphProperties(
                    new Indentation { Left = "240", Hanging = "160" }));

                return bulletParagraph;

            default:
                return StyledParagraph(block.Text, null);
        }
    }

    private static Paragraph StyledParagraph(string text, string? styleId, bool centered = false)
    {
        var properties = new ParagraphProperties();

        if (styleId is not null)
        {
            properties.Append(new ParagraphStyleId { Val = styleId });
        }

        if (centered)
        {
            properties.Append(new Justification { Val = JustificationValues.Center });
        }

        return new Paragraph(
            properties,
            new Run(new Text(text) { Space = SpaceProcessingModeValues.Preserve }));
    }

    /// <summary>OOXML sizes runs in half-points.</summary>
    private static string HalfPoints(double points) =>
        ((int)Math.Round(points * 2)).ToString();
}
