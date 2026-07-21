using CareerPilot.Application.Abstractions.Communication;
using CareerPilot.Application.Communication.Models;
using CareerPilot.Domain.Communication;

namespace CareerPilot.Infrastructure.Communication.Services;

public sealed class ReplyGenerationService : IReplyGenerationService
{
    public Task<ReplyDraftDto> GenerateReplyDraftAsync(
        RecruiterThreadDto thread,
        ReplyTone tone,
        string? additionalInstructions,
        CancellationToken cancellationToken)
    {
        var subject = $"Re: {thread.Subject}";
        var recruiterName = string.IsNullOrWhiteSpace(thread.RecruiterName) ? "Hiring Manager" : thread.RecruiterName;

        string body = tone switch
        {
            ReplyTone.Friendly => $@"Hi {recruiterName},

Thank you so much for reaching out regarding the {thread.Subject}! I am very excited about this opportunity at {thread.CompanyName}.

The proposed time works great for me. I look forward to speaking with the team!

Best regards,
Alex",

            ReplyTone.Executive => $@"Dear {recruiterName},

Thank you for your message concerning {thread.CompanyName}. I appreciate the invitation to discuss the position further.

I confirm my availability for the scheduled discussion and look forward to learning more about the strategic goals for this role.

Sincerely,
Alex",

            ReplyTone.Concise => $@"Hi {recruiterName},

Thank you for reaching out. The proposed time works for me. Looking forward to our conversation!

Thanks,
Alex",

            _ => $@"Dear {recruiterName},

Thank you for reaching out regarding the position at {thread.CompanyName}. I am pleased to confirm my interest and availability for the interview.

Please let me know if you need any additional documents or technical preparation details prior to our meeting.

Best regards,
Alex"
        };

        var draft = new ReplyDraftDto(
            thread.Id,
            tone,
            tone.ToString(),
            subject,
            body,
            new List<string>
            {
                "Acknowledges interview invitation promptly",
                "Confirms time slot availability",
                "Maintains professional tone"
            },
            DateTimeOffset.UtcNow);

        return Task.FromResult(draft);
    }
}
