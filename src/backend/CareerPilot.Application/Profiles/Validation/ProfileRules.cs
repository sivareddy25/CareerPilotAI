using System.Globalization;
using System.Text.RegularExpressions;
using FluentValidation;

namespace CareerPilot.Application.Profiles.Validation;

/// <summary>
/// Reusable rules for profile fields, so the same constraint means the same thing
/// wherever it is applied.
/// </summary>
public static partial class ProfileRules
{
    /// <summary>Column widths these must not exceed. Kept beside the rules that enforce them.</summary>
    public const int NameMaxLength = 100;
    public const int DisplayNameMaxLength = 100;
    public const int PhoneMaxLength = 32;
    public const int LocationMaxLength = 100;
    public const int TimeZoneMaxLength = 64;
    public const int LanguageMaxLength = 16;
    public const int BioMaxLength = 1000;
    public const int UrlMaxLength = 512;

    /// <summary>
    /// Digits with optional leading <c>+</c> and common separators.
    /// </summary>
    /// <remarks>
    /// Intentionally permissive. Phone numbering plans vary enormously and a strict
    /// pattern rejects legitimate numbers — which matters because this field is only
    /// ever displayed back to its owner. It is never an identifier, never a login
    /// factor, and never dialled by the system, so the cost of a loose format is
    /// cosmetic while the cost of a strict one is a user who cannot save their profile.
    /// </remarks>
    [GeneratedRegex(@"^\+?[0-9\s\-().]{6,32}$", RegexOptions.CultureInvariant)]
    private static partial Regex PhonePattern();

    /// <summary>BCP 47-shaped: <c>en</c>, <c>en-GB</c>, <c>zh-Hans-CN</c>.</summary>
    [GeneratedRegex(@"^[A-Za-z]{2,3}(-[A-Za-z0-9]{2,8})*$", RegexOptions.CultureInvariant)]
    private static partial Regex LanguagePattern();

    public static IRuleBuilderOptions<T, string?> OptionalPhone<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(value => string.IsNullOrWhiteSpace(value) || PhonePattern().IsMatch(value.Trim()))
            .WithMessage("Enter a valid phone number.");

    /// <summary>
    /// Requires an absolute http(s) URL.
    /// </summary>
    /// <remarks>
    /// The scheme allow-list is the point. These values are rendered as links in the
    /// user's own profile, and permitting <c>javascript:</c> or <c>data:</c> would turn
    /// a profile field into stored XSS the moment a template binds it to an href.
    /// Angular's sanitiser would catch most of it, but relying on the client to undo a
    /// bad value the server chose to store is the wrong place to put the control.
    /// </remarks>
    public static IRuleBuilderOptions<T, string?> OptionalWebUrl<T>(
        this IRuleBuilder<T, string?> rule,
        string? expectedHostSuffix = null) =>
        rule.Must(value =>
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return true;
                }

                if (!Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri))
                {
                    return false;
                }

                if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                {
                    return false;
                }

                return expectedHostSuffix is null
                    || uri.Host.EndsWith(expectedHostSuffix, StringComparison.OrdinalIgnoreCase);
            })
            .WithMessage(expectedHostSuffix is null
                ? "Enter a valid URL starting with http:// or https://."
                : $"Enter a valid {expectedHostSuffix} URL.");

    /// <summary>
    /// Validates an ISO 3166-1 alpha-2 code against the runtime's own region list.
    /// </summary>
    /// <remarks>
    /// Checked against <see cref="RegionInfo"/> rather than a hand-maintained list: a
    /// hard-coded list is wrong the moment a country changes, and nobody remembers to
    /// update it. <c>RegionInfo</c> throws on an unknown code, which is why this is a
    /// try/catch rather than a lookup.
    /// </remarks>
    public static IRuleBuilderOptions<T, string?> OptionalCountryCode<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(value =>
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return true;
                }

                var code = value.Trim();
                if (code.Length != 2)
                {
                    return false;
                }

                try
                {
                    _ = new RegionInfo(code.ToUpperInvariant());
                    return true;
                }
                catch (ArgumentException)
                {
                    return false;
                }
            })
            .WithMessage("Enter a valid two-letter country code.");

    /// <summary>
    /// Validates against the host's time zone database.
    /// </summary>
    /// <remarks>
    /// Accepts whatever the host recognises, which is IANA ids on Linux and containers
    /// and Windows ids on Windows. .NET 6+ converts between the two, so a value stored
    /// on one platform still resolves on the other.
    /// </remarks>
    public static IRuleBuilderOptions<T, string?> OptionalTimeZone<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(value =>
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return true;
                }

                try
                {
                    _ = TimeZoneInfo.FindSystemTimeZoneById(value.Trim());
                    return true;
                }
                catch (Exception exception)
                    when (exception is TimeZoneNotFoundException or InvalidTimeZoneException)
                {
                    return false;
                }
            })
            .WithMessage("Select a valid time zone.");

    public static IRuleBuilderOptions<T, string?> OptionalLanguageTag<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(value => string.IsNullOrWhiteSpace(value) || LanguagePattern().IsMatch(value.Trim()))
            .WithMessage("Enter a valid language tag, for example en or en-GB.");
}
