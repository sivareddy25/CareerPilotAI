using System.Text;
using CareerPilot.Application.Abstractions.Authentication;
using CareerPilot.Application.Authentication;
using CareerPilot.Infrastructure.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace CareerPilot.Infrastructure.Authentication;

/// <summary>
/// Registers the authentication services and the JWT bearer handler.
/// </summary>
internal static class AuthenticationRegistration
{
    public static IServiceCollection AddAuthenticationInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.Configure<AuthenticationOptions>(configuration.GetSection(AuthenticationOptions.SectionName));

        services.AddHttpContextAccessor();
        services.AddMemoryCache();

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();

        // Singletons: both are stateless apart from configuration read once at
        // construction. PasswordHashService in particular resolves and clamps its work
        // factor in the constructor, which should not happen per request.
        services.AddSingleton<IPasswordHashService, PasswordHashService>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<ITokenBlacklist, TokenBlacklist>();

        services.AddJwtBearerAuthentication(configuration, environment);

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
                // Relaxed only for a local HTTP dev host. Every other environment
                // requires transport security.
                options.RequireHttpsMetadata = !environment.IsDevelopment();

                // The token is returned in the response body and held by the client;
                // retaining a server-side copy in the auth properties serves no purpose
                // and only widens what a memory dump would expose.
                options.SaveToken = false;

                // Keep claim names exactly as issued. With mapping on, "sub" silently
                // becomes a ClaimTypes.NameIdentifier URI and lookups by the name we
                // wrote stop matching.
                options.MapInboundClaims = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,

                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),

                    // Pinned to the one algorithm actually used. Without this, a token
                    // is accepted on any algorithm the library supports, which is how
                    // algorithm-confusion attacks get their foothold.
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256],

                    ValidateLifetime = true,

                    // Default is five minutes of grace. That silently triples the life
                    // of a 15-minute token past revocation, so it is removed; both ends
                    // are the same process and their clocks agree.
                    ClockSkew = TimeSpan.Zero,

                    NameClaimType = JwtRegisteredClaimNames.Sub,
                    RoleClaimType = AuthenticationClaimTypes.Role,
                };

                options.Events = new JwtBearerEvents
                {
                    // Signature and expiry have passed by this point. The remaining
                    // question is whether the token was explicitly revoked, which the
                    // token itself cannot express.
                    OnTokenValidated = async context =>
                    {
                        var services = context.HttpContext.RequestServices;
                        var blacklist = services.GetRequiredService<ITokenBlacklist>();

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

                        // Exception type only. The message can echo token content, and
                        // the token is a credential — it must not reach the log.
                        logger.LogWarning(
                            "Bearer token rejected: {FailureType}",
                            context.Exception.GetType().Name);

                        return Task.CompletedTask;
                    },
                };
            });
    }

}
