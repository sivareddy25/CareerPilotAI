namespace CareerPilot.Domain.Jobs.ValueObjects;

public sealed record SalaryRange
{
    public decimal? MinSalary { get; init; }
    public decimal? MaxSalary { get; init; }
    public string Currency { get; init; } = "USD";
    public string PayPeriod { get; init; } = "Yearly"; // Yearly, Monthly, Hourly

    public static SalaryRange Create(decimal? min, decimal? max, string currency = "USD", string payPeriod = "Yearly") =>
        new()
        {
            MinSalary = min,
            MaxSalary = max,
            Currency = string.IsNullOrWhiteSpace(currency) ? "USD" : currency.Trim().ToUpperInvariant(),
            PayPeriod = string.IsNullOrWhiteSpace(payPeriod) ? "Yearly" : payPeriod.Trim(),
        };

    public string FormattedRange => (MinSalary, MaxSalary) switch
    {
        (decimal min, decimal max) => $"{Currency} {min:N0} - {max:N0} / {PayPeriod}",
        (decimal min, null) => $"From {Currency} {min:N0} / {PayPeriod}",
        (null, decimal max) => $"Up to {Currency} {max:N0} / {PayPeriod}",
        _ => "Salary not specified"
    };
}
