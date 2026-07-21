using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Authentication.Models;

namespace CareerPilot.Application.Authentication.Commands.Register;

/// <summary>
/// Creates a local account and signs the new user straight in.
/// </summary>
public sealed record RegisterCommand(
    string Email,
    string Password,
    string? FirstName,
    string? LastName) : ICommand<AuthenticationResult>;
