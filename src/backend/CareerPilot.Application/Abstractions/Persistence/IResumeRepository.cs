using CareerPilot.Domain.Resumes;

namespace CareerPilot.Application.Abstractions.Persistence;

public interface IResumeRepository
{
    /// <summary>
    /// Loads a resume only if it belongs to <paramref name="userId"/>.
    /// </summary>
    /// <remarks>
    /// Ownership is a parameter, not an afterthought. A <c>GetById</c> that returned any
    /// resume would put the burden of checking on every caller, and the first caller to
    /// forget would expose every user's resumes to anyone who could guess an id. Here
    /// there is no way to ask the wrong question.
    /// </remarks>
    Task<Resume?> GetOwnedAsync(Guid resumeId, Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Resume>> ListForUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<int> CountForUserAsync(Guid userId, CancellationToken cancellationToken = default);

    void Add(Resume resume);
}
