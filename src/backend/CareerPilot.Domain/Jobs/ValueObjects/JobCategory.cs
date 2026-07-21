namespace CareerPilot.Domain.Jobs.ValueObjects;

public sealed record JobCategory
{
    public string Name { get; init; } = "Engineering";
    public string Code { get; init; } = "ENG";

    public static JobCategory Create(string name, string? code = null) =>
        new()
        {
            Name = string.IsNullOrWhiteSpace(name) ? "General" : name.Trim(),
            Code = string.IsNullOrWhiteSpace(code) ? name[..Math.Min(name.Length, 4)].ToUpperInvariant() : code.Trim(),
        };
}
