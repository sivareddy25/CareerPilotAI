using CareerPilot.Application.Abstractions.Authentication;
using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Abstractions.Persistence;
using CareerPilot.Application.Abstractions.Storage;
using CareerPilot.Application.Exceptions;
using Microsoft.Extensions.Logging;

namespace CareerPilot.Application.Profiles.Commands.UploadProfileImage;

/// <summary>Where the newly stored avatar can be fetched from.</summary>
public sealed record ProfileImageDto(string ProfilePictureUrl);

/// <summary>
/// Replaces the caller's profile picture.
/// </summary>
/// <remarks>
/// Takes a <see cref="FileUploadRequest"/> rather than an <c>IFormFile</c>: the latter
/// is an ASP.NET Core type, and depending on it here would drag HTTP into the
/// Application layer and make this command untestable without a web host.
/// </remarks>
public sealed record UploadProfileImageCommand(FileUploadRequest File) : ICommand<ProfileImageDto>;

internal sealed class UploadProfileImageCommandHandler(
    ICurrentUserService currentUser,
    IUserProfileRepository profiles,
    IFileStorageService fileStorage,
    IUnitOfWork unitOfWork,
    ILogger<UploadProfileImageCommandHandler> logger)
    : ICommandHandler<UploadProfileImageCommand, ProfileImageDto>
{
    private const string Container = "profile-pictures";

    /// <summary>
    /// 2 MB. An avatar is displayed at a few hundred pixels, so anything larger is
    /// waste — and an unbounded upload endpoint is a cheap way to fill a disk.
    /// </summary>
    private const long MaxBytes = 2 * 1024 * 1024;

    /// <summary>
    /// Allow-list, not a block-list. Enumerating what is permitted fails closed on
    /// anything unanticipated; enumerating what is forbidden fails open on it.
    /// </summary>
    private static readonly string[] AllowedContentTypes =
        ["image/jpeg", "image/png", "image/webp", "image/gif"];

    public async Task<ProfileImageDto> Handle(
        UploadProfileImageCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (currentUser.UserId is not { } userId)
        {
            throw new InvalidCredentialsException();
        }

        var file = command.File;

        if (file.Length <= 0)
        {
            throw new InvalidUploadException("The uploaded file is empty.");
        }

        if (file.Length > MaxBytes)
        {
            throw new InvalidUploadException("Profile pictures must be 2 MB or smaller.");
        }

        // Content-Type is client-supplied and trivially forged, so this check alone
        // proves nothing. It is a cheap first filter; the storage implementation
        // additionally verifies the file's magic bytes and derives the extension from
        // those rather than from anything the client said.
        if (!AllowedContentTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidUploadException("Profile pictures must be a JPEG, PNG, WebP or GIF image.");
        }

        var profile = await profiles.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new ProfileNotFoundException();

        var previous = profile.ProfilePictureUrl;

        var stored = await fileStorage.SaveAsync(Container, file, cancellationToken);

        profile.SetProfilePicture(stored.Url);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // The old file is removed only after the new reference is committed. Doing it
        // first would leave the profile pointing at a deleted file if the save failed;
        // this ordering means the worst case is an orphaned blob, which costs disk
        // rather than breaking the page.
        if (!string.IsNullOrWhiteSpace(previous))
        {
            try
            {
                await fileStorage.DeleteAsync(previous, cancellationToken);
            }
            catch (Exception exception)
            {
                // The upload succeeded and the profile is correct. Failing the request
                // now would tell the user their upload failed when it did not.
                logger.LogWarning(
                    exception,
                    "Replaced profile picture could not be deleted; it is now orphaned. UserId: {UserId}",
                    userId);
            }
        }

        logger.LogInformation("Profile picture updated. UserId: {UserId}", userId);

        return new ProfileImageDto(stored.Url);
    }
}
