using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CareerPilot.Api.Middleware;

/// <summary>
/// Terminal exception handler: converts anything unhandled into an RFC 7807
/// response. Registered last, so more specific handlers get first refusal.
/// </summary>
internal sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Structured: the message template's placeholders become queryable fields
        // rather than being flattened into an interpolated string.
        logger.LogError(
            exception,
            "Unhandled exception processing {RequestMethod} {RequestPath}",
            httpContext.Request.Method,
            httpContext.Request.Path.Value);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        // Detail is intentionally generic. Exception text can carry connection
        // strings and PII, so it goes to logs, never to the client.
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred.",
                Detail = "The request could not be processed. Quote the traceId when reporting this.",
                Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
            },
        });
    }
}
