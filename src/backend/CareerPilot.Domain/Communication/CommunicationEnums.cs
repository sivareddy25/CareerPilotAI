namespace CareerPilot.Domain.Communication;

public enum CommunicationProviderKind
{
    Microsoft365 = 0,
    Google = 1
}

public enum EmailCategory
{
    GeneralCommunication = 0,
    InterviewInvitation = 1,
    CodingAssessment = 2,
    RecruiterOutreach = 3,
    Offer = 4,
    Rejection = 5,
    FollowUp = 6
}

public enum EmailPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Urgent = 3
}

public enum ReplyTone
{
    Professional = 0,
    Friendly = 1,
    Executive = 2,
    Concise = 3
}
