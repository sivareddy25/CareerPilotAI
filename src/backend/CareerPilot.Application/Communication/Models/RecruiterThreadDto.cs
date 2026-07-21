using CareerPilot.Domain.Communication;

namespace CareerPilot.Application.Communication.Models;

public sealed record RecruiterThreadDto(
    Guid Id,
    Guid UserId,
    Guid AccountId,
    string Subject,
    string CompanyName,
    string RecruiterName,
    string RecruiterEmail,
    EmailCategory Category,
    string CategoryName,
    EmailPriority Priority,
    bool RequiresReply,
    DateTimeOffset LastMessageAt,
    IReadOnlyList<EmailMessageDto> Messages,
    IReadOnlyList<string> ActionItems);
