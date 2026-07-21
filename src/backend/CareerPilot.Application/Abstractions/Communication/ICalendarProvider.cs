using CareerPilot.Application.Communication.Models;
using CareerPilot.Domain.Communication;

namespace CareerPilot.Application.Abstractions.Communication;

public interface ICalendarProvider
{
    CommunicationProviderKind ProviderKind { get; }
    Task<IReadOnlyList<InterviewEventDto>> FetchUpcomingEventsAsync(string accessToken, CancellationToken cancellationToken);
}
