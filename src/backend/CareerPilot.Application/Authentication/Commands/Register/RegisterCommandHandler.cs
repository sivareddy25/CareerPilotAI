using CareerPilot.Application.Abstractions.Authentication;
using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Abstractions.Persistence;
using CareerPilot.Application.Authentication.Models;
using CareerPilot.Application.Exceptions;
using CareerPilot.Domain.Entities.Identity;
using Microsoft.Extensions.Logging;

// Aliased because the authentication DTO in this file's own namespace is also called
// UserProfile. The two are genuinely different things — one is the projection returned
// by /auth/me, the other the profile aggregate — and the alias keeps that distinction
// visible at the point of use.
using ProfileAggregate = CareerPilot.Domain.Entities.Profiles.UserProfile;
using Microsoft.Extensions.Options;

namespace CareerPilot.Application.Authentication.Commands.Register;

internal sealed class RegisterCommandHandler(
    IUserRepository users,
    IUserProfileRepository profiles,
    IRoleRepository roles,
    IPasswordHashService passwordHashService,
    AuthenticationSessionFactory sessionFactory,
    IUnitOfWork unitOfWork,
    IOptions<AuthenticationOptions> options,
    ILogger<RegisterCommandHandler> logger)
    : ICommandHandler<RegisterCommand, AuthenticationResult>
{
    private readonly AuthenticationOptions _options = options.Value;

    public async Task<AuthenticationResult> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var normalizedEmail = User.NormalizeEmail(command.Email);

        // Fast path for the ordinary duplicate, giving a clean 409. This check races
        // concurrent registrations by design; the unique index is the real guarantee,
        // and UnitOfWork translates that violation into the same exception.
        if (await users.ExistsByNormalizedEmailAsync(normalizedEmail, cancellationToken))
        {
            logger.LogInformation(
                "Registration rejected: email already registered. NormalizedEmail: {NormalizedEmail}",
                normalizedEmail);

            throw new DuplicateEmailException();
        }

        var user = User.Create(
            command.Email,
            passwordHashService.Hash(command.Password),
            command.FirstName,
            command.LastName);

        var defaultRole = await roles.GetByNormalizedNameAsync(
            Role.Normalize(_options.DefaultRole),
            cancellationToken);

        if (defaultRole is not null)
        {
            user.AssignRole(defaultRole.Id);
        }
        else
        {
            // Fail open on the role, not on the account: the user still gets a working
            // login, just with no permissions until an administrator intervenes. An
            // unseeded database should not block registration outright.
            logger.LogError(
                "Default role {DefaultRole} is missing; registered user has no roles. UserId: {UserId}",
                _options.DefaultRole,
                user.Id);
        }

        users.Add(user);

        // Every account gets a profile at creation, so profile reads never have to
        // handle a missing row and no lazy "create on first access" path exists to get
        // the concurrency wrong.
        profiles.Add(ProfileAggregate.CreateFor(user.Id));

        // Saved before the session is built, and not merely for ordering: the session
        // factory resolves roles and permissions by querying them back, so the role
        // assignment must already be committed or the first access token would be
        // issued with an empty permission set.
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var result = await sessionFactory.CreateAsync(user, cancellationToken);

        // Second save persists the issued refresh token.
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Registration succeeded. UserId: {UserId}", user.Id);

        return result;
    }
}
