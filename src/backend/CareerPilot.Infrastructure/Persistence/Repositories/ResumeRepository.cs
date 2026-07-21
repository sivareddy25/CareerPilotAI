using CareerPilot.Application.Abstractions.Persistence;
using CareerPilot.Domain.Resumes;
using Microsoft.EntityFrameworkCore;

namespace CareerPilot.Infrastructure.Persistence.Repositories;

internal sealed class ResumeRepository(ApplicationDbContext context) : IResumeRepository
{
    /// <remarks>
    /// The ownership predicate is part of the query, not a check performed after
    /// loading. A resume belonging to someone else is therefore indistinguishable from
    /// one that does not exist — the caller cannot tell the difference, and there is no
    /// window in which the wrong row is in memory.
    /// </remarks>
    public Task<Resume?> GetOwnedAsync(
        Guid resumeId,
        Guid userId,
        CancellationToken cancellationToken = default) =>
        context.Resumes
            .FirstOrDefaultAsync(
                resume => resume.Id == resumeId && resume.UserId == userId,
                cancellationToken);

    /// <remarks>
    /// No-tracking: the listing is read-only, and tracking fifty resumes would put fifty
    /// deserialised documents into the change tracker to render a list of titles.
    /// </remarks>
    public async Task<IReadOnlyList<Resume>> ListForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        await context.Resumes
            .AsNoTracking()
            .Where(resume => resume.UserId == userId)
            .OrderByDescending(resume => resume.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<int> CountForUserAsync(Guid userId, CancellationToken cancellationToken = default) =>
        context.Resumes.CountAsync(resume => resume.UserId == userId, cancellationToken);

    public void Add(Resume resume) => context.Resumes.Add(resume);
}
