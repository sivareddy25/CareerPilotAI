using CareerPilot.Application.Abstractions.Jobs;
using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Abstractions.Persistence;

namespace CareerPilot.Application.Jobs.Commands;

public sealed record ImportJobsCommand(IReadOnlyList<RawJobPayload> Payloads) : ICommand<int>;

internal sealed class ImportJobsCommandHandler(
    IJobNormalizationService normalizationService,
    IJobRepository jobRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<ImportJobsCommand, int>
{
    public async Task<int> Handle(ImportJobsCommand command, CancellationToken cancellationToken)
    {
        var count = 0;
        foreach (var payload in command.Payloads)
        {
            var job = await normalizationService.NormalizeAsync(payload, cancellationToken);
            var existing = await jobRepository.GetByExternalIdAsync(job.ExternalJobId, job.Source, cancellationToken);

            if (existing is null)
            {
                await jobRepository.AddAsync(job, cancellationToken);
                count++;
            }
            else
            {
                existing.UpdatePosting(
                    job.Title,
                    job.Description,
                    job.Requirements,
                    job.Responsibilities,
                    job.Benefits,
                    job.Location,
                    job.Salary,
                    job.EmploymentType,
                    job.ExperienceLevel,
                    job.ExpiresAt,
                    job.ApplyUrl,
                    job.SourceMetadataJson,
                    job.ContentHash);

                jobRepository.Update(existing);
                count++;
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return count;
    }
}
