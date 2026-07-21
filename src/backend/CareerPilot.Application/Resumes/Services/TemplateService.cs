using CareerPilot.Application.Abstractions.Resumes;
using CareerPilot.Domain.Resumes;

namespace CareerPilot.Application.Resumes.Services;

/// <summary>
/// Service providing template listing and layout styling specs.
/// </summary>
public class ResumeTemplateService(IResumeTemplateCatalog catalog) : IResumeTemplateProvider
{
    public IReadOnlyList<ResumeTemplateDescriptor> GetAll() => catalog.All;

    public ResumeTemplateDescriptor Get(ResumeTemplateKey key) => catalog.Get(key);

    public IReadOnlyList<ResumeTemplateDescriptor> GetAtsSafe() =>
        catalog.All.Where(template => template.IsAtsSafe).ToList();

    public IReadOnlyList<ResumeTemplateDescriptor> GetTemplates() => catalog.All;

    public ResumeTemplateDescriptor GetTemplate(ResumeTemplateKey key) => catalog.Get(key);
}

// Alias for backwards compatibility
public sealed class TemplateService(IResumeTemplateCatalog catalog) : ResumeTemplateService(catalog);
