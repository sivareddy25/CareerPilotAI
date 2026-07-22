using CareerPilot.Application.Abstractions.Authentication;
using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Abstractions.Persistence;
using CareerPilot.Application.Profiles.Models;

namespace CareerPilot.Application.Profiles.Queries.GetCareerProfile;

public sealed record GetCareerProfileQuery : IQuery<CareerProfileDto>;

internal sealed class GetCareerProfileQueryHandler(
    ICurrentUserService currentUserService,
    IUserProfileRepository profileRepository)
    : IQueryHandler<GetCareerProfileQuery, CareerProfileDto>
{
    public async Task<CareerProfileDto> Handle(GetCareerProfileQuery query, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId ?? Guid.Empty;
        var profile = await profileRepository.GetByUserIdAsync(userId, cancellationToken);

        // An empty career profile is a valid state — a user who has not filled it in yet — so
        // this returns an empty DTO rather than a 404, keeping the read side total.
        return profile is null
            ? new CareerProfileDto(null, null, null, null, null, null, [])
            : CareerProfileDto.From(profile);
    }
}
