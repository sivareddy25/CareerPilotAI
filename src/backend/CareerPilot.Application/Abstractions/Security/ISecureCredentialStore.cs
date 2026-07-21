namespace CareerPilot.Application.Abstractions.Security;

public interface ISecureCredentialStore
{
    Task SetCredentialAsync(string key, string secret, CancellationToken cancellationToken);
    Task<string?> GetCredentialAsync(string key, CancellationToken cancellationToken);
    Task<bool> DeleteCredentialAsync(string key, CancellationToken cancellationToken);
}
