using CareerPilot.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CareerPilot.Api.Middleware;

/// <summary>
/// Maps authentication and conflict failures to their proper status codes.
/// </summary>
/// <remarks>
/// Without this they would reach <see cref="GlobalExceptionHandler"/> and surface as
/// 500s, which would tell a client nothing and would log a wrong password as a server
/// fault. The detail returned is the exception's own message, and those messages are
/// written to be safe for a client to read — see the exception definitions for why
/// each is worded as it is.
/// </remarks>
internal sealed class AuthenticationExceptionHandler(
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            AuthenticationException => (StatusCodes.Status401Unauthorized, "Authentication failed."),
            ConflictException => (StatusCodes.Status409Conflict, "The request conflicts with existing data."),
            NotFoundException => (StatusCodes.Status404NotFound, "The requested resource was not found."),
            InvalidUploadException => (StatusCodes.Status400BadRequest, "The uploaded file was rejected."),
            // Placeholder commands that fail closed rather than pretending to work.
            NotSupportedException => (StatusCodes.Status501NotImplemented, "This operation is not available."),
            _ => (0, string.Empty),
        };

        if (status == 0)
        {
            return false;
        }

        httpContext.Response.StatusCode = status;

        // WWW-Authenticate is what tells a client the credential was the problem, and
        // it is the signal the Angular interceptor keys on to attempt a silent refresh.
        if (status == StatusCodes.Status401Unauthorized)
        {
            httpContext.Response.Headers.WWWAuthenticate = "Bearer";
        }

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = exception.Message,
            },
        });
    }
}
