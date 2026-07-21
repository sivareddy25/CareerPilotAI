using CareerPilot.Application.Abstractions.Authentication;
using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Abstractions.Persistence;
using CareerPilot.Application.Exceptions;
using CareerPilot.Application.Profiles.Models;
using Microsoft.Extensions.Logging;

namespace CareerPilot.Application.Profiles.Commands.UpdateProfile;

/// <summary>
/// Updates the caller's profile.
/// </summary>
/// <remarks>
/// <para>
/// No user id, and no email. The id is taken from the token; the email is immutable
/// here by design — it is the login identifier and the account's uniqueness key, so
/// changing it is an identity change that needs a verification round-trip against both
/// the old and new address. Accepting it on this command would let anyone holding a
/// session take over an address they do not control.
/// </para>
/// <para>
/// Returns the updated profile rather than nothing, so the client can reconcile its
/// optimistic update against what the server actually stored.
/// </para>
/// </remarks>
public sealed record UpdateProfileCommand(
    string? FirstName,
    string? LastName,
    string? DisplayName,
    string? PhoneNumber,
    string? Country,
    string? State,
    string? City,
    string? TimeZone,
    string? PreferredLanguage,
    string? Bio,
    string? LinkedInUrl,
    string? GitHubUrl,
    string? PortfolioUrl) : ICommand<ProfileDto>;

internal sealed class UpdateProfileCommandHandler(
    ICurrentUserService currentUser,
    IUserProfileRepository profiles,
    IUnitOfWork unitOfWork,
    ILogger<UpdateProfileCommandHandler> logger)
    : ICommandHandler<UpdateProfileCommand, ProfileDto>
{
    public async Task<ProfileDto> Handle(UpdateProfileCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (currentUser.UserId is not { } userId)
        {
            throw new InvalidCredentialsException();
        }

        var profile = await profiles.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new ProfileNotFoundException();

        profile.UpdateDetails(
            command.DisplayName,
            command.PhoneNumber,
            command.Country,
            command.State,
            command.City,
            command.TimeZone,
            command.PreferredLanguage,
            command.Bio,
            command.LinkedInUrl,
            command.GitHubUrl,
            command.PortfolioUrl);

        // Name lives on the user, not the profile. Both are changed inside one unit of
        // work, so the two halves of a single logical edit commit or fail together.
        profile.User?.UpdateName(command.FirstName, command.LastName);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // The values themselves are personal data and stay out of the log; that a
        // change happened, and to whom, is what an audit trail needs.
        logger.LogInformation("Profile updated. UserId: {UserId}", userId);

        return ProfileDto.From(profile);
    }
}
