using CareerPilot.Domain.Resumes;

namespace CareerPilot.Application.Abstractions.Resumes;

public interface IResumeImporter
{
    Task<ResumeParseResult> ImportAsync(Stream content, ResumeFormat format, CancellationToken cancellationToken = default);
}
