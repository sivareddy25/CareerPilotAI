namespace CareerPilot.Domain.Jobs.ValueObjects;

public sealed record Location
{
    public string Country { get; init; } = "United States";
    public string? State { get; init; }
    public string? City { get; init; }
    public RemoteType RemoteType { get; init; } = RemoteType.Onsite;

    public static Location Create(string country, string? state, string? city, RemoteType remoteType) =>
        new()
        {
            Country = string.IsNullOrWhiteSpace(country) ? "United States" : country.Trim(),
            State = state?.Trim(),
            City = city?.Trim(),
            RemoteType = remoteType,
        };

    public string DisplayLocation => RemoteType switch
    {
        RemoteType.Remote => "Remote",
        RemoteType.Hybrid => $"Hybrid - {City ?? State ?? Country}",
        _ => string.IsNullOrWhiteSpace(City) ? Country : $"{City}, {State ?? Country}"
    };
}
