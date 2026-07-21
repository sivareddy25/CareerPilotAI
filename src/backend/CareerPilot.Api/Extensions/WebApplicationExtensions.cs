using CareerPilot.Api.Configuration;

namespace CareerPilot.Api.Extensions;

/// <summary>
/// Builds the HTTP request pipeline. Middleware order is behaviour, not style —
/// each stage is placed where it can actually do its job.
/// </summary>
public static class WebApplicationExtensions
{
    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        // 1. Outermost. Only middleware registered *after* the exception handler is
        //    covered by it, so nothing may precede it.
        app.UseExceptionHandler();

        // 2. Development-only API documentation. Never exposed in other environments:
        //    the schema is a map of the attack surface.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi().AllowAnonymous();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/openapi/v1.json", "CareerPilot AI API v1");
                options.RoutePrefix = "swagger";
                options.DocumentTitle = "CareerPilot AI API";
            });
        }

        // 3. Upgrade before anything reads the request body, so credentials are
        //    never processed over cleartext.
        app.UseHttpsRedirection();

        // 4. Routing must precede CORS: the CORS middleware inspects the matched
        //    endpoint's metadata to decide policy.
        app.UseRouting();

        app.UseCors(CorsOptions.PolicyName);

        // 5. Rate limiting before authentication, so that a flood of sign-in attempts
        //    is rejected without ever paying for a BCrypt verification. Placing it
        //    after would let an attacker force the expensive work anyway.
        app.UseRateLimiter();

        // 6. Authentication establishes *who* the caller is; authorization then decides
        //    what they may do. The order is mandatory — authorization inspects the
        //    principal that authentication produces, and reversing them means every
        //    request is evaluated as anonymous.
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        // Anonymous explicitly: the fallback policy requires an authenticated user on
        // every endpoint that says nothing, and an orchestrator's health probe has no
        // token to present.
        app.MapHealthChecks("/health").AllowAnonymous();

        return app;
    }
}
