using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Dashboard.Models;

namespace CareerPilot.Application.Dashboard.Queries;

public sealed record GetNotificationsQuery(Guid UserId) : IQuery<IReadOnlyList<NotificationDto>>;

internal sealed class GetNotificationsQueryHandler : IQueryHandler<GetNotificationsQuery, IReadOnlyList<NotificationDto>>
{
    public Task<IReadOnlyList<NotificationDto>> Handle(GetNotificationsQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyList<NotificationDto> notifications = new List<NotificationDto>
        {
            new(Guid.NewGuid(), query.UserId, "Upcoming Interview", "System Design interview with TechCorp Systems in 2 days.", "Reminder", false, DateTimeOffset.UtcNow.AddHours(-1), "/jobs"),
            new(Guid.NewGuid(), query.UserId, "New Job Match", "3 new high-score matches added to your dashboard.", "Info", false, DateTimeOffset.UtcNow.AddHours(-4), "/jobs"),
            new(Guid.NewGuid(), query.UserId, "Resume Optimized", "AI Optimization completed for Software Engineer role.", "System", true, DateTimeOffset.UtcNow.AddDays(-1), "/resumes/templates")
        };

        return Task.FromResult(notifications);
    }
}
