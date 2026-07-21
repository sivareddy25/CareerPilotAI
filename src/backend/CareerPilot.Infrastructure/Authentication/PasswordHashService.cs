using CareerPilot.Application.Abstractions.Authentication;
using CareerPilot.Application.Authentication;
using Microsoft.Extensions.Options;

namespace CareerPilot.Infrastructure.Authentication;

/// <summary>
/// BCrypt password hashing.
/// </summary>
/// <remarks>
/// <para>
/// BCrypt is deliberately slow and its cost is tunable, which is the property that
/// matters: an offline attacker with the hash table gets no faster than the work
/// factor allows, and the factor can be raised as hardware improves without
/// invalidating existing hashes.
/// </para>
/// <para>
/// Salting is not configured because BCrypt does it itself — a per-hash random salt is
/// generated on every call and embedded in the output string, along with the cost
/// factor. That self-describing format is what lets <see cref="Verify"/> and
/// <see cref="NeedsRehash"/> work without storing anything alongside the hash.
/// </para>
/// </remarks>
internal sealed class PasswordHashService : IPasswordHashService
{
    /// <summary>
    /// A real BCrypt hash of a throwaway value, used only by
    /// <see cref="SimulateVerification"/> to spend comparable time on the
    /// unknown-account path. It is never compared against a user's input.
    /// </summary>
    private const string DummyHash =
        "$2a$12$eImiTXuWVxfM37uY4JANjQ.cAoK.a1P0uc/nB0DvV8FhqcRcqXpvS";

    private readonly int _workFactor;

    public PasswordHashService(IOptions<AuthenticationOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        // Clamped rather than trusted. A misconfigured 4 would be trivially brute
        // forced; a misconfigured 20 would make every login take seconds and hand an
        // attacker a denial-of-service vector through the login form itself.
        _workFactor = Math.Clamp(options.Value.PasswordHashWorkFactor, 10, 15);
    }

    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrEmpty(password);

        return BCrypt.Net.BCrypt.HashPassword(password, _workFactor);
    }

    public bool Verify(string password, string passwordHash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordHash))
        {
            return false;
        }

        try
        {
            // BCrypt.Verify compares in constant time with respect to the hash content,
            // so it does not leak how much of the value matched.
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            // A corrupt or non-BCrypt hash. Treated as a failed password rather than an
            // error: an exception here would surface as a 500 and mark out which
            // accounts have malformed hashes.
            return false;
        }
    }

    public bool NeedsRehash(string passwordHash)
    {
        if (string.IsNullOrEmpty(passwordHash))
        {
            return true;
        }

        try
        {
            return BCrypt.Net.BCrypt.PasswordNeedsRehash(passwordHash, _workFactor);
        }
        catch (Exception exception) when (exception is BCrypt.Net.SaltParseException or ArgumentException)
        {
            // Unparseable, so it cannot be a hash at the current factor.
            return true;
        }
    }

    public void SimulateVerification() => BCrypt.Net.BCrypt.Verify("simulated-verification", DummyHash);
}
