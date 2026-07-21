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

        // 5. Uploaded files, served *before* the authorization middleware.
        //
        //    The position is forced by two things. First, an <img> element cannot send
        //    an Authorization header, so an avatar behind bearer authentication simply
        //    would not render. Second, this application sets a fallback authorization
        //    policy, and that policy is applied to requests which match no endpoint —
        //    static files included — so anything served after UseAuthorization() comes
        //    back 401.
        //
        //    These files are therefore public, and their protection is that the names
        //    are random GUIDs: a URL is unguessable, and it is only ever disclosed to
        //    the profile's owner. If avatars later need true access control, the answer
        //    is short-lived signed URLs from the storage provider, not moving this line.
        app.UseUploadedFiles();

        // 6. Rate limiting before authentication, so that a flood of sign-in attempts
        //    is rejected without ever paying for a BCrypt verification. Placing it
        //    after would let an attacker force the expensive work anyway.
        app.UseRateLimiter();

        // 7. Authentication establishes *who* the caller is; authorization then decides
        //    what they may do. The order is mandatory — authorization inspects the
        //    principal that authentication produces, and reversing them means every
        //    request is evaluated as anonymous.
        //    Both modes register a scheme — bearer for SaaS, an auto-authenticating local
        //    one for Local — so this is unconditional. Skipping it in Local mode leaves
        //    controllers carrying [Authorize] metadata with no middleware to honour it,
        //    which throws on every request rather than allowing it through.
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
