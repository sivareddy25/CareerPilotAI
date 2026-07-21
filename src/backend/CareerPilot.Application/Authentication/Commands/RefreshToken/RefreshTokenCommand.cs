using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Authentication.Models;

namespace CareerPilot.Application.Authentication.Commands.RefreshToken;

/// <summary>
/// Exchanges a refresh token for a new access/refresh pair.
/// </summary>
/// <remarks>
/// Takes no user id. Identity is derived from the token alone — accepting a caller
/// supplied id would let anyone with any valid token mint tokens for another account.
/// </remarks>
public sealed record RefreshTokenCommand(string RefreshToken) : ICommand<AuthenticationResult>;
