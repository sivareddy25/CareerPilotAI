using CareerPilot.Application.Abstractions.Authentication;
using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Abstractions.Persistence;
using CareerPilot.Application.Exceptions;
using CareerPilot.Domain.Enums;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CareerPilot.Application.Profiles.Commands.DeactivateAccount;

/// <summary>
/// Deactivates the caller's account. Reversible by an administrator.
/// </summary>
/// <remarks>
/// The password is required even though the caller is already authenticated. Account
/// destruction is exactly the action an attacker performs with a stolen access token,
/// and re-authentication is what stops a token alone from being enough.
/// </remarks>
public sealed record DeactivateAccountCommand(string CurrentPassword) : ICommand<Unit>;

internal sealed class DeactivateAccountCommandValidator : AbstractValidator<DeactivateAccountCommand>
{
    public DeactivateAccountCommandValidator()
    {
        RuleFor(command => command.CurrentPassword)
            .NotEmpty().WithMessage("Your password is required to deactivate your account.");
    }
}

internal sealed class DeactivateAccountCommandHandler(
    ICurrentUserService currentUser,
    IUserRepository users,
    IPasswordHashService passwordHashService,
    IRefreshTokenService refreshTokenService,
    ITokenBlacklist tokenBlacklist,
    IUnitOfWork unitOfWork,
    ILogger<DeactivateAccountCommandHandler> logger)
    : ICommandHandler<DeactivateAccountCommand, Unit>
{
    public async Task<Unit> Handle(DeactivateAccountCommand command, CancellationToken cancellationToken)
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
            logger.LogWarning("Deactivation refused: incorrect password. UserId: {UserId}", userId);
            throw new InvalidCredentialsException();
        }

        // Rotates the security stamp as a side effect, so tokens minted before this
        // point stop being renewable.
        user.Deactivate();

        await refreshTokenService.RevokeAllForUserAsync(
            userId,
            RefreshTokenRevocationReason.RevokedByAdministrator,
            cancellationToken);

        if (currentUser.TokenId is { } tokenId && currentUser.TokenExpiresAt is { } expiresAt)
        {
            await tokenBlacklist.BlacklistAsync(tokenId, expiresAt, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Account deactivated by its owner. UserId: {UserId}", userId);

        return Unit.Value;
    }
}
