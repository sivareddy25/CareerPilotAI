using CareerPilot.Domain.Resumes;

namespace CareerPilot.Application.Abstractions.Resumes;

public interface IResumeTemplateProvider
{
    IReadOnlyList<ResumeTemplateDescriptor> GetTemplates();
    ResumeTemplateDescriptor GetTemplate(ResumeTemplateKey key);
}
