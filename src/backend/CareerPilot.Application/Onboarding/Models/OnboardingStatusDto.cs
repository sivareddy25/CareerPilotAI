namespace CareerPilot.Application.Onboarding.Models;

public sealed record OnboardingStatusDto(
    bool IsCompleted,
    string? FullName,
    string? Email,
    string? Phone,
    string? LinkedInUrl,
    string? GitHubUrl,
    string? WorkAuthorization,
    string? PreferredSalary,
    string? TargetJobTitles);
