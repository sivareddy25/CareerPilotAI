using CareerPilot.Application.Abstractions.Authentication;
using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Abstractions.Persistence;
using CareerPilot.Application.Exceptions;
using CareerPilot.Application.Profiles.Models;

namespace CareerPilot.Application.Profiles.Queries.GetProfile;

/// <summary>
/// Returns the caller's own profile.
/// </summary>
/// <remarks>
/// Carries no user id, and that is the security control. Accepting one would mean every
/// authenticated user could read every other user's profile simply by changing a
/// parameter — the single most common broken-object-level-authorization bug. The
/// identity comes from the validated token and cannot be influenced by the request.
/// </remarks>
public sealed record GetProfileQuery : IQuery<ProfileDto>;

internal sealed class GetProfileQueryHandler(
    ICurrentUserService currentUser,
    IUserProfileRepository profiles)
    : IQueryHandler<GetProfileQuery, ProfileDto>
{
    public async Task<ProfileDto> Handle(GetProfileQuery query, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            throw new InvalidCredentialsException();
        }

        var profile = await profiles.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new ProfileNotFoundException();

        return ProfileDto.From(profile);
    }
}
