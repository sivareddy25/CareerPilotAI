namespace CareerPilot.Application.Communication.Models;

public sealed record EmailMessageDto(
    Guid Id,
    Guid ThreadId,
    string SenderEmail,
    string SenderName,
    string BodyText,
    DateTimeOffset ReceivedAt,
    string? MeetingUrl,
    DateTimeOffset? ExtractedInterviewDate,
    string? ExtractedTimeZone,
    IReadOnlyList<string> ActionItems);
