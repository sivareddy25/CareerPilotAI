namespace CareerPilot.Application.Abstractions.Storage;

/// <summary>A file being handed to storage.</summary>
/// <param name="Content">
/// Read once, forward-only. The caller owns the stream and disposes it.
/// </param>
/// <param name="FileName">Original client-supplied name. Never used as a storage path.</param>
/// <param name="ContentType">Client-declared MIME type. Advisory — see the validator.</param>
public sealed record FileUploadRequest(
    Stream Content,
    string FileName,
    string ContentType,
    long Length);

/// <summary>Where a stored file ended up.</summary>
/// <param name="Key">
/// Provider-relative identifier used for later deletion. The provider's own business —
/// a local path here, an object key elsewhere.
/// </param>
/// <param name="Url">Address a client can fetch the file from.</param>
public sealed record StoredFile(string Key, string Url);

/// <summary>
/// Binary storage, abstracted away from where the bytes actually live.
/// </summary>
/// <remarks>
/// <para>
/// Deliberately narrow: store, delete, resolve a URL. Nothing here exposes a file
/// system path, a <c>FileInfo</c>, or a container client, because anything that leaks a
/// provider concept into this signature becomes a change the Application layer has to
/// make when storage moves. An Azure Blob or S3 implementation satisfies this contract
/// without a single change above it.
/// </para>
/// <para>
/// Note the deliberate absence of a "read file" method. Serving avatars is the web
/// server's job — a static file middleware or a CDN in front of a bucket — and routing
/// bytes back through the application would add nothing but latency.
/// </para>
/// </remarks>
public interface IFileStorageService
{
    /// <summary>
    /// Stores a file under <paramref name="container"/> and returns its location.
    /// </summary>
    /// <param name="container">
    /// Logical grouping, for example <c>profile-pictures</c>. Implementations must
    /// treat this as a name, not a path, and reject separators.
    /// </param>
    Task<StoredFile> SaveAsync(
        string container,
        FileUploadRequest file,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a stored file. Idempotent: deleting something already gone is a no-op,
    /// not an error, so a retried delete cannot fail the request.
    /// </summary>
    Task DeleteAsync(string keyOrUrl, CancellationToken cancellationToken = default);
}
