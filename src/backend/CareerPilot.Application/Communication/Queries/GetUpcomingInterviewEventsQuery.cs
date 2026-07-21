using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Communication.Models;

namespace CareerPilot.Application.Communication.Queries;

public sealed record GetUpcomingInterviewEventsQuery(Guid UserId) : IQuery<IReadOnlyList<InterviewEventDto>>;

internal sealed class GetUpcomingInterviewEventsQueryHandler : IQueryHandler<GetUpcomingInterviewEventsQuery, IReadOnlyList<InterviewEventDto>>
{
    public Task<IReadOnlyList<InterviewEventDto>> Handle(GetUpcomingInterviewEventsQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyList<InterviewEventDto> events = new List<InterviewEventDto>
        {
            new(
                Guid.NewGuid(),
                query.UserId,
                "Technical System Design Interview",
                "TechCorp Systems",
                "Senior Full Stack Engineer",
                DateTimeOffset.UtcNow.AddDays(2).AddHours(4),
                DateTimeOffset.UtcNow.AddDays(2).AddHours(5),
                "EST",
                "https://meet.google.com/abc-defg-hij",
                new List<string>
                {
                    "Review C# .NET Clean Architecture principles",
                    "Prepare microservice scaling examples",
                    "Check webcam and microphone"
                })
        };

        return Task.FromResult(events);
    }
}
