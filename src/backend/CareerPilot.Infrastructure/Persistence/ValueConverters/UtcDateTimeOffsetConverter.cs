using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CareerPilot.Infrastructure.Persistence.ValueConverters;

/// <summary>
/// Normalises every <see cref="DateTimeOffset"/> to UTC on the way into the database.
/// </summary>
/// <remarks>
/// Npgsql maps <see cref="DateTimeOffset"/> to <c>timestamptz</c> and rejects any value whose
/// offset is not zero, rather than converting it. Third-party job boards return local offsets
/// as a matter of course — Greenhouse publishes <c>-04:00</c> — so without this every ingested
/// posting fails to save.
///
/// Converting here rather than in each provider keeps it impossible to forget: a new provider
/// gets the behaviour without knowing the rule exists. The instant is preserved; only the
/// offset representation changes, and <c>timestamptz</c> does not store an offset anyway.
/// </remarks>
public sealed class UtcDateTimeOffsetConverter : ValueConverter<DateTimeOffset, DateTimeOffset>
{
    public UtcDateTimeOffsetConverter()
        : base(
            v => v.ToUniversalTime(),
            v => v.ToUniversalTime())
    {
    }
}
