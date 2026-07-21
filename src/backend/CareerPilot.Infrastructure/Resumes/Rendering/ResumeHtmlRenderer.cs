using System.Text;
using CareerPilot.Application.Abstractions.Resumes;
using CareerPilot.Domain.Resumes;

namespace CareerPilot.Infrastructure.Resumes.Rendering;

public sealed class ResumeHtmlRenderer : IResumeRenderer
{
    public string RenderHtml(ResumeDocument document, ResumeTemplateDescriptor template)
    {
        var sb = new StringBuilder();

        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\">");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset=\"utf-8\">");
        sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        sb.AppendLine($"<title>{HtmlEncode(document.Contact.FullName ?? "Resume")}</title>");
        sb.AppendLine("<style>");
        sb.AppendLine($"  :root {{ --accent-color: {template.AccentColor}; }}");
        sb.AppendLine($"  body {{ font-family: '{template.BodyFont}', sans-serif; font-size: {template.BaseFontSize}pt; line-height: 1.5; color: #1e293b; margin: 0; padding: 24px; }}");
        sb.AppendLine($"  h1, h2, h3 {{ font-family: '{template.HeadingFont}', serif; margin: 0; }}");
        sb.AppendLine("  .resume-container { max-width: 800px; margin: 0 auto; background: #ffffff; padding: 32px; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.1); border-radius: 8px; }");
        sb.AppendLine("  .header-section { text-align: center; border-bottom: 2px solid var(--accent-color); padding-bottom: 16px; margin-bottom: 20px; }");
        sb.AppendLine("  .header-name { font-size: 24pt; font-weight: bold; color: var(--accent-color); }");
        sb.AppendLine("  .header-headline { font-size: 12pt; color: #64748b; margin-top: 4px; }");
        sb.AppendLine("  .contact-info { font-size: 9pt; color: #475569; margin-top: 8px; }");
        sb.AppendLine("  .section-block { margin-bottom: 20px; }");
        sb.AppendLine("  .section-title { font-size: 13pt; text-transform: uppercase; color: var(--accent-color); border-bottom: 1px solid #cbd5e1; padding-bottom: 4px; margin-bottom: 10px; }");
        sb.AppendLine("  .item-title { font-weight: bold; font-size: 11pt; }");
        sb.AppendLine("  .item-subtitle { color: #64748b; font-size: 9.5pt; display: flex; justify-content: space-between; }");
        sb.AppendLine("  ul { margin: 6px 0 0 18px; padding: 0; }");
        sb.AppendLine("  li { margin-bottom: 4px; }");
        sb.AppendLine("  .skill-badge { display: inline-block; background: #f1f5f9; color: #334155; padding: 2px 8px; border-radius: 4px; font-size: 9pt; margin: 2px; }");
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");

        sb.AppendLine("<div class=\"resume-container\">");

        // Header / Contact
        sb.AppendLine("  <div class=\"header-section\">");
        sb.AppendLine($"    <div class=\"header-name\">{HtmlEncode(document.Contact.FullName ?? "Your Name")}</div>");
        if (!string.IsNullOrWhiteSpace(document.Contact.Headline))
        {
            sb.AppendLine($"    <div class=\"header-headline\">{HtmlEncode(document.Contact.Headline)}</div>");
        }

        var contactParts = new List<string>();
        if (!string.IsNullOrWhiteSpace(document.Contact.Email)) contactParts.Add(HtmlEncode(document.Contact.Email));
        if (!string.IsNullOrWhiteSpace(document.Contact.Phone)) contactParts.Add(HtmlEncode(document.Contact.Phone));
        if (!string.IsNullOrWhiteSpace(document.Contact.Location)) contactParts.Add(HtmlEncode(document.Contact.Location));
        if (!string.IsNullOrWhiteSpace(document.Contact.LinkedIn)) contactParts.Add(HtmlEncode(document.Contact.LinkedIn));

        if (contactParts.Count > 0)
        {
            sb.AppendLine($"    <div class=\"contact-info\">{string.Join(" • ", contactParts)}</div>");
        }
        sb.AppendLine("  </div>");

        // Section order based on template descriptor
        foreach (var section in template.SectionOrder)
        {
            switch (section)
            {
                case ResumeSectionKind.Summary:
                    if (!string.IsNullOrWhiteSpace(document.Summary))
                    {
                        sb.AppendLine("  <div class=\"section-block\">");
                        sb.AppendLine("    <div class=\"section-title\">Professional Summary</div>");
                        sb.AppendLine($"    <p>{HtmlEncode(document.Summary)}</p>");
                        sb.AppendLine("  </div>");
                    }
                    break;

                case ResumeSectionKind.Experience:
                    if (document.Experience.Count > 0)
                    {
                        sb.AppendLine("  <div class=\"section-block\">");
                        sb.AppendLine("    <div class=\"section-title\">Work Experience</div>");
                        foreach (var exp in document.Experience)
                        {
                            sb.AppendLine("    <div style=\"margin-bottom: 12px;\">");
                            sb.AppendLine($"      <div class=\"item-title\">{HtmlEncode(exp.Role)} — {HtmlEncode(exp.Company)}</div>");
                            sb.AppendLine($"      <div class=\"item-subtitle\"><span>{HtmlEncode(exp.Location)}</span><span>{HtmlEncode(exp.StartDate)} - {HtmlEncode(exp.IsCurrent ? "Present" : exp.EndDate)}</span></div>");
                            if (exp.Highlights.Count > 0)
                            {
                                sb.AppendLine("      <ul>");
                                foreach (var bullet in exp.Highlights)
                                {
                                    sb.AppendLine($"        <li>{HtmlEncode(bullet)}</li>");
                                }
                                sb.AppendLine("      </ul>");
                            }
                            sb.AppendLine("    </div>");
                        }
                        sb.AppendLine("  </div>");
                    }
                    break;

                case ResumeSectionKind.Education:
                    if (document.Education.Count > 0)
                    {
                        sb.AppendLine("  <div class=\"section-block\">");
                        sb.AppendLine("    <div class=\"section-title\">Education</div>");
                        foreach (var edu in document.Education)
                        {
                            sb.AppendLine("    <div style=\"margin-bottom: 8px;\">");
                            sb.AppendLine($"      <div class=\"item-title\">{HtmlEncode(edu.Degree)} in {HtmlEncode(edu.FieldOfStudy)}</div>");
                            sb.AppendLine($"      <div class=\"item-subtitle\"><span>{HtmlEncode(edu.Institution)}</span><span>{HtmlEncode(edu.EndDate)}</span></div>");
                            sb.AppendLine("    </div>");
                        }
                        sb.AppendLine("  </div>");
                    }
                    break;

                case ResumeSectionKind.Skills:
                    if (document.Skills.Count > 0)
                    {
                        sb.AppendLine("  <div class=\"section-block\">");
                        sb.AppendLine("    <div class=\"section-title\">Skills</div>");
                        sb.AppendLine("    <div>");
                        foreach (var skill in document.Skills)
                        {
                            sb.AppendLine($"      <span class=\"skill-badge\">{HtmlEncode(skill.Name)}</span>");
                        }
                        sb.AppendLine("    </div>");
                        sb.AppendLine("  </div>");
                    }
                    break;

                case ResumeSectionKind.Projects:
                    if (document.Projects.Count > 0)
                    {
                        sb.AppendLine("  <div class=\"section-block\">");
                        sb.AppendLine("    <div class=\"section-title\">Projects</div>");
                        foreach (var proj in document.Projects)
                        {
                            sb.AppendLine("    <div style=\"margin-bottom: 8px;\">");
                            sb.AppendLine($"      <div class=\"item-title\">{HtmlEncode(proj.Name)}</div>");
                            if (!string.IsNullOrWhiteSpace(proj.Description))
                            {
                                sb.AppendLine($"      <p style=\"margin: 2px 0;\">{HtmlEncode(proj.Description)}</p>");
                            }
                            sb.AppendLine("    </div>");
                        }
                        sb.AppendLine("  </div>");
                    }
                    break;

                case ResumeSectionKind.Certifications:
                    if (document.Certifications.Count > 0)
                    {
                        sb.AppendLine("  <div class=\"section-block\">");
                        sb.AppendLine("    <div class=\"section-title\">Certifications</div>");
                        foreach (var cert in document.Certifications)
                        {
                            sb.AppendLine($"    <div><strong>{HtmlEncode(cert.Name)}</strong> — {HtmlEncode(cert.Issuer)} ({HtmlEncode(cert.IssuedDate)})</div>");
                        }
                        sb.AppendLine("  </div>");
                    }
                    break;
            }
        }

        sb.AppendLine("</div>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    private static string HtmlEncode(string? input) =>
        global::System.Net.WebUtility.HtmlEncode(input ?? string.Empty);
}
