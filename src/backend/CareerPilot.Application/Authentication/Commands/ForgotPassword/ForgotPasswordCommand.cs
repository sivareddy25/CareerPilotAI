using CareerPilot.Application.Abstractions.Messaging;

namespace CareerPilot.Application.Authentication.Commands.ForgotPassword;

/// <summary>
/// Begins a password reset. Placeholder — see
/// <see cref="ForgotPasswordCommandHandler"/> for what is and is not implemented.
/// </summary>
public sealed record ForgotPasswordCommand(string Email) : ICommand<Unit>;
