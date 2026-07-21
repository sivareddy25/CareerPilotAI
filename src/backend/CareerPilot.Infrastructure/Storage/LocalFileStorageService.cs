using CareerPilot.Application.Abstractions.Storage;
using CareerPilot.Application.Exceptions;
using CareerPilot.Infrastructure.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CareerPilot.Infrastructure.Storage;

/// <summary>
/// Stores files on the local disk.
/// </summary>
/// <remarks>
/// <para>
/// The default provider, suitable for development and single-node deployments. It is
/// not suitable behind a load balancer without shared storage — a file written on one
/// node is invisible to the others. Swapping in an Azure Blob or S3 implementation of
/// <see cref="IFileStorageService"/> is the fix, and requires no change above this
/// class.
/// </para>
/// <para>
/// Three things here are security controls rather than conveniences, and none should be
/// removed without a replacement:
/// </para>
/// <list type="number">
/// <item>Names are generated, never taken from the client.</item>
/// <item>The extension is derived from the file's own magic bytes.</item>
/// <item>Every resolved path is verified to sit inside the storage root.</item>
/// </list>
/// </remarks>
internal sealed class LocalFileStorageService : IFileStorageService
{
    /// <summary>
    /// Leading bytes that identify the image formats accepted for upload, mapped to the
    /// extension that will actually be written.
    /// </summary>
    /// <remarks>
    /// Content-Type and file name both come from the client and are worth nothing as
    /// evidence. What a file <i>is</i> can only be established from its contents, and
    /// deriving the stored extension from the sniffed type is what stops a
    /// <c>.html</c> or <c>.svg</c> payload being written under an image's name and
    /// later served back as active content.
    /// </remarks>
    private static readonly (byte[] Signature, string Extension)[] ImageSignatures =
    [
        ([0xFF, 0xD8, 0xFF], ".jpg"),
        ([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A], ".png"),
        ([0x47, 0x49, 0x46, 0x38], ".gif"),
        // WebP is "RIFF....WEBP"; the RIFF prefix is checked here and the WEBP tag at
        // offset 8 is checked separately.
        ([0x52, 0x49, 0x46, 0x46], ".webp"),
    ];

    private readonly FileStorageOptions _options;
    private readonly string _rootPath;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(
        IOptions<FileStorageOptions> options,
        IHostEnvironment environment,
        ILogger<LocalFileStorageService> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(environment);

        _options = options.Value;
        _logger = logger;

        _rootPath = Path.IsPathRooted(_options.RootPath)
            ? _options.RootPath
            : Path.Combine(environment.ContentRootPath, _options.RootPath);

        // Fully resolved once, at construction, so every later containment check
        // compares against a canonical path rather than one with ".." still in it.
        _rootPath = Path.GetFullPath(_rootPath);
    }

    public async Task<StoredFile> SaveAsync(
        string container,
        FileUploadRequest file,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(container);
        ArgumentNullException.ThrowIfNull(file);

        // A container is a name, not a path. Rejecting separators here stops a caller
        // from escaping the storage root through the container argument.
        if (container.Contains('/') || container.Contains('\\') || container.Contains(".."))
        {
            throw new ArgumentException("Container must be a simple name.", nameof(container));
        }

        var extension = await ResolveImageExtensionAsync(file.Content, cancellationToken);

        // The stored name is a fresh GUID. The client's file name never touches the
        // file system, which removes path traversal, overwriting someone else's file,
        // and reserved-name collisions in one stroke.
        var storedName = $"{Guid.NewGuid():N}{extension}";
        var key = $"{container}/{storedName}";

        var directory = Path.Combine(_rootPath, container);
        Directory.CreateDirectory(directory);

        var absolutePath = EnsureWithinRoot(Path.Combine(directory, storedName));

        // CreateNew, not Create: a GUID collision is effectively impossible, but if one
        // occurred, silently overwriting another user's avatar is not an acceptable
        // failure mode.
        await using (var destination = new FileStream(
            absolutePath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            useAsync: true))
        {
            await file.Content.CopyToAsync(destination, cancellationToken);
        }

        _logger.LogInformation("Stored file {Key} ({Bytes} bytes)", key, file.Length);

        return new StoredFile(key, $"{_options.PublicBaseUrl.TrimEnd('/')}/{key}");
    }

