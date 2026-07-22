namespace CareerPilot.Domain.Jobs;

public enum EmploymentType
{
    FullTime = 0,
    PartTime = 1,
    Contract = 2,
    Internship = 3,
    Temporary = 4,
    Freelance = 5,
}

public enum ExperienceLevel
{
    EntryLevel = 0,
    MidLevel = 1,
    SeniorLevel = 2,
    Lead = 3,
    Executive = 4,
    Director = 5,
}

public enum RemoteType
{
    Onsite = 0,
    Hybrid = 1,
    Remote = 2,
}

public enum JobProviderKind
{
    Greenhouse = 0,
    Lever = 1,
    Ashby = 2,
    Workday = 3,
    SmartRecruiters = 4,
    CareerPage = 5,
    Custom = 6,
    LinkedIn = 7,
}

public enum JobStatus
{
    Active = 0,
    Expired = 1,
    Filled = 2,
    Draft = 3,
    Removed = 4,
}
