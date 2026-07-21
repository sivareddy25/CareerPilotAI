using CareerPilot.Application.Abstractions.Messaging;

namespace CareerPilot.Application.Authentication.Commands.ResetPassword;

/// <summary>
/// Completes a password reset. Placeholder — see
/// <see cref="ResetPasswordCommandHandler"/>.
/// </summary>
public sealed record ResetPasswordCommand(string Email, string Token, string NewPassword) : ICommand<Unit>;
