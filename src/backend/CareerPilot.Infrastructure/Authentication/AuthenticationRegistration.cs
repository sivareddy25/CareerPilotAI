using System.Text;
using CareerPilot.Application.Abstractions.Authentication;
using CareerPilot.Application.Abstractions.Security;
using CareerPilot.Application.Authentication;
using CareerPilot.Domain.Configuration;
using CareerPilot.Infrastructure.Configuration;
using CareerPilot.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using AuthenticationSchemeOptions = Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace CareerPilot.Infrastructure.Authentication;

internal static class AuthenticationRegistration
{
    public static IServiceCollection AddAuthenticationInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.Configure<AuthenticationOptions>(configuration.GetSection(AuthenticationOptions.SectionName));
        services.Configure<HostingOptions>(configuration.GetSection(HostingOptions.SectionName));

        var hostingOptions = configuration.GetSection(HostingOptions.SectionName).Get<HostingOptions>() ?? new HostingOptions();

        services.AddHttpContextAccessor();
        services.AddMemoryCache();

        services.AddSingleton<ISecureCredentialStore, EncryptedFileCredentialStore>();

        // Token and password primitives are hosting-mode independent. The register/login
        // command handlers depend on them and are discovered by assembly scanning in both
        // modes, so scoping these to SaaS leaves the container unable to construct them.
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddSingleton<IPasswordHashService, PasswordHashService>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<ITokenBlacklist, TokenBlacklist>();

        // What *does* vary by mode: who the current user is, how permissions resolve,
        // and whether incoming requests are authenticated by a bearer scheme at all.
        if (hostingOptions.IsLocalMode)
        {
            services.AddScoped<ICurrentUserService, LocalCurrentUserService>();
            services.AddScoped<IPermissionService, LocalPermissionService>();
            services.AddScoped<LocalUserProvider>();

            services
                .AddAuthentication(LocalAuthenticationHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, LocalAuthenticationHandler>(
                    LocalAuthenticationHandler.SchemeName,
                    configureOptions: null);
        }
        else
        {
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IPermissionService, PermissionService>();

            services.AddJwtBearerAuthentication(configuration, environment);
        }

        return services;
    }

    private static void AddJwtBearerAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = !environment.IsDevelopment();
                options.SaveToken = false;
                options.MapInboundClaims = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,

                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),

                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,

                    NameClaimType = JwtRegisteredClaimNames.Sub,
                    RoleClaimType = AuthenticationClaimTypes.Role,
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var servicesProvider = context.HttpContext.RequestServices;
                        var blacklist = servicesProvider.GetRequiredService<ITokenBlacklist>();
                        var tokenId = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

                        if (!string.IsNullOrEmpty(tokenId) &&
                            await blacklist.IsBlacklistedAsync(tokenId, context.HttpContext.RequestAborted))
                        {
                            context.Fail("The token has been revoked.");
                        }
                    },

                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger("CareerPilot.Authentication");

                        logger.LogWarning(
                            "Bearer token rejected: {FailureType}",
                            context.Exception.GetType().Name);

                        return Task.CompletedTask;
                    },
                };
            });
    }
}
