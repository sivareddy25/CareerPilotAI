namespace CareerPilot.Application.Communication.Models;

public sealed record InterviewEventDto(
    Guid Id,
    Guid UserId,
    string Title,
    string CompanyName,
    string JobTitle,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    string TimeZone,
    string? MeetingUrl,
    IReadOnlyList<string> PreparationChecklist);
