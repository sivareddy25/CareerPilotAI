using CareerPilot.Application.Abstractions.Authentication;
using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Abstractions.Persistence;
using CareerPilot.Application.Exceptions;
using CareerPilot.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace CareerPilot.Application.Authentication.Commands.Logout;

/// <summary>
/// Revokes the refresh token and blacklists the access token that carried the request.
/// </summary>
/// <remarks>
/// Both halves are necessary. Revoking the refresh token stops the session being
/// renewed, but the access token already issued stays cryptographically valid until
/// it expires — a signed JWT cannot be un-issued. Blacklisting its <c>jti</c> closes
/// that window.
/// </remarks>
internal sealed class LogoutCommandHandler(
    ICurrentUserService currentUser,
    IRefreshTokenService refreshTokenService,
    ITokenBlacklist tokenBlacklist,
    IUnitOfWork unitOfWork,
    ILogger<LogoutCommandHandler> logger)
    : ICommandHandler<LogoutCommand, Unit>
{
    public async Task<Unit> Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        // The endpoint requires authentication, so this is defence in depth rather
        // than the primary check.
        if (currentUser.UserId is not { } userId)
        {
            throw new InvalidCredentialsException();
        }

        if (!string.IsNullOrWhiteSpace(command.RefreshToken))
        {
            var validation = await refreshTokenService.ValidateAsync(command.RefreshToken, cancellationToken);

            // Only revoke a token that belongs to the caller. Without this check, an
            // authenticated user could sign out arbitrary other sessions by posting
            // someone else's refresh token.
            if (validation.Token is { } token && token.UserId == userId)
            {
                await refreshTokenService.RevokeAsync(
                    token,
                    RefreshTokenRevocationReason.SignedOut,
                    cancellationToken);
            }
            else
            {
                // Unknown or foreign token. Nothing to revoke, and the response is the
                // same either way so sign-out cannot be used to probe token validity.
                logger.LogWarning(
                    "Sign-out presented a refresh token that is unknown or not the caller's. UserId: {UserId}",
                    userId);
            }
        }
        else
        {
            await refreshTokenService.RevokeAllForUserAsync(
                userId,
                RefreshTokenRevocationReason.SignedOut,
                cancellationToken);
        }

        if (currentUser.TokenId is { } tokenId && currentUser.TokenExpiresAt is { } expiresAt)
        {
            await tokenBlacklist.BlacklistAsync(tokenId, expiresAt, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Sign-out completed. UserId: {UserId}", userId);

        return Unit.Value;
    }
}
