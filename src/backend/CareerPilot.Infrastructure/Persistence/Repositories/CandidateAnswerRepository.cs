using CareerPilot.Application.Abstractions.Persistence;
using CareerPilot.Domain.Entities.Automation;
using Microsoft.EntityFrameworkCore;

namespace CareerPilot.Infrastructure.Persistence.Repositories;

public sealed class CandidateAnswerRepository(ApplicationDbContext dbContext) : ICandidateAnswerRepository
{
    public async Task<IReadOnlyList<CandidateAnswer>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await dbContext.Set<CandidateAnswer>()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.LastUsedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<CandidateAnswer?> FindByQuestionKeyAsync(Guid userId, string questionKey, CancellationToken cancellationToken)
    {
        var normalizedKey = CandidateAnswer.NormalizeKey(questionKey);
        return await dbContext.Set<CandidateAnswer>()
            .FirstOrDefaultAsync(a => a.UserId == userId && a.QuestionKey == normalizedKey, cancellationToken);
    }

    public async Task AddAsync(CandidateAnswer answer, CancellationToken cancellationToken)
    {
        await dbContext.Set<CandidateAnswer>().AddAsync(answer, cancellationToken);
    }

    public Task UpdateAsync(CandidateAnswer answer, CancellationToken cancellationToken)
    {
        dbContext.Set<CandidateAnswer>().Update(answer);
        return Task.CompletedTask;
    }
}
