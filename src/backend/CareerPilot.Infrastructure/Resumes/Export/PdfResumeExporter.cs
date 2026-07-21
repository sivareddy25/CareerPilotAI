using CareerPilot.Application.Abstractions.Resumes;
using CareerPilot.Domain.Resumes;
using Microsoft.Extensions.Logging;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.Rendering;
using PdfSharp.Fonts;

namespace CareerPilot.Infrastructure.Resumes.Export;

/// <summary>
/// Renders a resume to PDF.
/// </summary>
/// <remarks>
/// <para>
/// Uses MigraDoc/PDFsharp (MIT). MigraDoc is a document-flow layout engine, which is
/// what makes multi-page resumes work: it breaks pages, keeps headings with the content
/// beneath them, and reflows when a template changes the font size. Drawing to raw
/// PDFsharp coordinates would mean re-implementing pagination per template.
/// </para>
/// <para>
/// QuestPDF has a nicer API for this, but its Community licence is conditional on
/// revenue — a commercial decision that should not be made silently inside a build
/// file. MigraDoc carries no such condition.
/// </para>
/// <para>
/// Consumes the shared <see cref="ResumeLayout"/> block list, so it decides only how a
/// block looks, never which blocks exist or in what order.
/// </para>
/// </remarks>
internal sealed class PdfResumeExporter : IResumeExporter
{
    private readonly IResumeTemplateCatalog _templates;

    /// <summary>
    /// Installs the font resolver exactly once for the process.
    /// </summary>
    /// <remarks>
    /// GlobalFontSettings is static and PDFsharp throws if it is assigned twice, so this
    /// is guarded by Lazy rather than set in the constructor — the exporter is a
    /// singleton today, but a future scoped registration must not break rendering.
    /// </remarks>
    private static readonly Lazy<bool> FontResolverInstalled = new(() =>
    {
        GlobalFontSettings.FontResolver = new SystemFontResolver();
        return true;
    });

    public PdfResumeExporter(IResumeTemplateCatalog templates, ILogger<PdfResumeExporter> logger)
    {
        _templates = templates;

        _ = FontResolverInstalled.Value;

        if (!SystemFontResolver.HasAnyFont)
        {
            // Loud, and at startup rather than on the first export: a container with no
            // fonts installed cannot render a PDF at all, and that is a deployment
            // problem the operator needs told about before a user hits it.
            logger.LogError(
                "No TrueType fonts were found on this host. PDF export will fail. "
                + "Install a font package such as fonts-liberation in the runtime image.");
        }
        else
        {
            logger.LogInformation(
                "PDF font resolver ready with {FontCount} fonts.",
                SystemFontResolver.DiscoveredFontCount);
        }
    }

    public ResumeFormat Format => ResumeFormat.Pdf;

    public Task<ResumeExportResult> ExportAsync(
        ResumeDocument document,
        ResumeTemplateKey template,
        string fileNameWithoutExtension,
        CancellationToken cancellationToken = default)
    {
        var descriptor = _templates.Get(template);
        var blocks = ResumeLayout.Build(document, descriptor);

        var pdfDocument = new Document();
        ConfigureStyles(pdfDocument, descriptor);

        var section = pdfDocument.AddSection();
        ConfigurePage(section, descriptor);

        foreach (var block in blocks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Render(section, block, descriptor);
        }

        var renderer = new PdfDocumentRenderer { Document = pdfDocument };
        renderer.RenderDocument();

        using var buffer = new MemoryStream();
        renderer.PdfDocument.Save(buffer, closeStream: false);

        return Task.FromResult(new ResumeExportResult(
            buffer.ToArray(),
            "application/pdf",
            $"{fileNameWithoutExtension}.pdf"));
    }

    private static void ConfigurePage(Section section, ResumeTemplateDescriptor descriptor)
    {
        section.PageSetup.PageFormat = PageFormat.A4;

        // Executive gets wider margins — the airier layout is the whole point of it.
        var margin = descriptor.Key == ResumeTemplateKey.Executive ? "2.4cm" : "1.8cm";

        section.PageSetup.TopMargin = margin;
        section.PageSetup.BottomMargin = margin;
        section.PageSetup.LeftMargin = margin;
        section.PageSetup.RightMargin = margin;
    }

