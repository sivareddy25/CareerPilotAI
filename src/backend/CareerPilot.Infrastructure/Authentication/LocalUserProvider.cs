using CareerPilot.Domain.Entities.Identity;
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

        return localUser.Id;
    }
}
