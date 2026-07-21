using System.Diagnostics;
using Asp.Versioning;
using CareerPilot.Api.Authorization;
using CareerPilot.Api.Configuration;
using CareerPilot.Api.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;

namespace CareerPilot.Api.Extensions;

/// <summary>
/// Composition entry point for the Api layer: everything HTTP-facing.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddControllers();

        var connectionString = configuration["Database:ConnectionString"]
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? string.Empty;

        var healthChecks = services.AddHealthChecks();
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            healthChecks.AddNpgSql(connectionString, name: "postgresql", tags: ["db", "data"]);
        }

        services.AddApiVersioningSupport();
        services.AddCorsPolicy(configuration);
        services.AddProblemDetailsSupport();
        services.AddOpenApiDocuments();
        services.AddAuthorizationInfrastructure();
        services.AddAuthenticationRateLimiting(configuration);

        services.Configure<RefreshTokenCookieOptions>(
            configuration.GetSection(RefreshTokenCookieOptions.SectionName));

        return services;
    }

    /// <summary>
    /// Authorization only. Authentication — the JWT bearer handler — is registered by
    /// the Infrastructure layer, which owns token issuance and validation together.
    /// </summary>
    private static void AddAuthorizationInfrastructure(this IServiceCollection services)
    {
        // Replaces the default provider so that Permission:{name} policies resolve
        // without being registered one by one. Singleton, matching the framework's own
        // lifetime for this service.
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

        // Scoped, not singleton: it depends on ICurrentUserService and IPermissionService,
        // both of which are per-request. A singleton here would capture the first
        // request's DbContext and hold it for the process lifetime.
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services.AddAuthorization(options => options.AddCareerPilotPolicies());
    }

    private static void AddApiVersioningSupport(this IServiceCollection services)
    {
        services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader(),
                    new HeaderApiVersionReader("x-api-version"));
            })
            .AddMvc()
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });
    }

    private static void AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        var corsOptions = configuration
            .GetSection(CorsOptions.SectionName)
            .Get<CorsOptions>() ?? new CorsOptions();

        services.AddCors(options =>
            options.AddPolicy(CorsOptions.PolicyName, policy =>
            {
                if (corsOptions.AllowedOrigins.Length == 0)
                {
                    return;
                }

                policy.WithOrigins(corsOptions.AllowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();

                if (corsOptions.AllowCredentials)
                {
                    policy.AllowCredentials();
                }
            }));
    }

    private static void AddProblemDetailsSupport(this IServiceCollection services)
    {
        services.AddProblemDetails(options =>
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Instance =
                    $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
                context.ProblemDetails.Extensions["traceId"] =
                    Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
            });

        // Order is precedence: each handler may decline, passing the exception on. The
        // catch-all must stay last.
        services.AddExceptionHandler<ValidationExceptionHandler>();
        services.AddExceptionHandler<AuthenticationExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
    }

    private static void AddOpenApiDocuments(this IServiceCollection services)
    {
        services.AddOpenApi("v1", options =>
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Info = new OpenApiInfo
                {
                    Title = "CareerPilot AI API",
                    Version = "v1",
                    Description = "AI-powered job application automation platform.",
                };

                // Declares the bearer scheme so the generated document — and the Swagger
                // UI's Authorize button — reflect how the API is actually secured.
                // Documentation only: it grants nothing and is never the enforcement point.
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Paste the access token only — Swagger adds the \"Bearer \" prefix.",
                };

                return Task.CompletedTask;
            }));
    }
}
