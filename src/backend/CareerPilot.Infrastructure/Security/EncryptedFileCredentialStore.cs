using CareerPilot.Application.Abstractions.Security;

namespace CareerPilot.Infrastructure.Security;

public sealed class EncryptedFileCredentialStore : ISecureCredentialStore
{
    private readonly Dictionary<string, string> _inMemoryVault = new();

    public Task SetCredentialAsync(string key, string secret, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        _inMemoryVault[key] = secret;
        return Task.CompletedTask;
    }

    public Task<string?> GetCredentialAsync(string key, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        _inMemoryVault.TryGetValue(key, out var val);
        return Task.FromResult(val);
    }

    public Task<bool> DeleteCredentialAsync(string key, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        var removed = _inMemoryVault.Remove(key);
        return Task.FromResult(removed);
    }
}
