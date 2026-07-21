using CareerPilot.Application.Communication.Models;
using CareerPilot.Domain.Communication;

namespace CareerPilot.Application.Abstractions.Communication;

public interface IEmailProvider
{
    CommunicationProviderKind ProviderKind { get; }
    Task<IReadOnlyList<RecruiterThreadDto>> FetchRecruiterThreadsAsync(string accessToken, CancellationToken cancellationToken);
    Task<bool> SendEmailAsync(string accessToken, string toAddress, string subject, string body, CancellationToken cancellationToken);
}
