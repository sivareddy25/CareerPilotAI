using CareerPilot.Api.Extensions;
using CareerPilot.Application;
using CareerPilot.Infrastructure;
using CareerPilot.Infrastructure.Authentication;
using CareerPilot.Infrastructure.Configuration;
using CareerPilot.Infrastructure.Persistence;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration, builder.Environment)
    .AddApi(builder.Configuration);

var app = builder.Build();

app.UseApiPipeline();

var hostingOptions = app.Services.GetRequiredService<IOptions<HostingOptions>>().Value;

if (hostingOptions.IsLocalMode)
{
    await app.ApplyLocalDatabaseMigrationsAsync();

    using var scope = app.Services.CreateScope();
    var localUserProvider = scope.ServiceProvider.GetRequiredService<LocalUserProvider>();
    await localUserProvider.EnsureLocalUserCreatedAsync();
}
else
{
    await app.SeedIdentityAsync();
}

app.Run();
