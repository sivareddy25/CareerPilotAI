using CareerPilot.Domain.Resumes;

namespace CareerPilot.Application.Abstractions.Resumes;

public sealed record ResumeExportOptions(
    ResumePageSize PageSize = ResumePageSize.A4,
    ResumeMarginSize Margin = ResumeMarginSize.Normal,
    bool IncludePageNumbers = true,
    bool IncludeHeaderFooter = false,
    string? CustomHeader = null);

public interface IResumeRenderer
{
    string RenderHtml(ResumeDocument document, ResumeTemplateDescriptor template);
}
