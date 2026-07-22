using CareerPilot.Domain.Jobs;

namespace CareerPilot.Application.Jobs.Models;

public sealed record LocationDto(
    string Country,
    string? State,
    string? City,
    RemoteType RemoteType,
    string DisplayLocation);

public sealed record SalaryRangeDto(
    decimal? MinSalary,
    decimal? MaxSalary,
    string Currency,
    string PayPeriod,
    string FormattedRange);

public sealed record JobDto(
    Guid Id,
    string ExternalJobId,
    JobProviderKind Source,
    string SourceName,
    string Title,
    string Slug,
    CompanyDto Company,
    string Description,
    string? Requirements,
    string? Responsibilities,
    string? Benefits,
    LocationDto Location,
    SalaryRangeDto Salary,
    EmploymentType EmploymentType,
    ExperienceLevel ExperienceLevel,
    JobStatus Status,
    DateTimeOffset PostedAt,
    DateTimeOffset? ExpiresAt,
    string? ApplyUrl,
    string Language,
    IReadOnlyList<string> Skills,
    IReadOnlyList<string> Tags,
    DateTimeOffset LastSynchronizedAt,
    // Null when the caller has no scorable career profile. A number here is the deterministic
    // match; the component breakdown and matched skills ride alongside for the "why" panel.
    int? MatchScore = null,
    IReadOnlyList<string>? MatchedSkills = null,
    IReadOnlyList<MatchComponentDto>? MatchComponents = null,
    string? MatchSummary = null);

/// <summary>One dimension of a job's match score, for the "why" breakdown.</summary>
public sealed record MatchComponentDto(string Name, int Score, string Detail);
