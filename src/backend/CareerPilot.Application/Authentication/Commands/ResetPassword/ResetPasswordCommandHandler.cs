using CareerPilot.Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging;

namespace CareerPilot.Application.Authentication.Commands.ResetPassword;

/// <summary>
/// Placeholder for password reset completion.
/// </summary>
/// <remarks>
/// <para>
/// Not wired to an HTTP endpoint, and it throws rather than returning success. There
/// is no reset-token store in this phase, so there is nothing to validate the supplied
/// token against.
/// </para>
/// <para>
/// Failing closed is the whole point. A stub that silently succeeded, or that changed
/// the password without checking the token, would be an unauthenticated account
/// takeover on any address an attacker can name. Throwing makes the gap loud instead
/// of exploitable — and the companion
/// <see cref="ForgotPassword.ForgotPasswordCommandHandler"/> never issues a token, so
/// no legitimate caller can reach this path.
/// </para>
/// </remarks>
internal sealed class ResetPasswordCommandHandler(ILogger<ResetPasswordCommandHandler> logger)
    : ICommandHandler<ResetPasswordCommand, Unit>
{
    public Task<Unit> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        logger.LogError("Password reset attempted, but reset tokens are not implemented in this phase.");

        throw new NotSupportedException(
            "Password reset is not available: reset-token issuance and validation are not implemented.");
    }
}
