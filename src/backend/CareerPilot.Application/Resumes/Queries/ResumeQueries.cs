using CareerPilot.Application.Abstractions.Authentication;
using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Abstractions.Persistence;
using CareerPilot.Application.Abstractions.Resumes;
using CareerPilot.Application.Exceptions;
using CareerPilot.Application.Resumes.Models;
using CareerPilot.Application.Resumes.Services;

namespace CareerPilot.Application.Resumes.Queries;

/// <summary>Lists the caller's resumes, newest first. Metadata only.</summary>
public sealed record ListResumesQuery : IQuery<IReadOnlyList<ResumeSummaryDto>>;

internal sealed class ListResumesQueryHandler(
    ICurrentUserService currentUser,
    IResumeRepository resumes)
    : IQueryHandler<ListResumesQuery, IReadOnlyList<ResumeSummaryDto>>
{
    public async Task<IReadOnlyList<ResumeSummaryDto>> Handle(
        ListResumesQuery query,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            throw new InvalidCredentialsException();
        }

        var items = await resumes.ListForUserAsync(userId, cancellationToken);

        return items.Select(ResumeSummaryDto.From).ToList();
    }
}

/// <summary>
/// Returns one resume with its full document.
/// </summary>
/// <remarks>
/// Takes an id, unlike the profile module's queries, because a user has many resumes.
/// Ownership is therefore an explicit check rather than a structural guarantee — the
/// repository will only return the resume if it belongs to the caller, so a guessed id
/// is indistinguishable from a nonexistent one.
/// </remarks>
public sealed record GetResumeQuery(Guid ResumeId) : IQuery<ResumeDto>;

internal sealed class GetResumeQueryHandler(
    ICurrentUserService currentUser,
    IResumeRepository resumes)
    : IQueryHandler<GetResumeQuery, ResumeDto>
{
    public async Task<ResumeDto> Handle(GetResumeQuery query, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            throw new InvalidCredentialsException();
        }

        var resume = await resumes.GetOwnedAsync(query.ResumeId, userId, cancellationToken)
            ?? throw new ResumeNotFoundException();

        return ResumeDto.From(resume);
    }
}

/// <summary>Returns the template catalogue. No resume data, so nothing user-specific.</summary>
public sealed record GetResumeTemplatesQuery : IQuery<IReadOnlyList<ResumeTemplateDescriptor>>;

internal sealed class GetResumeTemplatesQueryHandler(TemplateService templates)
    : IQueryHandler<GetResumeTemplatesQuery, IReadOnlyList<ResumeTemplateDescriptor>>
{
    public Task<IReadOnlyList<ResumeTemplateDescriptor>> Handle(
        GetResumeTemplatesQuery query,
        CancellationToken cancellationToken) =>
        Task.FromResult(templates.GetAll());
}
