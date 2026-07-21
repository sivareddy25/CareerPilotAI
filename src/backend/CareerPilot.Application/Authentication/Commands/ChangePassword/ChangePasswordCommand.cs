using CareerPilot.Application.Abstractions.Messaging;

namespace CareerPilot.Application.Authentication.Commands.ChangePassword;

/// <summary>
/// Changes the authenticated caller's password.
/// </summary>
/// <remarks>
/// Carries no user id: the account is always the caller's own, taken from the
/// validated token. <paramref name="CurrentPassword"/> is required even though the
/// caller is authenticated, so that a stolen access token alone cannot be used to take
/// over the account.
/// </remarks>
public sealed record ChangePasswordCommand(string CurrentPassword, string NewPassword) : ICommand<Unit>;
