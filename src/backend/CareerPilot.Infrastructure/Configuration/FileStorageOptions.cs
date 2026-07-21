namespace CareerPilot.Infrastructure.Configuration;

/// <summary>
/// Settings for the local file storage provider.
/// </summary>
/// <remarks>
/// Only <see cref="LocalFileStorageService"/> reads these. A cloud provider would bind
/// its own options type; nothing in the Application layer references either, which is
/// what keeps the provider swappable.
/// </remarks>
public sealed class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    /// <summary>
    /// Directory that uploads are written beneath, absolute or relative to the content
    /// root.
    /// </summary>
    /// <remarks>
    /// Deliberately defaults outside <c>wwwroot</c>. Writing user-supplied files into a
    /// directory the host serves and executes from is how an upload becomes remote code
    /// execution; this path is served by an explicitly configured, non-executing static
    /// file mapping instead.
    /// </remarks>
    public string RootPath { get; set; } = "App_Data/uploads";

    /// <summary>
    /// URL prefix the stored files are reachable under. Point this at a CDN when one
    /// exists — nothing else has to change.
    /// </summary>
    public string PublicBaseUrl { get; set; } = "/media";
}