    private static void ConfigureStyles(Document document, ResumeTemplateDescriptor descriptor)
    {
        var accent = Color.Parse(descriptor.AccentColor);

        var normal = document.Styles["Normal"]!;
        normal.Font.Name = descriptor.BodyFont;
        normal.Font.Size = Unit.FromPoint(descriptor.BaseFontSize);
        // Slightly open leading: dense resumes are harder to skim, and a recruiter skims.
        normal.ParagraphFormat.LineSpacingRule = LineSpacingRule.Multiple;
        normal.ParagraphFormat.LineSpacing = 1.15;
        normal.ParagraphFormat.SpaceAfter = Unit.FromPoint(2);

        var name = document.Styles.AddStyle("ResumeName", "Normal");
        name.Font.Name = descriptor.HeadingFont;
        name.Font.Size = Unit.FromPoint(descriptor.BaseFontSize + 11);
        name.Font.Bold = true;
        name.Font.Color = accent;

        var headline = document.Styles.AddStyle("ResumeHeadline", "Normal");
        headline.Font.Size = Unit.FromPoint(descriptor.BaseFontSize + 1.5);
        headline.Font.Color = Colors.DimGray;

        var contact = document.Styles.AddStyle("ResumeContact", "Normal");
        contact.Font.Size = Unit.FromPoint(descriptor.BaseFontSize - 1);
        contact.Font.Color = Colors.DimGray;
        contact.ParagraphFormat.SpaceAfter = Unit.FromPoint(10);

        var heading = document.Styles.AddStyle("ResumeSection", "Normal");
        heading.Font.Name = descriptor.HeadingFont;
        heading.Font.Size = Unit.FromPoint(descriptor.BaseFontSize + 1.5);
        heading.Font.Bold = true;
        heading.Font.Color = accent;
        heading.ParagraphFormat.SpaceBefore = Unit.FromPoint(11);
        heading.ParagraphFormat.SpaceAfter = Unit.FromPoint(4);
        // Keeps a heading attached to the content under it, so a page break cannot
        // strand "Experience" alone at the foot of a page.
        heading.ParagraphFormat.KeepWithNext = true;

        // Minimal has no rules or colour at all; every other template gets an underline.
        if (descriptor.Key != ResumeTemplateKey.Minimal)
        {
            heading.ParagraphFormat.Borders.Bottom = new Border
            {
                Width = Unit.FromPoint(0.75),
                Color = accent,
            };
        }

        var entryTitle = document.Styles.AddStyle("ResumeEntryTitle", "Normal");
        entryTitle.Font.Bold = true;
        entryTitle.Font.Size = Unit.FromPoint(descriptor.BaseFontSize + 0.5);
        entryTitle.ParagraphFormat.SpaceBefore = Unit.FromPoint(6);
        entryTitle.ParagraphFormat.KeepWithNext = true;

        var entryMeta = document.Styles.AddStyle("ResumeEntryMeta", "Normal");
        entryMeta.Font.Size = Unit.FromPoint(descriptor.BaseFontSize - 1);
        entryMeta.Font.Color = Colors.Gray;
        entryMeta.ParagraphFormat.KeepWithNext = true;

        var bullet = document.Styles.AddStyle("ResumeBullet", "Normal");
        bullet.ParagraphFormat.LeftIndent = Unit.FromPoint(12);
        bullet.ParagraphFormat.FirstLineIndent = Unit.FromPoint(-8);
        bullet.ParagraphFormat.SpaceAfter = Unit.FromPoint(1.5);
    }

    private static void Render(Section section, LayoutBlock block, ResumeTemplateDescriptor descriptor)
    {
        switch (block.Kind)
        {
            case LayoutBlockKind.Name:
                var nameParagraph = section.AddParagraph(block.Text, "ResumeName");
                // Executive centres its header; the others stay left-aligned, which
                // reads faster and parses more predictably.
                if (descriptor.Key == ResumeTemplateKey.Executive)
                {
                    nameParagraph.Format.Alignment = ParagraphAlignment.Center;
                }

                break;

            case LayoutBlockKind.Headline:
                var headlineParagraph = section.AddParagraph(block.Text, "ResumeHeadline");
                if (descriptor.Key == ResumeTemplateKey.Executive)
                {
                    headlineParagraph.Format.Alignment = ParagraphAlignment.Center;
                }

                break;

            case LayoutBlockKind.Contact:
                var contactParagraph = section.AddParagraph(block.Text, "ResumeContact");
                if (descriptor.Key == ResumeTemplateKey.Executive)
                {
                    contactParagraph.Format.Alignment = ParagraphAlignment.Center;
                }

                break;

            case LayoutBlockKind.SectionHeading:
                // Upper-cased for templates whose visual language calls for it. This is
                // a rendering choice only — the stored document is untouched.
                var headingText = descriptor.Key is ResumeTemplateKey.AtsFriendly or ResumeTemplateKey.Modern
                    ? block.Text.ToUpperInvariant()
                    : block.Text;

                section.AddParagraph(headingText, "ResumeSection");
                break;

            case LayoutBlockKind.EntryTitle:
                section.AddParagraph(block.Text, "ResumeEntryTitle");
                break;

            case LayoutBlockKind.EntryMeta:
                section.AddParagraph(block.Text, "ResumeEntryMeta");
                break;

            case LayoutBlockKind.Paragraph:
                section.AddParagraph(block.Text, "Normal");
                break;

            case LayoutBlockKind.Bullets:
                foreach (var bullet in block.Bullets)
                {
                    // A literal bullet character rather than MigraDoc's list styles:
                    // ATS parsers read the glyph, and a list style can be lost when a
                    // PDF is converted back to text.
                    section.AddParagraph($"•  {bullet}", "ResumeBullet");
                }

                break;
        }
    }
}
