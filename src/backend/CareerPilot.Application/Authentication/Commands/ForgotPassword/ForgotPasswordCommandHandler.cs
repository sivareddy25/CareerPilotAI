using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Abstractions.Persistence;
using CareerPilot.Domain.Entities.Identity;
using Microsoft.Extensions.Logging;

namespace CareerPilot.Application.Authentication.Commands.ForgotPassword;

/// <summary>
/// Placeholder for password reset initiation.
/// </summary>
/// <remarks>
/// <para>
/// Not wired to an HTTP endpoint. Completing it needs two things this phase does not
/// include: a reset-token store, and an email transport to deliver the link.
/// </para>
/// <para>
/// What is implemented is the response shape, because it is the part that is easy to
/// get wrong later. The handler always reports success, whether or not the address is
/// registered. Any observable difference — a 404, or a faster reply — turns this
/// endpoint into an account-enumeration oracle, and it is an unauthenticated endpoint,
/// so that oracle would be open to everyone.
/// </para>
/// </remarks>
internal sealed class ForgotPasswordCommandHandler(
    IUserRepository users,
    ILogger<ForgotPasswordCommandHandler> logger)
    : ICommandHandler<ForgotPasswordCommand, Unit>
{
    public async Task<Unit> Handle(ForgotPasswordCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var normalizedEmail = User.NormalizeEmail(command.Email);
        var user = await users.GetByNormalizedEmailAsync(normalizedEmail, cancellationToken);

        // The branch exists only to log; both paths return the same result.
        if (user is null)
        {
            logger.LogInformation(
                "Password reset requested for an unregistered address. NormalizedEmail: {NormalizedEmail}",
                normalizedEmail);
        }
        else
        {
            // TODO: issue a single-use, short-lived, hashed reset token and email it.
            logger.LogInformation("Password reset requested. UserId: {UserId}", user.Id);
        }

        return Unit.Value;
    }
}
