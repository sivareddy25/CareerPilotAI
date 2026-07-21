using CareerPilot.Application.Abstractions.Authentication;
using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Abstractions.Persistence;
using CareerPilot.Application.Exceptions;
using CareerPilot.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace CareerPilot.Application.Authentication.Commands.ChangePassword;

/// <summary>
/// Re-verifies the current password, stores the new hash, and tears down every
/// existing session.
/// </summary>
/// <remarks>
/// The session teardown is the point of the operation as much as the new hash is.
/// Users change passwords because they believe someone else has the old one; leaving
/// that attacker's sessions alive would make the change cosmetic.
/// </remarks>
internal sealed class ChangePasswordCommandHandler(
    ICurrentUserService currentUser,
    IUserRepository users,
    IPasswordHashService passwordHashService,
    IRefreshTokenService refreshTokenService,
    ITokenBlacklist tokenBlacklist,
    IUnitOfWork unitOfWork,
    ILogger<ChangePasswordCommandHandler> logger)
    : ICommandHandler<ChangePasswordCommand, Unit>
{
    public async Task<Unit> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (currentUser.UserId is not { } userId)
        {
            throw new InvalidCredentialsException();
        }

        var user = await users.GetByIdAsync(userId, cancellationToken)
            ?? throw new InvalidCredentialsException();

        if (!passwordHashService.Verify(command.CurrentPassword, user.PasswordHash))
        {
            logger.LogWarning("Password change refused: current password incorrect. UserId: {UserId}", userId);
            throw new InvalidCredentialsException();
        }

        // Rotates the security stamp as a side effect, which is what invalidates
        // tokens minted under the old password.
        user.SetPasswordHash(passwordHashService.Hash(command.NewPassword));

        await refreshTokenService.RevokeAllForUserAsync(
            userId,
            RefreshTokenRevocationReason.CredentialsChanged,
            cancellationToken);

        // Including the caller's own token. Re-authenticating after a password change
        // is the expected behaviour, and special-casing the current session would
        // leave exactly one live token behind.
        if (currentUser.TokenId is { } tokenId && currentUser.TokenExpiresAt is { } expiresAt)
        {
            await tokenBlacklist.BlacklistAsync(tokenId, expiresAt, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Password changed; all sessions revoked. UserId: {UserId}", userId);

        return Unit.Value;
    }
}
