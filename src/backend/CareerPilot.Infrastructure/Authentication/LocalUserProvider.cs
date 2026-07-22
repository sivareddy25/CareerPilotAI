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
        try
        {
            var profile = await dbContext.UserProfiles
                .Include(p => p.Skills)
                .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

            if (profile == null)
            {
                profile = UserProfile.CreateFor(userId);
                dbContext.UserProfiles.Add(profile);
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            var hasDotNetOrAngular = profile.Skills.Any(s => s.Name.Equals(".NET", StringComparison.OrdinalIgnoreCase) || s.Name.Equals("Angular", StringComparison.OrdinalIgnoreCase));

            if (!hasDotNetOrAngular || string.IsNullOrWhiteSpace(profile.TargetJobTitles))
            {
                await dbContext.Database.ExecuteSqlRawAsync(
                    "UPDATE user_profiles SET target_job_titles = {0}, work_authorization = {1}, display_name = {2} WHERE id = {3}",
                    ".NET Full Stack Developer, Angular Developer, C# Software Engineer, Full Stack Engineer",
                    "US Citizen",
                    "Venkata Sivareddy Ganjikunta",
                    profile.Id);

                await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM profile_skill WHERE profile_id = {0}", profile.Id);

                var skills = new[] { ".NET", "C#", "ASP.NET Core", "Angular", "TypeScript", "SQL", "Entity Framework", "REST API", "Microservices", "Docker" };
                foreach (var skill in skills)
                {
                    await dbContext.Database.ExecuteSqlRawAsync(
                        "INSERT INTO profile_skill (id, profile_id, name, years_of_experience) VALUES ({0}, {1}, {2}, 5)",
                        Guid.NewGuid(), profile.Id, skill);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Local profile seeding warning handled safely.");
        }
    }

    private async Task EnsureDotNetJobsSeededAsync(CancellationToken cancellationToken)
    {
        var count = await dbContext.Jobs.CountAsync(j => j.Title.Contains(".NET") || j.Title.Contains("Angular") || j.Title.Contains("C#"), cancellationToken);
        if (count >= 10)
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

        var vanguard = await dbContext.Companies.FirstOrDefaultAsync(c => c.Name == "Vanguard Financial Tech", cancellationToken);
        if (vanguard == null)
        {
            vanguard = Company.Create("Vanguard Financial Tech", "https://vanguard.com", "https://vanguard.com/careers", "FinTech", "Global investment management systems.");
            dbContext.Companies.Add(vanguard);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var fidelity = await dbContext.Companies.FirstOrDefaultAsync(c => c.Name == "Fidelity Investments", cancellationToken);
        if (fidelity == null)
        {
            fidelity = Company.Create("Fidelity Investments", "https://fidelity.com", "https://jobs.fidelity.com", "FinTech", "Financial services technology.");
            dbContext.Companies.Add(fidelity);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var slalom = await dbContext.Companies.FirstOrDefaultAsync(c => c.Name == "Slalom Consulting", cancellationToken);
        if (slalom == null)
        {
            slalom = Company.Create("Slalom Consulting", "https://slalom.com", "https://slalom.com/careers", "Consulting", "Enterprise digital platform solutions.");
            dbContext.Companies.Add(slalom);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var accenture = await dbContext.Companies.FirstOrDefaultAsync(c => c.Name == "Accenture Technology", cancellationToken);
        if (accenture == null)
        {
            accenture = Company.Create("Accenture Technology", "https://accenture.com", "https://accenture.com/careers", "IT Services", "Global software transformation.");
            dbContext.Companies.Add(accenture);
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
                "hash-dotnet-103"),

            Job.Create(
                "dotnet-job-104",
                JobProviderKind.LinkedIn,
                "Senior C# / .NET / Angular Enterprise Architect",
                vanguard.Id,
                "Vanguard is seeking a Senior C# / .NET / Angular Enterprise Architect to lead financial technology transformation. Key tech stack: C#, .NET 9, ASP.NET Core, Angular, TypeScript, Entity Framework, REST API, SQL Server, Microservices, and Azure.",
                "Expert level C#, .NET Core, Angular, TypeScript, Entity Framework, Microservices, SQL Server, Web APIs.",
                "Architect scalable microservice backend APIs and Angular enterprise frontends.",
                "Bonus package, 401k match up to 10%, health, dental, tuition reimbursement.",
                Location.Create("United States", "PA", "Malvern", RemoteType.Hybrid),
                SalaryRange.Create(160000m, 205000m, "USD", "Yearly"),
                EmploymentType.FullTime,
                ExperienceLevel.SeniorLevel,
                DateTimeOffset.UtcNow,
                null,
                "https://www.linkedin.com/jobs/view/senior-dotnet-angular-architect-vanguard",
                "en",
                null,
                "hash-dotnet-104"),

            Job.Create(
                "dotnet-job-105",
                JobProviderKind.LinkedIn,
                "Principal .NET & Angular Full Stack Engineer",
                fidelity.Id,
                "Fidelity Technology is looking for a Principal Full Stack Engineer with strong experience in .NET Core, C#, Angular, TypeScript, Entity Framework, REST APIs, SQL Server, and Docker.",
                "5+ years of C#, ASP.NET Core, Angular, TypeScript, Entity Framework, SQL Server, Microservices.",
                "Deliver enterprise web applications and API microservices supporting millions of active trade transactions.",
                "Generous salary, stock options, remote work flexibility, wellness stipend.",
                Location.Create("United States", "NC", "Raleigh", RemoteType.Remote),
                SalaryRange.Create(150000m, 190000m, "USD", "Yearly"),
                EmploymentType.FullTime,
                ExperienceLevel.Lead,
                DateTimeOffset.UtcNow,
                null,
                "https://www.linkedin.com/jobs/view/principal-dotnet-angular-fidelity",
                "en",
                null,
                "hash-dotnet-105"),

            Job.Create(
                "dotnet-job-106",
                JobProviderKind.LinkedIn,
                "Senior Full Stack Developer (.NET 9 / Angular 18 / SQL)",
                slalom.Id,
                "Slalom is building Next-Gen enterprise web portals using C#, .NET 9, ASP.NET Core, Angular 18, TypeScript, Entity Framework Core, Microservices, and REST API.",
                "C#, .NET, ASP.NET Core, Angular, TypeScript, SQL Server, Entity Framework, REST API, Docker.",
                "Collaborate with client stakeholders to build modern Angular frontend UIs and resilient C# backend REST microservices.",
                "Paid certification training, profit-sharing, full insurance benefits.",
                Location.Create("United States", "IL", "Chicago", RemoteType.Remote),
                SalaryRange.Create(140000m, 180000m, "USD", "Yearly"),
                EmploymentType.FullTime,
                ExperienceLevel.SeniorLevel,
                DateTimeOffset.UtcNow,
                null,
                "https://www.linkedin.com/jobs/view/full-stack-dotnet-angular-slalom",
                "en",
                null,
                "hash-dotnet-106"),

            Job.Create(
                "dotnet-job-107",
                JobProviderKind.LinkedIn,
                "Full Stack Engineer (.NET Core / Angular / Microservices)",
                accenture.Id,
                "Accenture Digital Engineering is seeking a Full Stack Engineer specialized in .NET Core, C#, Angular, TypeScript, Entity Framework, REST API, Docker, and SQL.",
                "Core experience with .NET, C#, ASP.NET Core, Angular, TypeScript, Entity Framework, SQL, Docker, Microservices.",
                "Build cloud-native microservices and Angular web apps for global enterprise clients.",
                "Global mobility programs, health & dental, learning credits.",
                Location.Create("United States", "WA", "Seattle", RemoteType.Hybrid),
                SalaryRange.Create(138000m, 175000m, "USD", "Yearly"),
                EmploymentType.FullTime,
                ExperienceLevel.MidLevel,
                DateTimeOffset.UtcNow,
                null,
                "https://www.linkedin.com/jobs/view/fullstack-dotnet-angular-accenture",
                "en",
                null,
                "hash-dotnet-107")
        };

        foreach (var j in dotnetJobs)
        {
            var exists = await dbContext.Jobs.AnyAsync(x => x.ExternalJobId == j.ExternalJobId && x.Source == j.Source, cancellationToken);
            if (!exists)
            {
                j.SetSkills([".NET", "C#", "ASP.NET Core", "Angular", "TypeScript", "SQL", "Entity Framework", "REST API", "Microservices", "Docker"]);
                j.SetTags([".NET", "Angular", "Full Stack", "C#"]);
                dbContext.Jobs.Add(j);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
