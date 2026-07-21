using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Communication.Models;
using CareerPilot.Domain.Communication;

namespace CareerPilot.Application.Communication.Queries;

public sealed record GetRecruiterInboxQuery(Guid UserId, EmailCategory? Category = null) : IQuery<IReadOnlyList<RecruiterThreadDto>>;

internal sealed class GetRecruiterInboxQueryHandler : IQueryHandler<GetRecruiterInboxQuery, IReadOnlyList<RecruiterThreadDto>>
{
    public Task<IReadOnlyList<RecruiterThreadDto>> Handle(GetRecruiterInboxQuery query, CancellationToken cancellationToken)
    {
        var threadId1 = Guid.NewGuid();
        var threadId2 = Guid.NewGuid();

        IReadOnlyList<RecruiterThreadDto> threads = new List<RecruiterThreadDto>
        {
            new(
                threadId1,
                query.UserId,
                Guid.NewGuid(),
                "Interview Invitation: Senior Full Stack Engineer at TechCorp Systems",
                "TechCorp Systems",
                "Sarah Jenkins",
                "sarah.jenkins@techcorp.com",
                EmailCategory.InterviewInvitation,
                "Interview Invitation",
                EmailPriority.High,
                true,
                DateTimeOffset.UtcNow.AddHours(-2),
                new List<EmailMessageDto>
                {
                    new(
                        Guid.NewGuid(),
                        threadId1,
                        "sarah.jenkins@techcorp.com",
                        "Sarah Jenkins",
                        "Hi Alex, We reviewed your resume and would love to invite you for a 45-minute Technical System Design interview on Thursday at 2:00 PM EST. Please confirm if this time works for you.",
                        DateTimeOffset.UtcNow.AddHours(-2),
                        "https://meet.google.com/abc-defg-hij",
                        DateTimeOffset.UtcNow.AddDays(2).AddHours(4),
                        "EST",
                        new List<string> { "Confirm interview availability for Thursday 2:00 PM EST", "Review System Design architecture guidelines" })
                },
                new List<string> { "Confirm interview availability for Thursday 2:00 PM EST" }),

            new(
                threadId2,
                query.UserId,
                Guid.NewGuid(),
                "Online Technical Assessment — DataFlow Systems",
                "DataFlow Systems",
                "Recruiting Team",
                "careers@dataflow.io",
                EmailCategory.CodingAssessment,
                "Coding Assessment",
                EmailPriority.Urgent,
                true,
                DateTimeOffset.UtcNow.AddDays(-1),
                new List<EmailMessageDto>
                {
                    new(
                        Guid.NewGuid(),
                        threadId2,
                        "careers@dataflow.io",
                        "DataFlow Recruiting Team",
                        "Hello Alex, You have been invited to complete a 90-minute online coding assessment. The link expires in 48 hours.",
                        DateTimeOffset.UtcNow.AddDays(-1),
                        "https://hacker-rank.com/test-id-12345",
                        null,
                        null,
                        new List<string> { "Complete 90-min Coding Assessment before Friday midnight" })
                },
                new List<string> { "Complete 90-min Coding Assessment before Friday midnight" })
        };

        if (query.Category.HasValue)
        {
            threads = threads.Where(t => t.Category == query.Category.Value).ToList();
        }

        return Task.FromResult(threads);
    }
}
