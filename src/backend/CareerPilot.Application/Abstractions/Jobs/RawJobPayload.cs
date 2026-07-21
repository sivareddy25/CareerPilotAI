using CareerPilot.Domain.Jobs;

namespace CareerPilot.Application.Abstractions.Jobs;

public sealed record RawJobPayload(
    string ExternalJobId,
    JobProviderKind Source,
    string Title,
    string CompanyName,
    string? CompanyWebsite,
    string Description,
    string? Requirements,
    string? Responsibilities,
    string? Benefits,
    string? Country,
    string? State,
    string? City,
    RemoteType RemoteType,
    EmploymentType EmploymentType,
    ExperienceLevel ExperienceLevel,
    decimal? MinSalary,
    decimal? MaxSalary,
    string Currency,
    string PayPeriod,
    DateTimeOffset PostedAt,
    DateTimeOffset? ExpiresAt,
    string? ApplyUrl,
    string? Language,
    IReadOnlyList<string> Skills,
    IReadOnlyList<string> Tags,
    string? RawJsonMetadata);
