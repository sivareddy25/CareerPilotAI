namespace CareerPilot.Domain.Enums;

/// <summary>
/// Theme preference. Mirrors the values the Angular <c>ThemeService</c> already
/// understands, so the stored value can be applied by the client without translation.
/// </summary>
public enum ThemePreference
{
    System = 0,
    Light = 1,
    Dark = 2,
    HighContrast = 3,
}

/// <summary>
/// Date presentation preference.
/// </summary>
/// <remarks>
/// An enum rather than a free-text format string on purpose. A user-supplied format
/// reaches a formatter, and arbitrary format strings are both a correctness hazard and
/// an injection surface in some renderers. A closed set is rendered by the client from
/// a lookup it controls.
/// </remarks>
public enum DateFormatPreference
{
    /// <summary>2026-07-21</summary>
    IsoYearMonthDay = 0,

    /// <summary>21/07/2026</summary>
    DayMonthYear = 1,

    /// <summary>07/21/2026</summary>
    MonthDayYear = 2,

    /// <summary>21 Jul 2026</summary>
    DayMonthNameYear = 3,
}

public enum TimeFormatPreference
{
    TwentyFourHour = 0,
    TwelveHour = 1,
}
