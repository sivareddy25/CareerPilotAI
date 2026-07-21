using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Authentication.Models;

namespace CareerPilot.Application.Authentication.Queries.GetCurrentUser;

/// <summary>
/// Returns the authenticated caller's profile. Parameterless by design — the identity
/// comes from the validated token, never from the request.
/// </summary>
public sealed record GetCurrentUserQuery : IQuery<UserProfile>;
