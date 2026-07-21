using CareerPilot.Domain.Communication;

namespace CareerPilot.Application.Communication.Models;

public sealed record ReplyDraftDto(
    Guid ThreadId,
    ReplyTone Tone,
    string ToneName,
    string Subject,
    string GeneratedBody,
    IReadOnlyList<string> SuggestedKeyPoints,
    DateTimeOffset GeneratedAt);
