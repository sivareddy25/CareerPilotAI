using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Abstractions.Persistence;

namespace CareerPilot.Application.Jobs.Commands;

public sealed record DeactivateExpiredJobsCommand : ICommand<int>;

internal sealed class DeactivateExpiredJobsCommandHandler(
    IJobRepository jobRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<DeactivateExpiredJobsCommand, int>
{
    public async Task<int> Handle(DeactivateExpiredJobsCommand command, CancellationToken cancellationToken)
    {
        var count = await jobRepository.DeactivateExpiredJobsAsync(cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return count;
    }
}