    public Task DeleteAsync(string keyOrUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(keyOrUrl))
        {
            return Task.CompletedTask;
        }

        var key = ToKey(keyOrUrl);

        // Anything that is not a plain "container/name" pair did not come from
        // SaveAsync. Refusing to act on it prevents a tampered stored value from
        // steering a delete at an arbitrary file.
        var segments = key.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length != 2 || segments.Any(segment => segment is ".." or "."))
        {
            _logger.LogWarning("Refusing to delete a file with an unexpected key shape.");
            return Task.CompletedTask;
        }

        var absolutePath = EnsureWithinRoot(Path.Combine(_rootPath, segments[0], segments[1]));

        // Idempotent by contract: a missing file means the goal is already met.
        if (File.Exists(absolutePath))
        {
            File.Delete(absolutePath);
            _logger.LogInformation("Deleted file {Key}", key);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Reads the leading bytes and returns the extension for the format they identify.
    /// </summary>
    /// <remarks>
    /// Rewinds the stream afterwards so the caller's subsequent copy still sees the
    /// whole file. A non-seekable stream cannot be sniffed and is rejected rather than
    /// trusted.
    /// </remarks>
    private static async Task<string> ResolveImageExtensionAsync(
        Stream content,
        CancellationToken cancellationToken)
    {
        if (!content.CanSeek)
        {
            throw new InvalidUploadException("The uploaded file could not be inspected.");
        }

        var header = new byte[12];
        content.Seek(0, SeekOrigin.Begin);
        var read = await content.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false, cancellationToken);
        content.Seek(0, SeekOrigin.Begin);

        foreach (var (signature, extension) in ImageSignatures)
        {
            if (read < signature.Length || !header.AsSpan(0, signature.Length).SequenceEqual(signature))
            {
                continue;
            }

            // RIFF alone is a container marker shared with WAV and AVI; the WEBP tag at
            // offset 8 is what makes it an image.
            if (extension == ".webp")
            {
                var isWebP = read >= 12
                    && header[8] == 0x57 && header[9] == 0x45
                    && header[10] == 0x42 && header[11] == 0x50;

                if (!isWebP)
                {
                    continue;
                }
            }

            return extension;
        }

        throw new InvalidUploadException(
            "The file is not a recognised image. Upload a JPEG, PNG, WebP or GIF.");
    }

    /// <summary>
    /// Strips the public URL prefix, leaving the provider-relative key.
    /// </summary>
    /// <remarks>
    /// Accepts either form because the value stored on the profile is a URL, while
    /// <see cref="StoredFile.Key"/> is the key — and the delete path may be handed
    /// either depending on the caller.
    /// </remarks>
    private string ToKey(string keyOrUrl)
    {
        var prefix = _options.PublicBaseUrl.TrimEnd('/');

        var value = keyOrUrl.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? keyOrUrl[prefix.Length..]
            : keyOrUrl;

        return value.TrimStart('/');
    }

    /// <summary>
    /// Final containment check: the fully resolved path must sit inside the storage
    /// root.
    /// </summary>
    /// <remarks>
    /// The generated-name and key-shape checks should make this unreachable. It stays
    /// because it is the control that actually holds if either of those is weakened by
    /// a later change — a traversal that gets this far ends in an exception rather than
    /// a write outside the root.
    /// </remarks>
    private string EnsureWithinRoot(string candidate)
    {
        var resolved = Path.GetFullPath(candidate);

        if (!resolved.StartsWith(_rootPath + Path.DirectorySeparatorChar, StringComparison.Ordinal)
            && !string.Equals(resolved, _rootPath, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Resolved path escapes the storage root.");
        }

        return resolved;
    }
}
