using CareerPilot.Domain.Entities.Identity;

namespace CareerPilot.Application.Authentication.Models;

/// <summary>
/// The publicly safe projection of a user.
/// </summary>
/// <remarks>
/// Constructed explicitly rather than by mapping the entity wholesale, so that
/// <see cref="User.PasswordHash"/> and <see cref="User.SecurityStamp"/> cannot reach a
/// response by accident when the entity later grows a field.
/// </remarks>
public sealed record UserProfile(
    Guid Id,
    string Email,
    string? FirstName,
    string? LastName,
    bool EmailConfirmed,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions)
{
    public static UserProfile From(
        User user,
        IReadOnlyCollection<string> roles,
        IReadOnlyCollection<string> permissions) =>
        new(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.EmailConfirmed,
            roles,
            permissions);
}
