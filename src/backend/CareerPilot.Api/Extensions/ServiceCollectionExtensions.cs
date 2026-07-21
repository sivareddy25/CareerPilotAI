using System.Diagnostics;
using Asp.Versioning;
using CareerPilot.Api.Configuration;
using CareerPilot.Api.Middleware;
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
        services.AddHealthChecks();

        services.AddApiVersioningSupport();
        services.AddCorsPolicy(configuration);
        services.AddProblemDetailsSupport();
        services.AddOpenApiDocuments();

        return services;
    }

    private static void AddApiVersioningSupport(this IServiceCollection services)
    {
        services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);

                // Unversioned requests resolve to v1 rather than 400ing, so existing
                // clients keep working when v2 appears.
                options.AssumeDefaultVersionWhenUnspecified = true;

                // Advertises supported/deprecated versions in response headers.
                options.ReportApiVersions = true;

                // URL segment is canonical; the header is a fallback for clients
                // that cannot vary their path.
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader(),
                    new HeaderApiVersionReader("x-api-version"));
            })
            .AddMvc()
            .AddApiExplorer(options =>
            {
                // Produces group names of the form "v1", which the OpenAPI document
                // name matches — that is what keeps each version's endpoints grouped.
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
                // No configured origins means no cross-origin access. Failing closed
                // matters more than developer convenience here.
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

                // Correlates the client-visible error with the server log entry.
                context.ProblemDetails.Extensions["traceId"] =
                    Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
            });

        // Order is the contract: most specific first, terminal handler last.
        services.AddExceptionHandler<ValidationExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
    }

    private static void AddOpenApiDocuments(this IServiceCollection services)
    {
        // Document name "v1" matches the ApiExplorer group name, so v1 endpoints
        // land in the v1 document. Add a document per version as versions are added.
        services.AddOpenApi("v1", options =>
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Info = new OpenApiInfo
                {
                    Title = "CareerPilot AI API",
                    Version = "v1",
                    Description = "AI-powered job application automation platform.",
                };

                return Task.CompletedTask;
            }));
    }
}
