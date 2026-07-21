using CareerPilot.Application.Abstractions.Authentication;
using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Abstractions.Persistence;
using CareerPilot.Domain.Entities.Profiles;

namespace CareerPilot.Application.Onboarding.Commands;

public sealed record CompleteOnboardingCommand(
    string DisplayName,
    string PhoneNumber,
    string LinkedInUrl,
    string GitHubUrl,
    string PortfolioUrl,
    string WorkAuthorization,
    string PreferredSalary,
    string TargetJobTitles) : ICommand<bool>;

internal sealed class CompleteOnboardingCommandHandler(
    ICurrentUserService currentUserService,
    IUserProfileRepository profileRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CompleteOnboardingCommand, bool>
{
    public async Task<bool> Handle(CompleteOnboardingCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId ?? Guid.Empty;
        var profile = await profileRepository.GetByUserIdAsync(userId, cancellationToken);

        if (profile == null)
        {
            profile = UserProfile.CreateFor(userId);
            profileRepository.Add(profile);
        }

        profile.CompleteOnboarding(
            command.DisplayName,
            command.PhoneNumber,
            command.LinkedInUrl,
            command.GitHubUrl,
            command.PortfolioUrl,
            command.WorkAuthorization,
            command.PreferredSalary,
            command.TargetJobTitles);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
