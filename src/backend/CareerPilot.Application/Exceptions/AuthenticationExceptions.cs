namespace CareerPilot.Application.Exceptions;

/// <summary>
/// Base for every failure that should surface as HTTP 401.
/// </summary>
/// <remarks>
/// The message on these is written for the client and is intentionally vague. Detail
/// about *why* authentication failed belongs in the log, never in the response — see
/// <see cref="InvalidCredentialsException"/>.
/// </remarks>
public abstract class AuthenticationException(string message) : Exception(message);

/// <summary>
/// Wrong password, unknown account, or an account that may not sign in.
/// </summary>
/// <remarks>
/// One exception covers all of these on purpose. Distinguishing "no such user" from
/// "wrong password" hands an attacker a free account-enumeration oracle: they learn
/// which addresses are registered without ever guessing a password correctly.
/// </remarks>
public sealed class InvalidCredentialsException()
    : AuthenticationException("The email address or password is incorrect.");

/// <summary>
/// Raised only after the password was verified as correct, so it reveals nothing to
/// someone guessing — they would have needed the password to see it.
/// </summary>
public sealed class AccountLockedException(DateTime lockoutEndsAt)
    : AuthenticationException("The account is temporarily locked following repeated failed sign-in attempts.")
{
    public DateTime LockoutEndsAt { get; } = lockoutEndsAt;
}

/// <summary>Also raised only post-verification, for the same reason.</summary>
public sealed class AccountInactiveException()
    : AuthenticationException("The account is not active. Contact an administrator.");

/// <summary>
/// The presented refresh token was unknown, expired, revoked, or already rotated.
/// The distinction is logged and acted on server-side but never returned.
/// </summary>
public sealed class InvalidRefreshTokenException()
    : AuthenticationException("The refresh token is invalid or has expired.");

/// <summary>
/// A state conflict the caller could resolve — surfaces as HTTP 409.
/// </summary>
public class ConflictException(string message) : Exception(message);

/// <summary>
/// Registration against an address that already exists.
/// </summary>
/// <remarks>
/// This one *is* an enumeration oracle, and it is an accepted trade-off: a
/// registration form that silently succeeds on a duplicate is unusable. The exposure
/// is contained by rate limiting the endpoint rather than by obscuring the message.
/// </remarks>
public sealed class DuplicateEmailException()
    : ConflictException("An account with this email address already exists.");
