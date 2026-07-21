using CareerPilot.Application.Abstractions.Authentication;
using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Abstractions.Persistence;
using CareerPilot.Application.Abstractions.Storage;
using CareerPilot.Application.Exceptions;
using Microsoft.Extensions.Logging;

namespace CareerPilot.Application.Profiles.Commands.DeleteProfileImage;

/// <summary>
/// Removes the caller's profile picture.
/// </summary>
/// <remarks>
/// Idempotent: deleting an avatar that is not there succeeds. A 404 would be
/// technically defensible but practically useless — the caller's goal is "no picture",
/// and that goal is already met.
/// </remarks>
public sealed record DeleteProfileImageCommand : ICommand<Unit>;

internal sealed class DeleteProfileImageCommandHandler(
    ICurrentUserService currentUser,
    IUserProfileRepository profiles,
    IFileStorageService fileStorage,
    IUnitOfWork unitOfWork,
    ILogger<DeleteProfileImageCommandHandler> logger)
    : ICommandHandler<DeleteProfileImageCommand, Unit>
{
    public async Task<Unit> Handle(DeleteProfileImageCommand command, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            throw new InvalidCredentialsException();
        }

        var profile = await profiles.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new ProfileNotFoundException();

        var previous = profile.ClearProfilePicture();

        if (previous is null)
        {
            return Unit.Value;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Same ordering as upload: the row loses the reference first, the bytes go
        // second. A failure here leaves an orphaned file, not a broken profile.
        try
        {
            await fileStorage.DeleteAsync(previous, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Profile picture row cleared but the file could not be removed. UserId: {UserId}",
                userId);
        }

        logger.LogInformation("Profile picture removed. UserId: {UserId}", userId);

        return Unit.Value;
    }
}
