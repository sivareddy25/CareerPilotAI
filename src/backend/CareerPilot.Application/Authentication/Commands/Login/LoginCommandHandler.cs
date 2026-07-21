using CareerPilot.Application.Abstractions.Authentication;
using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Abstractions.Persistence;
using CareerPilot.Application.Authentication.Models;
using CareerPilot.Application.Exceptions;
using CareerPilot.Domain.Entities.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CareerPilot.Application.Authentication.Commands.Login;

/// <summary>
/// Verifies credentials, applies lockout policy, and opens a session.
/// </summary>
/// <remarks>
/// The ordering of checks in <see cref="Handle"/> is the security-critical part of
/// this class and is documented inline. Nothing here ever logs the submitted password
/// or the issued tokens — only the outcome and the account identity.
/// </remarks>
internal sealed class LoginCommandHandler(
    IUserRepository users,
    IPasswordHashService passwordHashService,
    AuthenticationSessionFactory sessionFactory,
    IUnitOfWork unitOfWork,
    IOptions<AuthenticationOptions> options,
    ILogger<LoginCommandHandler> logger)
    : ICommandHandler<LoginCommand, AuthenticationResult>
{
    private readonly AuthenticationOptions _options = options.Value;

    public async Task<AuthenticationResult> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var normalizedEmail = User.NormalizeEmail(command.Email);
        var user = await users.GetByNormalizedEmailAsync(normalizedEmail, cancellationToken);

        if (user is null)
        {
            // Spend the same time an existing account would, then fail identically.
            // See IPasswordHashService.SimulateVerification.
            passwordHashService.SimulateVerification();

            logger.LogWarning(
                "Login failed for unknown account. NormalizedEmail: {NormalizedEmail}",
                normalizedEmail);

            throw new InvalidCredentialsException();
        }

        var utcNow = DateTime.UtcNow;

        // Password is verified *before* lockout and activation are reported, so that
        // someone without the password learns nothing about the account's state. The
        // cost is one hash on a locked account, which is the point — it keeps the
        // response indistinguishable.
        if (!passwordHashService.Verify(command.Password, user.PasswordHash))
        {
            user.RecordFailedLogin(
                utcNow,
                _options.MaxFailedAccessAttempts,
                TimeSpan.FromMinutes(_options.LockoutMinutes));

            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogWarning(
                "Login failed: incorrect password. UserId: {UserId}, FailedAttempts: {FailedAttempts}, LockedOut: {LockedOut}",
                user.Id,
                user.AccessFailedCount,
                user.IsLockedOut(utcNow));

            throw new InvalidCredentialsException();
        }

        if (user.IsLockedOut(utcNow))
        {
            logger.LogWarning(
                "Login refused: account locked until {LockoutEndsAt:o}. UserId: {UserId}",
                user.LockoutEndsAt,
                user.Id);

            throw new AccountLockedException(user.LockoutEndsAt!.Value);
        }

        if (!user.IsActive)
        {
            logger.LogWarning("Login refused: account inactive. UserId: {UserId}", user.Id);
            throw new AccountInactiveException();
        }

        // Transparent hash upgrade: the password is in hand and known correct, so this
        // is the only moment a stored hash can be moved to a stronger cost factor.
        if (passwordHashService.NeedsRehash(user.PasswordHash))
        {
            user.UpgradePasswordHash(passwordHashService.Hash(command.Password));
            logger.LogInformation("Password hash upgraded to current work factor. UserId: {UserId}", user.Id);
        }

        user.RecordSuccessfulLogin(utcNow);

        var result = await sessionFactory.CreateAsync(user, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Login succeeded. UserId: {UserId}", user.Id);

        return result;
    }
}
