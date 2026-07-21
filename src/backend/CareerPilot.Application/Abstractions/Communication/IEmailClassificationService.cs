using CareerPilot.Application.Communication.Models;
using CareerPilot.Domain.Communication;

namespace CareerPilot.Application.Abstractions.Communication;

public interface IEmailClassificationService
{
    EmailCategory Classify(string subject, string body);
    EmailPriority AssignPriority(EmailCategory category, string body);
    IReadOnlyList<string> ExtractActionItems(string body);
}

public interface IReplyGenerationService
{
    Task<ReplyDraftDto> GenerateReplyDraftAsync(
        RecruiterThreadDto thread,
        ReplyTone tone,
        string? additionalInstructions,
        CancellationToken cancellationToken);
}
