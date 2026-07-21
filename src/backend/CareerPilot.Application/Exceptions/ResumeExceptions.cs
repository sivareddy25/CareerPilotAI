namespace CareerPilot.Application.Exceptions;

/// <summary>
/// The resume does not exist, or does not belong to the caller.
/// </summary>
/// <remarks>
/// One exception covers both, and the message does not distinguish them. Returning 404
/// for "not yours" and 403 for "exists but not yours" would let anyone enumerate which
/// resume ids are real by watching which status code comes back.
/// </remarks>
public sealed class ResumeNotFoundException()
    : NotFoundException("The resume was not found.");

/// <summary>An uploaded file was rejected before parsing. Surfaces as 400.</summary>
public sealed class UnsupportedResumeFormatException(string message)
    : InvalidUploadException(message);

/// <summary>The account already holds the maximum number of resumes. Surfaces as 409.</summary>
public sealed class ResumeQuotaExceededException(int limit)
    : ConflictException($"You can store up to {limit} resumes. Delete one before importing another.");
