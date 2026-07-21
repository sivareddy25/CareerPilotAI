namespace CareerPilot.Application.Abstractions.Authentication;

/// <summary>
/// Hashes and verifies passwords. The only component in the system permitted to see
/// a plaintext password, and it never returns one.
/// </summary>
public interface IPasswordHashService
{
    /// <summary>Produces a self-describing hash that embeds its own salt and cost factor.</summary>
    string Hash(string password);

    /// <summary>
    /// Constant-time verification against a stored hash. Returns <c>false</c> rather
    /// than throwing on a malformed hash, so a corrupted row cannot be distinguished
    /// from a wrong password by an attacker watching responses.
    /// </summary>
    bool Verify(string password, string passwordHash);

    /// <summary>
    /// True when <paramref name="passwordHash"/> was produced with a cost factor below
    /// the current policy, so it can be transparently upgraded on next successful login.
    /// </summary>
    bool NeedsRehash(string passwordHash);

    /// <summary>
    /// Burns the same work as a real <see cref="Verify"/> without comparing anything.
    /// </summary>
    /// <remarks>
    /// Called on the "no such user" path. Skipping the hash there would make a
    /// nonexistent account answer measurably faster than a real one with a wrong
    /// password, turning response latency into an account-enumeration oracle. This
    /// keeps the two paths indistinguishable.
    /// </remarks>
    void SimulateVerification();
}
