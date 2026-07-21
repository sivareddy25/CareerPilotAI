using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Authentication.Models;

namespace CareerPilot.Application.Authentication.Commands.Login;

/// <summary>
/// Authenticates an email/password pair and opens a session.
/// </summary>
/// <remarks>
/// A command rather than a query despite "reading" credentials: it mutates the failed
/// attempt counter, the last-login timestamp, and issues a refresh token.
/// </remarks>
public sealed record LoginCommand(string Email, string Password) : ICommand<AuthenticationResult>;
