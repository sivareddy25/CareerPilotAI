using CareerPilot.Application.Abstractions.Authentication;
using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Abstractions.Persistence;
using CareerPilot.Domain.Entities.Profiles;

namespace CareerPilot.Application.Onboarding.Commands;

public sealed record CompleteOnboardingCommand(
    string FirstName,
    string LastName,
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
    IUserRepository userRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CompleteOnboardingCommand, bool>
{
    public async Task<bool> Handle(CompleteOnboardingCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId ?? Guid.Empty;
        var profile = await profileRepository.GetByUserIdAsync(userId, cancellationToken);
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);

        if (user != null)
        {
            user.UpdateName(command.FirstName, command.LastName);
        }

        if (profile == null)
        {
            profile = UserProfile.CreateFor(userId);
            profileRepository.Add(profile);
        }

        var fullName = string.IsNullOrWhiteSpace(command.DisplayName)
            ? $"{command.FirstName} {command.LastName}".Trim()
            : command.DisplayName;

        profile.CompleteOnboarding(
            fullName,
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
