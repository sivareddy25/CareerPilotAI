namespace CareerPilot.Application.Jobs.Models;

public sealed record CompanyDto(
    Guid Id,
    string Name,
    string Slug,
    string? WebsiteUrl,
    string? CareerPageUrl,
    string? LogoUrl,
    string? Industry,
    string? Description);
