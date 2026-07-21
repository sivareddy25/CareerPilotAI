using System.Text.RegularExpressions;
using CareerPilot.Domain.Jobs;

namespace CareerPilot.Infrastructure.Jobs.Normalization;

/// <summary>
/// Splits the free-text location string that job boards return into country/state/city.
/// </summary>
/// <remarks>
/// Every board in scope exposes location as one unstructured string — "San Francisco, CA",
/// "Remote - US", "London, United Kingdom" — with no schema behind it. There is no parse that
/// is correct for all of them, so this is explicitly a best-effort heuristic: it recovers the
/// common shapes and leaves the rest as a city with a null region rather than guessing.
///
/// Nothing downstream treats these fields as authoritative; they drive filtering and display,
/// and <see cref="RawJobPayload.RemoteType"/> is the field users actually filter on.
/// </remarks>
internal static partial class JobLocationParser
{
    private const string DefaultCountry = "United States";

    /// <summary>
    /// Two-letter uppercase token in the trailing position, e.g. the "CA" of
    /// "San Francisco, CA". Distinguishes a US state abbreviation from a spelled-out
    /// country name, which is the only ambiguity that actually shows up in board data.
    /// </summary>
    [GeneratedRegex(@"^[A-Z]{2}$")]
    private static partial Regex StateAbbreviation();

    [GeneratedRegex(@"\b(remote|anywhere|distributed|work from home|wfh)\b", RegexOptions.IgnoreCase)]
    private static partial Regex RemoteMarker();

    [GeneratedRegex(@"\b(hybrid|flexible)\b", RegexOptions.IgnoreCase)]
    private static partial Regex HybridMarker();

    internal readonly record struct ParsedLocation(string Country, string? State, string? City, RemoteType RemoteType);

    public static ParsedLocation Parse(string? rawLocation)
    {
        var text = rawLocation?.Trim();

        if (string.IsNullOrEmpty(text))
        {
            return new ParsedLocation(DefaultCountry, null, null, RemoteType.Onsite);
        }

        var remoteType = DetectRemoteType(text);

        // "Remote - New York" and "Remote, US" both carry a real place after the marker.
        // Strip the marker so the geography parse below sees only the location part.
        var geography = RemoteMarker().Replace(text, string.Empty)
            .Trim(' ', '-', ',', '(', ')', '/');

        if (string.IsNullOrEmpty(geography))
        {
            return new ParsedLocation(DefaultCountry, null, null, remoteType);
        }

        var segments = geography
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToArray();

        return segments switch
        {
            // "San Francisco, CA" → city + US state. "London, United Kingdom" → city + country.
            [var city, var tail] => StateAbbreviation().IsMatch(tail)
                ? new ParsedLocation(DefaultCountry, tail, city, remoteType)
                : new ParsedLocation(tail, null, city, remoteType),

            // "Austin, TX, United States"
            [var city, var state, var country, ..] => new ParsedLocation(country, state, city, remoteType),

            // A bare token could be either a city or a country; treating it as a city keeps
            // the country field trustworthy, which is what preference matching filters on.
            [var single] => new ParsedLocation(DefaultCountry, null, single, remoteType),

            _ => new ParsedLocation(DefaultCountry, null, null, remoteType),
        };
    }

    public static RemoteType DetectRemoteType(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return RemoteType.Onsite;
        }

        // Hybrid is checked first: "Hybrid Remote" is a common phrasing and is not fully remote.
        if (HybridMarker().IsMatch(text))
        {
            return RemoteType.Hybrid;
        }

        return RemoteMarker().IsMatch(text) ? RemoteType.Remote : RemoteType.Onsite;
    }
}
