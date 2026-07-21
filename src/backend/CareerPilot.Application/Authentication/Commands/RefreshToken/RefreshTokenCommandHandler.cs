using CareerPilot.Application.Abstractions.Authentication;
using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Abstractions.Persistence;
using CareerPilot.Application.Authentication.Models;
using CareerPilot.Application.Exceptions;
using CareerPilot.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace CareerPilot.Application.Authentication.Commands.RefreshToken;

/// <summary>
/// Rotates a refresh token, re-checking that the account is still allowed to hold a
/// session before issuing anything.
/// </summary>
/// <remarks>
/// This is the enforcement point for everything the short-lived access token cannot
/// react to on its own — deactivation, lockout, and password changes. An access token
/// stays valid until it expires; refusal here is what stops the session continuing
/// past that point.
/// </remarks>
internal sealed class RefreshTokenCommandHandler(
    IRefreshTokenService refreshTokenService,
    IUserRepository users,
    AuthenticationSessionFactory sessionFactory,
    IUnitOfWork unitOfWork,
    ILogger<RefreshTokenCommandHandler> logger)
    : ICommandHandler<RefreshTokenCommand, AuthenticationResult>
{
    public async Task<AuthenticationResult> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validation = await refreshTokenService.ValidateAsync(command.RefreshToken, cancellationToken);

        // A token that exists but was already rotated means two parties hold it and
        // there is no way to tell which one is calling. Assume the worst and cut the
        // entire family, which signs out the legitimate user too — an inconvenience
        // that is strictly preferable to leaving a live session in an attacker's hands.
        if (validation.Status == RefreshTokenValidationStatus.ReuseDetected && validation.Token is not null)
        {
            logger.LogCritical(
                "Refresh token reuse detected; revoking all sessions. UserId: {UserId}, TokenId: {TokenId}",
                validation.Token.UserId,
                validation.Token.Id);

            await refreshTokenService.RevokeAllForUserAsync(
                validation.Token.UserId,
                RefreshTokenRevocationReason.ReuseDetected,
                cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            throw new InvalidRefreshTokenException();
        }

        if (!validation.IsValid || validation.Token is null)
        {
            logger.LogWarning("Refresh rejected. Status: {Status}", validation.Status);
            throw new InvalidRefreshTokenException();
        }

        var token = validation.Token;
        var user = await users.GetByIdAsync(token.UserId, cancellationToken);

        // The user row is gone or soft-deleted while a token survives. Fail as an
        // invalid token rather than a missing user: the caller gets no confirmation
        // that the account ever existed.
        if (user is null)
        {
            logger.LogWarning("Refresh rejected: no active user for token. UserId: {UserId}", token.UserId);
            throw new InvalidRefreshTokenException();
        }

        var utcNow = DateTime.UtcNow;

        if (!user.IsActive || user.IsLockedOut(utcNow))
        {
            logger.LogWarning(
                "Refresh rejected: account not eligible. UserId: {UserId}, IsActive: {IsActive}, LockedOut: {LockedOut}",
                user.Id,
                user.IsActive,
                user.IsLockedOut(utcNow));

            await refreshTokenService.RevokeAllForUserAsync(
                user.Id,
                RefreshTokenRevocationReason.RevokedByAdministrator,
                cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            throw new InvalidRefreshTokenException();
        }

        var rotated = await refreshTokenService.RotateAsync(token, cancellationToken);
        var result = await sessionFactory.CreateFromRotatedAsync(user, rotated, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Refresh succeeded. UserId: {UserId}", user.Id);

        return result;
    }
}
