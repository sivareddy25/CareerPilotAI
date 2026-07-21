using CareerPilot.Application.Abstractions.Jobs;
using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Domain.Jobs;

namespace CareerPilot.Application.Jobs.Commands;

public sealed record SynchronizeJobsCommand(JobProviderKind? ProviderKind = null)
    : ICommand<IReadOnlyList<JobSyncResult>>;

internal sealed class SynchronizeJobsCommandHandler(IJobSynchronizationService syncService)
    : ICommandHandler<SynchronizeJobsCommand, IReadOnlyList<JobSyncResult>>
{
    public async Task<IReadOnlyList<JobSyncResult>> Handle(SynchronizeJobsCommand command, CancellationToken cancellationToken)
    {
        if (command.ProviderKind.HasValue)
        {
            var result = await syncService.SynchronizeProviderAsync(command.ProviderKind.Value, cancellationToken);
            return [result];
        }

        return await syncService.SynchronizeAllProvidersAsync(cancellationToken);
    }
}
