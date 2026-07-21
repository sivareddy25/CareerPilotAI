using CareerPilot.Application.Abstractions.Resumes;
using CareerPilot.Domain.Resumes;

namespace CareerPilot.Infrastructure.Resumes.Templates;

/// <summary>
/// The six templates, defined once.
/// </summary>
internal sealed class ResumeTemplateCatalog : IResumeTemplateCatalog
{
    private static readonly IReadOnlyList<ResumeSectionKind> ExperienceFirst =
    [
        ResumeSectionKind.Summary,
        ResumeSectionKind.Experience,
        ResumeSectionKind.Skills,
        ResumeSectionKind.Education,
        ResumeSectionKind.Projects,
        ResumeSectionKind.Certifications,
    ];

    private static readonly IReadOnlyList<ResumeSectionKind> SkillsForward =
    [
        ResumeSectionKind.Summary,
        ResumeSectionKind.Skills,
        ResumeSectionKind.Experience,
        ResumeSectionKind.Projects,
        ResumeSectionKind.Education,
        ResumeSectionKind.Certifications,
    ];

    private readonly IReadOnlyList<ResumeTemplateDescriptor> _templates =
    [
        new()
        {
            Key = ResumeTemplateKey.AtsFriendly,
            Name = "ATS Friendly",
            Description =
                "Single column, standard headings, no graphics. The safest choice when a "
                + "resume will be read by software before a person.",
            IsAtsSafe = true,
            Columns = 1,
            AccentColor = "#000000",
            HeadingFont = "Arial",
            BodyFont = "Arial",
            BaseFontSize = 10.5,
            SectionOrder = ExperienceFirst,
        },
        new()
        {
            Key = ResumeTemplateKey.Professional,
            Name = "Professional",
            Description = "Serif headings with a restrained accent rule. Suits most corporate applications.",
            IsAtsSafe = true,
            Columns = 1,
            AccentColor = "#1f3a5f",
            HeadingFont = "Georgia",
            BodyFont = "Calibri",
            BaseFontSize = 10.5,
            SectionOrder = ExperienceFirst,
        },
        new()
        {
            Key = ResumeTemplateKey.Executive,
            Name = "Executive",
            Description = "Generous spacing and a prominent header. Built for senior and board-level roles.",
            IsAtsSafe = true,
            Columns = 1,
            AccentColor = "#5b4636",
            HeadingFont = "Georgia",
            BodyFont = "Georgia",
            BaseFontSize = 11,
            SectionOrder = ExperienceFirst,
        },
        new()
        {
            Key = ResumeTemplateKey.Minimal,
            Name = "Minimal",
            Description = "Plain type, no rules or colour. Maximum content in the least space.",
            IsAtsSafe = true,
            Columns = 1,
            AccentColor = "#333333",
            HeadingFont = "Helvetica",
            BodyFont = "Helvetica",
            BaseFontSize = 10,
            SectionOrder = ExperienceFirst,
        },
        new()
        {
            Key = ResumeTemplateKey.Modern,
            Name = "Modern",
            Description =
                "Two columns with a colour accent. Visually strongest, but automated "
                + "parsers can misread multi-column layouts.",
            IsAtsSafe = false,
            Columns = 2,
            AccentColor = "#2563eb",
            HeadingFont = "Verdana",
            BodyFont = "Calibri",
            BaseFontSize = 10,
            SectionOrder = SkillsForward,
        },
        new()
        {
            Key = ResumeTemplateKey.Corporate,
            Name = "Corporate",
            Description = "Formal structured layout with distinct section dividers, ideal for enterprise and finance roles.",
            IsAtsSafe = true,
            Columns = 1,
            AccentColor = "#1e293b",
            HeadingFont = "Times New Roman",
            BodyFont = "Arial",
            BaseFontSize = 10.5,
            SectionOrder = ExperienceFirst,
        },
    ];

    public IReadOnlyList<ResumeTemplateDescriptor> All => _templates;

    public ResumeTemplateDescriptor Get(ResumeTemplateKey key) =>
        _templates.FirstOrDefault(template => template.Key == key)
        ?? _templates.First(template => template.Key == ResumeTemplateKey.AtsFriendly);
}
