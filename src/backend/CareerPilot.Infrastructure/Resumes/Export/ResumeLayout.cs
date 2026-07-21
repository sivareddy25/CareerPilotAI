using CareerPilot.Application.Abstractions.Resumes;
using CareerPilot.Domain.Resumes;

namespace CareerPilot.Infrastructure.Resumes.Export;

/// <summary>One renderable block, already flattened out of the document.</summary>
public sealed record LayoutBlock(LayoutBlockKind Kind, string Text, IReadOnlyList<string> Bullets);

public enum LayoutBlockKind
{
    /// <summary>Person's name. Largest type on the page.</summary>
    Name = 0,
    /// <summary>Headline under the name.</summary>
    Headline = 1,
    /// <summary>Contact line: email, phone, links.</summary>
    Contact = 2,
    /// <summary>Section heading such as "Experience".</summary>
    SectionHeading = 3,
    /// <summary>Entry title: role at company, or a degree.</summary>
    EntryTitle = 4,
    /// <summary>Dates and location under an entry title.</summary>
    EntryMeta = 5,
    /// <summary>Body paragraph.</summary>
    Paragraph = 6,
    /// <summary>Bulleted achievements, carried in <see cref="LayoutBlock.Bullets"/>.</summary>
    Bullets = 7,
}

/// <summary>
/// Flattens a <see cref="ResumeDocument"/> into an ordered block list, applying a
/// template's section order.
/// </summary>
/// <remarks>
/// <para>
/// This is the shared half of the rendering engine, and the reason exporters do not
/// multiply against templates. Deciding <i>what</i> to draw and in what order happens
/// exactly once, here; each exporter then only decides <i>how</i> to draw a block in its
/// own format. Adding a sixth template changes no exporter, and adding a fourth output
/// format changes no template.
/// </para>
/// <para>
/// Produces blocks, not strings, so an exporter can style each kind natively — a real
/// Word heading style rather than bold text that merely looks like one, which is also
/// what keeps the DOCX output navigable and accessible.
/// </para>
/// </remarks>
internal static class ResumeLayout
{
    public static IReadOnlyList<LayoutBlock> Build(
        ResumeDocument document,
        ResumeTemplateDescriptor template)
    {
        var blocks = new List<LayoutBlock>();

        void Add(LayoutBlockKind kind, string? text, IReadOnlyList<string>? bullets = null)
        {
            if (kind == LayoutBlockKind.Bullets)
            {
                if (bullets is { Count: > 0 })
                {
                    blocks.Add(new LayoutBlock(kind, string.Empty, bullets));
                }

                return;
            }

            if (!string.IsNullOrWhiteSpace(text))
            {
                blocks.Add(new LayoutBlock(kind, text.Trim(), []));
            }
        }

        Add(LayoutBlockKind.Name, document.Contact.FullName);
        Add(LayoutBlockKind.Headline, document.Contact.Headline);

        // Contact details collapse to one line. Separate lines per field would consume
        // a disproportionate share of a one-page resume.
        var contactParts = new[]
        {
            document.Contact.Email,
            document.Contact.Phone,
            document.Contact.Location,
            document.Contact.LinkedIn,
            document.Contact.GitHub,
            document.Contact.Website,
        }.Where(part => !string.IsNullOrWhiteSpace(part));

        Add(LayoutBlockKind.Contact, string.Join("  ·  ", contactParts));

        // Section order comes from the template — the one place presentation is allowed
        // to reorder content.
        foreach (var section in template.SectionOrder)
        {
            switch (section)
            {
                case ResumeSectionKind.Summary when !string.IsNullOrWhiteSpace(document.Summary):
                    Add(LayoutBlockKind.SectionHeading, "Summary");
                    Add(LayoutBlockKind.Paragraph, document.Summary);
                    break;

                case ResumeSectionKind.Experience when document.Experience.Count > 0:
                    Add(LayoutBlockKind.SectionHeading, "Experience");

                    foreach (var role in document.Experience)
                    {
                        Add(LayoutBlockKind.EntryTitle, JoinNonEmpty(" — ", role.Role, role.Company));
                        Add(LayoutBlockKind.EntryMeta, JoinNonEmpty("  ·  ", DateRange(role.StartDate, role.EndDate, role.IsCurrent), role.Location));
                        Add(LayoutBlockKind.Bullets, null, role.Highlights);
                    }

                    break;

                case ResumeSectionKind.Education when document.Education.Count > 0:
                    Add(LayoutBlockKind.SectionHeading, "Education");

                    foreach (var study in document.Education)
                    {
                        Add(LayoutBlockKind.EntryTitle, JoinNonEmpty(" — ", study.Degree, study.Institution));
                        Add(LayoutBlockKind.EntryMeta, JoinNonEmpty("  ·  ", study.FieldOfStudy, DateRange(study.StartDate, study.EndDate, false), study.Grade));
                    }

                    break;

                case ResumeSectionKind.Skills when document.Skills.Count > 0:
                    Add(LayoutBlockKind.SectionHeading, "Skills");

                    // Grouped when the document supplies categories, one flat line when
                    // it does not — the grouping is never stored twice.
                    var grouped = document.Skills
                        .GroupBy(skill => skill.Category)
                        .ToList();

                    foreach (var group in grouped)
                    {
                        var names = string.Join(", ", group.Select(skill => skill.Name));

                        Add(
                            LayoutBlockKind.Paragraph,
                            string.IsNullOrWhiteSpace(group.Key) ? names : $"{group.Key}: {names}");
                    }

                    break;

                case ResumeSectionKind.Projects when document.Projects.Count > 0:
                    Add(LayoutBlockKind.SectionHeading, "Projects");

                    foreach (var project in document.Projects)
                    {
                        Add(LayoutBlockKind.EntryTitle, project.Name);
                        Add(LayoutBlockKind.EntryMeta, JoinNonEmpty("  ·  ", project.Url));
                        Add(LayoutBlockKind.Paragraph, project.Description);
                        Add(LayoutBlockKind.Bullets, null, project.Highlights);
                    }

                    break;

                case ResumeSectionKind.Certifications when document.Certifications.Count > 0:
                    Add(LayoutBlockKind.SectionHeading, "Certifications");

                    foreach (var certification in document.Certifications)
                    {
                        Add(LayoutBlockKind.EntryTitle, certification.Name);
                        Add(LayoutBlockKind.EntryMeta, JoinNonEmpty("  ·  ", certification.Issuer, certification.IssuedDate));
                    }

                    break;
            }
        }

        return blocks;
    }

    private static string DateRange(string? start, string? end, bool isCurrent)
    {
        var to = isCurrent ? "Present" : end;

        if (string.IsNullOrWhiteSpace(start))
        {
            return to ?? string.Empty;
        }

        return string.IsNullOrWhiteSpace(to) ? start : $"{start} – {to}";
    }

    private static string JoinNonEmpty(string separator, params string?[] parts) =>
        string.Join(separator, parts.Where(part => !string.IsNullOrWhiteSpace(part)));
}
