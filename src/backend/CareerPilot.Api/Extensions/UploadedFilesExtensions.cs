using CareerPilot.Infrastructure.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace CareerPilot.Api.Extensions;

/// <summary>
/// Serves uploaded files over HTTP.
/// </summary>
internal static class UploadedFilesExtensions
{
    public static WebApplication UseUploadedFiles(this WebApplication app)
    {
        var options = app.Services.GetRequiredService<IOptions<FileStorageOptions>>().Value;

        var rootPath = Path.IsPathRooted(options.RootPath)
            ? options.RootPath
            : Path.Combine(app.Environment.ContentRootPath, options.RootPath);

        rootPath = Path.GetFullPath(rootPath);

        // Created eagerly: PhysicalFileProvider throws if the directory is absent, which
        // would take the whole application down on a clean deployment that has not
        // received an upload yet.
        Directory.CreateDirectory(rootPath);

        // Only meaningful for the local provider. Behind a CDN or object store the URLs
        // point elsewhere and this middleware simply never matches.
        if (!options.PublicBaseUrl.StartsWith('/'))
        {
            return app;
        }

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(rootPath),
            RequestPath = options.PublicBaseUrl.TrimEnd('/'),

            // Unknown types are not served at all. The default would refuse them too,
            // but stating it makes the intent explicit: only the image types the
            // upload path admits should ever come back out.
            ServeUnknownFileTypes = false,

            OnPrepareResponse = context =>
            {
                var headers = context.Context.Response.Headers;

                // nosniff is the control that matters here. Without it a browser may
                // ignore the declared Content-Type and re-interpret a crafted file as
                // HTML or script, which turns an avatar into stored XSS on the API's
                // own origin.
                headers.XContentTypeOptions = "nosniff";

                // A defence-in-depth backstop: even if something non-image were served,
                // this policy forbids script execution and plugin content.
                headers.ContentSecurityPolicy = "default-src 'none'; img-src 'self'; sandbox";

                // Names are random GUIDs, so a URL is effectively unguessable and the
                // content is immutable — a replacement gets a new name. Long-lived
                // caching is safe and avoids re-fetching avatars on every page.
                headers.CacheControl = "public, max-age=31536000, immutable";
            },
        });

        return app;
    }
}
