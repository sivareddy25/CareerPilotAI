using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CareerPilot.Infrastructure.Persistence.ValueConverters;

/// <summary>
/// Ensures every <see cref="DateTime"/> stored in and read from PostgreSQL
/// carries <see cref="DateTimeKind.Utc"/>.
/// </summary>
public sealed class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter()
        : base(
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
    {
    }
}
