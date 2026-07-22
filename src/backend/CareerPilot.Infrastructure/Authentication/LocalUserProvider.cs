using CareerPilot.Domain.Entities.Identity;
using CareerPilot.Domain.Entities.Profiles;
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
                CareerPilot.Domain.Jobs.EmploymentType.FullTime,
                CareerPilot.Domain.Jobs.RemoteType.Hybrid,
                ".NET Full Stack Developer, Angular Developer, C# Software Engineer, Full Stack Engineer",
                defaultSkills);

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
