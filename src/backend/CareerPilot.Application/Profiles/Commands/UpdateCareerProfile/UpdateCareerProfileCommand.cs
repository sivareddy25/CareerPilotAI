using CareerPilot.Application.Abstractions.Authentication;
using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Abstractions.Persistence;
using CareerPilot.Application.Profiles.Models;
using CareerPilot.Domain.Entities.Profiles;
using CareerPilot.Domain.Jobs;

namespace CareerPilot.Application.Profiles.Commands.UpdateCareerProfile;

public sealed record UpdateCareerProfileCommand(
    int? YearsOfExperience,
    decimal? DesiredSalaryAmount,
    string? DesiredSalaryCurrency,
    EmploymentType? PreferredEmploymentType,
    RemoteType? PreferredRemoteType,
    string? TargetJobTitles,
    IReadOnlyList<ProfileSkillDto> Skills) : ICommand<CareerProfileDto>;

internal sealed class UpdateCareerProfileCommandHandler(
    ICurrentUserService currentUserService,
    IUserProfileRepository profileRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateCareerProfileCommand, CareerProfileDto>
{
    public async Task<CareerProfileDto> Handle(UpdateCareerProfileCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId ?? Guid.Empty;
        var profile = await profileRepository.GetByUserIdAsync(userId, cancellationToken);

        // Create-on-first-write, matching CompleteOnboarding: a user can fill in their career
        // data before ever touching the onboarding wizard, so a missing profile is not an error.
        if (profile is null)
        {
            profile = UserProfile.CreateFor(userId);
            profileRepository.Add(profile);
        }

        profile.SetCareerProfile(
            command.YearsOfExperience,
            command.DesiredSalaryAmount,
            command.DesiredSalaryCurrency,
            command.PreferredEmploymentType,
            command.PreferredRemoteType,
            command.TargetJobTitles,
            (command.Skills ?? []).Select(skill => (skill.Name, skill.YearsOfExperience)));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return CareerProfileDto.From(profile);
    }
}
