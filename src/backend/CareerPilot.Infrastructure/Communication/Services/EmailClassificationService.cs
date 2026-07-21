using CareerPilot.Application.Abstractions.Communication;
using CareerPilot.Domain.Communication;

namespace CareerPilot.Infrastructure.Communication.Services;

public sealed class EmailClassificationService : IEmailClassificationService
{
    public EmailCategory Classify(string subject, string body)
    {
        var combined = $"{subject} {body}".ToLowerInvariant();

        if (combined.Contains("interview") || combined.Contains("schedule") || combined.Contains("availability"))
            return EmailCategory.InterviewInvitation;
        if (combined.Contains("assessment") || combined.Contains("hackerrank") || combined.Contains("test"))
            return EmailCategory.CodingAssessment;
        if (combined.Contains("offer") || combined.Contains("compensation") || combined.Contains("congratulations"))
            return EmailCategory.Offer;
        if (combined.Contains("regret") || combined.Contains("unfortunately") || combined.Contains("other candidates"))
            return EmailCategory.Rejection;
        if (combined.Contains("follow up") || combined.Contains("checking in"))
            return EmailCategory.FollowUp;

        return EmailCategory.RecruiterOutreach;
    }

    public EmailPriority AssignPriority(EmailCategory category, string body)
    {
        return category switch
        {
            EmailCategory.InterviewInvitation => EmailPriority.High,
            EmailCategory.CodingAssessment => EmailPriority.Urgent,
            EmailCategory.Offer => EmailPriority.Urgent,
            EmailCategory.Rejection => EmailPriority.Low,
            _ => EmailPriority.Normal
        };
    }

    public IReadOnlyList<string> ExtractActionItems(string body)
    {
        var items = new List<string>();
        if (body.Contains("confirm", StringComparison.OrdinalIgnoreCase))
            items.Add("Confirm availability for proposed interview slot");
        if (body.Contains("assessment", StringComparison.OrdinalIgnoreCase))
            items.Add("Complete online coding assessment before deadline");

        if (items.Count == 0)
            items.Add("Review email message and prepare response");

        return items;
    }
}
