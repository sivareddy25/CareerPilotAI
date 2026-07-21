using CareerPilot.Application.Abstractions.Authentication;
using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Abstractions.Persistence;
using CareerPilot.Application.Exceptions;
using CareerPilot.Application.Profiles.Models;
using CareerPilot.Domain.Enums;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CareerPilot.Application.Profiles.Commands.UpdatePreferences;

/// <summary>
/// Updates display and notification preferences.
/// </summary>
/// <remarks>
/// Separate from <c>UpdateProfileCommand</c> because the two are edited on different
/// screens and at different moments. Folding them together would mean the appearance
/// settings page had to round-trip the user's entire profile — including their bio and
/// links — to change a theme, and any staleness in that payload would silently
/// overwrite a concurrent edit made elsewhere.
/// </remarks>
public sealed record UpdatePreferencesCommand(
    ThemePreference Theme,
    DateFormatPreference DateFormat,
    TimeFormatPreference TimeFormat,
    bool EmailNotifications,
    bool InAppNotifications,
    bool MarketingEmails,
    bool WeeklySummaryEmails) : ICommand<PreferencesDto>;

internal sealed class UpdatePreferencesCommandValidator : AbstractValidator<UpdatePreferencesCommand>
{
    public UpdatePreferencesCommandValidator()
    {
        // IsInEnum matters more than it looks: model binding will happily deserialise
        // an out-of-range integer into an enum without complaint, so an unchecked value
        // reaches the database and then the client as a theme that does not exist.
        RuleFor(command => command.Theme).IsInEnum();
        RuleFor(command => command.DateFormat).IsInEnum();
        RuleFor(command => command.TimeFormat).IsInEnum();
    }
}

internal sealed class UpdatePreferencesCommandHandler(
    ICurrentUserService currentUser,
    IUserProfileRepository profiles,
    IUnitOfWork unitOfWork,
    ILogger<UpdatePreferencesCommandHandler> logger)
    : ICommandHandler<UpdatePreferencesCommand, PreferencesDto>
{
    public async Task<PreferencesDto> Handle(
        UpdatePreferencesCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (currentUser.UserId is not { } userId)
        {
            throw new InvalidCredentialsException();
        }

        var profile = await profiles.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new ProfileNotFoundException();

        profile.Preferences.Update(command.Theme, command.DateFormat, command.TimeFormat);

        profile.Preferences.UpdateNotifications(
            command.EmailNotifications,
            command.InAppNotifications,
            command.MarketingEmails,
            command.WeeklySummaryEmails);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Preferences updated. UserId: {UserId}", userId);

        return PreferencesDto.From(profile.Preferences);
    }
}
