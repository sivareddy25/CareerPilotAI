namespace CareerPilot.Application.Exceptions;

/// <summary>
/// A resource the caller may legitimately ask for does not exist. Surfaces as 404.
/// </summary>
public class NotFoundException(string message) : Exception(message);

/// <summary>
/// The caller has no profile row.
/// </summary>
/// <remarks>
/// Should not occur in practice — registration creates the profile with the account —
/// so this represents a data-integrity fault rather than an expected outcome. It is a
/// 404 rather than a 500 because the caller can do nothing about it either way, and a
/// stack trace would tell them nothing useful.
/// </remarks>
public sealed class ProfileNotFoundException()
    : NotFoundException("No profile exists for this account.");

/// <summary>
/// The uploaded file was rejected — wrong type, too large, or not actually an image.
/// Surfaces as 400.
/// </summary>
public sealed class InvalidUploadException(string message) : Exception(message);
