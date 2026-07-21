using CareerPilot.Application.Abstractions.Authentication;
using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Abstractions.Persistence;
using CareerPilot.Application.Onboarding.Models;

namespace CareerPilot.Application.Onboarding.Queries;

public sealed record GetOnboardingStatusQuery : IQuery<OnboardingStatusDto>;

internal sealed class GetOnboardingStatusQueryHandler(
    ICurrentUserService currentUserService,
    IUserProfileRepository profileRepository)
    : IQueryHandler<GetOnboardingStatusQuery, OnboardingStatusDto>
{
    public async Task<OnboardingStatusDto> Handle(GetOnboardingStatusQuery query, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId ?? Guid.Empty;
        var profile = await profileRepository.GetByUserIdAsync(userId, cancellationToken);

        if (profile == null)
        {
            return new OnboardingStatusDto(false, null, currentUserService.Email, null, null, null, null, null, null);
        }

        return new OnboardingStatusDto(
            profile.IsOnboardingCompleted,
            profile.DisplayName,
            currentUserService.Email,
            profile.PhoneNumber,
            profile.LinkedInUrl,
            profile.GitHubUrl,
            profile.WorkAuthorization,
            profile.PreferredSalary,
            profile.TargetJobTitles);
    }
}
