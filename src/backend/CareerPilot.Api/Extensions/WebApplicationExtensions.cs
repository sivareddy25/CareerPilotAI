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
            app.MapOpenApi();
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

        // 5. UseAuthentication() / UseAuthorization() belong here, between CORS and
        //    endpoint execution. Not wired in this phase.

        app.MapControllers();
        app.MapHealthChecks("/health");

        return app;
    }
}
