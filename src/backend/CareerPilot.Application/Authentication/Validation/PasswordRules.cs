using FluentValidation;

namespace CareerPilot.Application.Authentication.Validation;

/// <summary>
/// The single definition of password policy, shared by registration, password change
/// and password reset so the three cannot drift apart.
/// </summary>
public static class PasswordRules
{
    public static IRuleBuilderOptions<T, string> Password<T>(
        this IRuleBuilder<T, string> ruleBuilder,
        AuthenticationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var rule = ruleBuilder
            .NotEmpty().WithMessage("A password is required.")
            .MinimumLength(options.MinimumPasswordLength)
                .WithMessage($"Password must be at least {options.MinimumPasswordLength} characters.")
            // BCrypt ignores everything past 72 bytes. Truncating silently would mean a
            // long passphrase is weaker than the user believes, so it is rejected instead.
            .MaximumLength(options.MaximumPasswordLength)
                .WithMessage($"Password must be at most {options.MaximumPasswordLength} characters.");

        if (options.RequireUppercase)
        {
            rule = rule.Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.");
        }

        if (options.RequireLowercase)
        {
            rule = rule.Matches("[a-z]").WithMessage("Password must contain a lowercase letter.");
        }

        if (options.RequireDigit)
        {
            rule = rule.Matches("[0-9]").WithMessage("Password must contain a digit.");
        }

        if (options.RequireNonAlphanumeric)
        {
            rule = rule.Matches("[^a-zA-Z0-9]").WithMessage("Password must contain a symbol.");
        }

        return rule;
    }

    /// <summary>
    /// Email validation for credentials.
    /// </summary>
    /// <remarks>
    /// FluentValidation 12's <c>EmailAddress()</c> performs a structural check rather
    /// than attempting full RFC 5322 conformance. That is the right trade: the stricter
    /// regexes reject addresses that are legal and deliverable, and the only true test
    /// of an address is sending mail to it.
    ///
    /// The 256-character cap matches the column width, so an over-long address is
    /// refused as a validation error instead of a truncation or a database failure.
    /// </remarks>
    public static IRuleBuilderOptions<T, string> Email<T>(this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder
            .NotEmpty().WithMessage("An email address is required.")
            .MaximumLength(256).WithMessage("Email address must be at most 256 characters.")
            .EmailAddress().WithMessage("Enter a valid email address.");
}
