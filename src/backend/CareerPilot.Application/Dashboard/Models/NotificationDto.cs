namespace CareerPilot.Application.Dashboard.Models;

public sealed record NotificationDto(
    Guid Id,
    Guid UserId,
    string Title,
    string Message,
    string Type,
    bool IsRead,
    DateTimeOffset CreatedAt,
    string? ActionUrl);
