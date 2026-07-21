using CareerPilot.Application.Abstractions.Messaging;

namespace CareerPilot.Application.Authentication.Commands.Logout;

/// <summary>
/// Ends the caller's session.
/// </summary>
/// <param name="RefreshToken">
/// The session to close. When omitted, every active session for the user is revoked —
/// the "sign out everywhere" case.
/// </param>
public sealed record LogoutCommand(string? RefreshToken) : ICommand<Unit>;
