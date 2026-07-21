using CareerPilot.Application.Abstractions.Authentication;
using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Abstractions.Persistence;
using CareerPilot.Application.Exceptions;
using CareerPilot.Domain.Enums;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CareerPilot.Application.Profiles.Commands.DeleteAccount;

/// <summary>
/// Soft-deletes the caller's account.
/// </summary>
/// <remarks>
/// <para>
/// Requires the password, and additionally a typed confirmation phrase. The second
/// factor is not security theatre: this is the one irreversible action in the product
/// from the user's point of view, and a misclick should not be able to reach it.
/// </para>
/// <para>
/// Soft, not hard. The row stays so referential integrity and audit history survive,
/// while the global query filter removes it from every read — the account is gone as
/// far as the application is concerned. Because the unique email index is filtered on
/// <c>is_deleted</c>, the address is immediately free to register again.
/// </para>
/// </remarks>
public sealed record DeleteAccountCommand(string CurrentPassword, string Confirmation) : ICommand<Unit>;

internal sealed class DeleteAccountCommandValidator : AbstractValidator<DeleteAccountCommand>
{
    /// <summary>Exact phrase the user must type. Compared ordinally, case-sensitively.</summary>
    public const string RequiredConfirmation = "DELETE";

    public DeleteAccountCommandValidator()
    {
        RuleFor(command => command.CurrentPassword)
            .NotEmpty().WithMessage("Your password is required to delete your account.");

        RuleFor(command => command.Confirmation)
            .Equal(RequiredConfirmation, StringComparer.Ordinal)
            .WithMessage($"Type {RequiredConfirmation} to confirm account deletion.");
    }
}

internal sealed class DeleteAccountCommandHandler(
    ICurrentUserService currentUser,
    IUserRepository users,
    IUserProfileRepository profiles,
    IPasswordHashService passwordHashService,
    IRefreshTokenService refreshTokenService,
    ITokenBlacklist tokenBlacklist,
    IUnitOfWork unitOfWork,
    ILogger<DeleteAccountCommandHandler> logger)
    : ICommandHandler<DeleteAccountCommand, Unit>
{
    public async Task<Unit> Handle(DeleteAccountCommand command, CancellationToken cancellationToken)
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
            logger.LogWarning("Account deletion refused: incorrect password. UserId: {UserId}", userId);
            throw new InvalidCredentialsException();
        }

        var utcNow = DateTime.UtcNow;

        user.MarkDeleted(utcNow, deletedBy: userId.ToString());

        // The profile is soft-deleted explicitly rather than relying on a cascade:
        // cascades apply to hard deletes, and nothing is actually being removed here.
        var profile = await profiles.GetByUserIdAsync(userId, cancellationToken);
        if (profile is not null)
        {
            profile.IsDeleted = true;
            profile.DeletedAt = utcNow;
            profile.DeletedBy = userId.ToString();
        }

        await refreshTokenService.RevokeAllForUserAsync(
            userId,
            RefreshTokenRevocationReason.RevokedByAdministrator,
            cancellationToken);

        if (currentUser.TokenId is { } tokenId && currentUser.TokenExpiresAt is { } expiresAt)
        {
            await tokenBlacklist.BlacklistAsync(tokenId, expiresAt, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Logged at Warning: account deletion is rare, irreversible from the user's
        // perspective, and the first thing anyone looks for when a user reports that
        // their account has vanished.
        logger.LogWarning("Account soft-deleted by its owner. UserId: {UserId}", userId);

        return Unit.Value;
    }
}
