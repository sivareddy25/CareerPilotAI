using CareerPilot.Domain.Entities.Identity;
using CareerPilot.Domain.Entities.Profiles;
using CareerPilot.Domain.Jobs;
using CareerPilot.Domain.Jobs.Entities;
using CareerPilot.Domain.Jobs.ValueObjects;
using CareerPilot.Infrastructure.Configuration;
using CareerPilot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CareerPilot.Infrastructure.Authentication;

public sealed class LocalUserProvider(
    ApplicationDbContext dbContext,
    IOptions<HostingOptions> options,
    ILogger<LocalUserProvider> logger)
{
    public static readonly Guid DefaultLocalUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public async Task<Guid> EnsureLocalUserCreatedAsync(CancellationToken cancellationToken = default)
    {
        var hostingOptions = options.Value;
        var existingUser = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == hostingOptions.DefaultLocalUserEmail, cancellationToken);

        if (existingUser != null)
        {
            await EnsureLocalUserProfileSeededAsync(existingUser.Id, cancellationToken);
            await EnsureDotNetJobsSeededAsync(cancellationToken);
            return existingUser.Id;
        }

        logger.LogInformation("Local Mode: Auto-provisioning default local user profile ({Email}).", hostingOptions.DefaultLocalUserEmail);

        var localUser = User.Create(
            hostingOptions.DefaultLocalUserEmail,
            "LocalUserHashPassword123!",
            hostingOptions.DefaultLocalUserName);

        typeof(User).GetProperty(nameof(User.Id))?.SetValue(localUser, DefaultLocalUserId);

        dbContext.Users.Add(localUser);
        await dbContext.SaveChangesAsync(cancellationToken);

        await EnsureLocalUserProfileSeededAsync(DefaultLocalUserId, cancellationToken);
        await EnsureDotNetJobsSeededAsync(cancellationToken);

        return localUser.Id;
    }

    private async Task EnsureLocalUserProfileSeededAsync(Guid userId, CancellationToken cancellationToken)
    {
        var profile = await dbContext.UserProfiles
            .Include(p => p.Skills)
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (profile == null)
        {
            profile = UserProfile.CreateFor(userId);
            dbContext.UserProfiles.Add(profile);
        }

        if (profile.Skills.Count == 0 || string.IsNullOrWhiteSpace(profile.TargetJobTitles))
        {
            var defaultSkills = new (string Name, int? Years)[]
            {
                (".NET", 5),
                ("C#", 5),
                ("ASP.NET Core", 5),
                ("Angular", 4),
                ("TypeScript", 4),
                ("SQL", 5),
                ("Entity Framework", 5),
                ("REST API", 5),
                ("Microservices", 3),
                ("Docker", 3)
            };

            profile.CompleteOnboarding(
                "Venkata Sivareddy Ganjikunta",
                "+1 (555) 019-2834",
                "https://www.linkedin.com/in/venkata-sivareddy/",
                "https://github.com/sivareddy25",
                "https://github.com/sivareddy25",
                "US Citizen",
                "$140,000 / year",
                ".NET Full Stack Developer, Angular Developer, C# Software Engineer, Full Stack Engineer");

            profile.SetCareerProfile(
                5,
                140000m,
                "USD",
                EmploymentType.FullTime,
                RemoteType.Hybrid,
                ".NET Full Stack Developer, Angular Developer, C# Software Engineer, Full Stack Engineer",
                defaultSkills);

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task EnsureDotNetJobsSeededAsync(CancellationToken cancellationToken)
    {
        var hasDotNetJobs = await dbContext.Jobs.AnyAsync(j => j.Title.Contains(".NET") || j.Title.Contains("Angular"), cancellationToken);
        if (hasDotNetJobs)
        {
            return;
        }

        logger.LogInformation("Seeding high-precision .NET Full Stack & Angular Developer job postings...");

        var company = await dbContext.Companies.FirstOrDefaultAsync(c => c.Name == "Microsoft Enterprise Systems", cancellationToken);
        if (company == null)
        {
            company = Company.Create("Microsoft Enterprise Systems", "https://microsoft.com", "https://careers.microsoft.com", "Cloud Technology", "Enterprise software solutions.");
            dbContext.Companies.Add(company);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var dotnetJobs = new[]
        {
            Job.Create(
                "dotnet-job-101",
                JobProviderKind.Greenhouse,
                "Senior .NET Full Stack Developer (Angular / C# / ASP.NET Core)",
                company.Id,
                "We are seeking a Senior .NET Full Stack Developer to build high-scale cloud platforms using C#, ASP.NET Core, Angular 17+, TypeScript, Entity Framework, REST APIs, Microservices, and SQL Server.",
                "5+ years of C#, ASP.NET Core, Angular, TypeScript, SQL Server, Entity Framework, REST APIs, Microservices, Docker.",
                "Design and develop resilient web applications using ASP.NET Core and Angular. Architect REST APIs and SQL databases.",
                "Health, dental, 401(k) matching, flexible PTO, hybrid workspace.",
                Location.Create("United States", "CA", "San Francisco", RemoteType.Remote),
                SalaryRange.Create(145000m, 185000m, "USD", "Yearly"),
                EmploymentType.FullTime,
                ExperienceLevel.SeniorLevel,
                DateTimeOffset.UtcNow,
                null,
                "https://careers.microsoft.com/jobs/dotnet-fullstack-101",
                "en",
                null,
                "hash-dotnet-101"),

            Job.Create(
                "dotnet-job-102",
                JobProviderKind.Lever,
                "Lead .NET Core & Angular Software Engineer",
                company.Id,
                "Join our core engineering division as a Lead .NET & Angular Software Engineer. Work with C#, ASP.NET Core 9, Angular, TypeScript, Microservices, Docker, PostgreSQL, and Azure.",
                "Strong mastery of .NET, C#, ASP.NET Core, Angular, TypeScript, Entity Framework, Web APIs, Microservices, Docker, SQL.",
                "Lead frontend Angular and backend ASP.NET Core architecture. Mentor junior developers and drive CI/CD deployment pipelines.",
                "Competitive salary, equity options, healthcare, learning stipend.",
                Location.Create("United States", "NY", "New York", RemoteType.Hybrid),
                SalaryRange.Create(155000m, 195000m, "USD", "Yearly"),
                EmploymentType.FullTime,
                ExperienceLevel.Lead,
                DateTimeOffset.UtcNow,
                null,
                "https://jobs.lever.co/microsoft/lead-dotnet-angular-102",
                "en",
                null,
                "hash-dotnet-102"),

            Job.Create(
                "dotnet-job-103",
                JobProviderKind.Ashby,
                "Full Stack .NET Developer (Angular / TypeScript / Web APIs)",
                company.Id,
                "High-growth enterprise team looking for a Full Stack .NET Developer skilled in C#, ASP.NET Core, Angular, TypeScript, SQL Server, Entity Framework Core, and RESTful web services.",
                "3+ years with .NET, C#, ASP.NET Core, Angular, TypeScript, HTML/CSS, SQL Server, REST API.",
                "Develop responsive Angular web UI components and robust backend C# REST web services.",
                "Comprehensive benefits, remote work budget, annual bonus.",
                Location.Create("United States", "TX", "Austin", RemoteType.Remote),
                SalaryRange.Create(135000m, 170000m, "USD", "Yearly"),
                EmploymentType.FullTime,
                ExperienceLevel.MidLevel,
                DateTimeOffset.UtcNow,
                null,
                "https://jobs.ashbyhq.com/microsoft/fullstack-dotnet-103",
                "en",
                null,
                "hash-dotnet-103")
        };

        foreach (var j in dotnetJobs)
        {
            j.SetSkills([".NET", "C#", "ASP.NET Core", "Angular", "TypeScript", "SQL", "Entity Framework", "REST API", "Microservices", "Docker"]);
            j.SetTags([".NET", "Angular", "Full Stack", "C#"]);
            dbContext.Jobs.Add(j);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
