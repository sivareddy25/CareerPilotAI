using CareerPilot.Domain.Entities.Automation;

namespace CareerPilot.Application.Abstractions.Persistence;

public interface ICandidateAnswerRepository
{
    Task<IReadOnlyList<CandidateAnswer>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<CandidateAnswer?> FindByQuestionKeyAsync(Guid userId, string questionKey, CancellationToken cancellationToken);
    Task AddAsync(CandidateAnswer answer, CancellationToken cancellationToken);
    Task UpdateAsync(CandidateAnswer answer, CancellationToken cancellationToken);
}
