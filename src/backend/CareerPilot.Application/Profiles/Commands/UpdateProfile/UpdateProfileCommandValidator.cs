using CareerPilot.Application.Profiles.Validation;
using FluentValidation;

namespace CareerPilot.Application.Profiles.Commands.UpdateProfile;

/// <summary>
/// Every field is optional — a profile is meaningful when partly filled in, and forcing
/// a bio or a phone number to save a name change would be hostile. What is enforced is
/// that whatever <i>is</i> supplied is well formed and fits its column.
/// </summary>
internal sealed class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(command => command.FirstName)
            .MaximumLength(ProfileRules.NameMaxLength);

        RuleFor(command => command.LastName)
            .MaximumLength(ProfileRules.NameMaxLength);

        RuleFor(command => command.DisplayName)
            .MaximumLength(ProfileRules.DisplayNameMaxLength);

        RuleFor(command => command.PhoneNumber)
            .MaximumLength(ProfileRules.PhoneMaxLength)
            .OptionalPhone();

        RuleFor(command => command.Country)
            .OptionalCountryCode();

        RuleFor(command => command.State)
            .MaximumLength(ProfileRules.LocationMaxLength);

        RuleFor(command => command.City)
            .MaximumLength(ProfileRules.LocationMaxLength);

        RuleFor(command => command.TimeZone)
            .MaximumLength(ProfileRules.TimeZoneMaxLength)
            .OptionalTimeZone();

        RuleFor(command => command.PreferredLanguage)
            .MaximumLength(ProfileRules.LanguageMaxLength)
            .OptionalLanguageTag();

        RuleFor(command => command.Bio)
            .MaximumLength(ProfileRules.BioMaxLength);

        // Host suffixes are checked on the two fields where the platform is part of the
        // field's meaning — a "LinkedIn URL" pointing anywhere else is a mistake worth
        // catching. Portfolio is any host by definition.
        RuleFor(command => command.LinkedInUrl)
            .MaximumLength(ProfileRules.UrlMaxLength)
            .OptionalWebUrl("linkedin.com");

        RuleFor(command => command.GitHubUrl)
            .MaximumLength(ProfileRules.UrlMaxLength)
            .OptionalWebUrl("github.com");

        RuleFor(command => command.PortfolioUrl)
            .MaximumLength(ProfileRules.UrlMaxLength)
            .OptionalWebUrl();
    }
}
